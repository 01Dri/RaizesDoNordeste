using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Payments;
using RaizesDoNordeste.Domain.Core.Payments.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.Services;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using UninterPayment.SDK;

namespace RaizesDoNordeste.Application.UseCases.Payments;

[Transactional]
public sealed class PaymentUseCaseHandler : IUseCaseHandler<PaymentRequestDto, PaymentResponseDto>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IValidator<PaymentRequestDto> _validator;
    private readonly ICurrentUser _currentUser;
    private readonly IUninterPaymentClient _uninterPaymentClient;
    private readonly ILoyalityProgramService _loyalityProgramService;
    private readonly IConfiguration _configuration;

    public PaymentUseCaseHandler
    (
        ApplicationDbContext dbContext,
        IValidator<PaymentRequestDto> validator,
        ICurrentUser currentUser,
        IUninterPaymentClient uninterPaymentClient,
        ILoyalityProgramService loyalityProgramService,
        IConfiguration configuration
    )
    {
        _dbContext = dbContext;
        _validator = validator;
        _currentUser = currentUser;
        _uninterPaymentClient = uninterPaymentClient;
        _loyalityProgramService = loyalityProgramService;
        _configuration = configuration;
    }

    // PENSAR EM PONTOS DE FILIDADE PARA DESCONTO E GANHA DE PONTOS.


    /**
      • Paridade de Centavos (Mais comum no mercado):
      • Regra: 100 pontos = R$ 1,00 (ou seja, cada 1 ponto vale R$ 0,01).
      • Cálculo: Desconto = Pontos / 100
      • Exemplo: Se o cliente tem 1500 pontos, ele tem R$ 15,00 de desconto (1500 / 100).
    **/
    public async Task<Result<PaymentResponseDto>> HandleAsync(PaymentRequestDto parameter, CancellationToken cancellation = default)
    {
        var validation = await _validator.ValidateAsync(parameter, cancellation);
        if (validation.ContainsErrors())
        {
            return validation.ToResultFailure<PaymentResponseDto>();
        }

        var order = await _dbContext.Orders
            .Include(x => x.Items).ThenInclude(x => x.MenuItem)
            .FirstOrDefaultAsync(x => x.PublicId == parameter.OrderId  && x.AccountId == _currentUser.AccountId, cancellation);

        if (order == null)
        {
            return Result<PaymentResponseDto>.FailureNotFound("Pedido não encontrado");
        }

        if (order.Status != OrderStatus.Ready)
        {
            return Result<PaymentResponseDto>.Failure(new Error("O pedido precisa estar pronto para ser pago."));
        }

        if (order.Items.Sum(x => x.MenuItem.Price *  x.Quantity) != order.TotalPrice)
        {
            return Result<PaymentResponseDto>.Failure(new Error("Total do pedido está incorreto."));
        }

        var totalToPay = order.TotalPrice;
        bool usedLoyaltyPoints = false;
        if (parameter.UseLoyalityPoints)
        {
            var discountResult = await _loyalityProgramService
                .ApplyDiscountAsync(order.TotalPrice, _currentUser.AccountId, _currentUser.RestaurantId, cancellation);
            
            usedLoyaltyPoints = discountResult.PointsConsumed;
            totalToPay = order.TotalPrice - discountResult.DiscountAmount;
        }

        if (totalToPay < 0)
        {
            return Result<PaymentResponseDto>.Failure(new Error("Total do pedido está incorreto."));
        }

        var sdkMethod = parameter.PaymentMethod.Method == PaymentMethod.Pix 
            ? UninterPaymentMethod.Pix 
            : UninterPaymentMethod.CreditCard;

        var sdkRequest = new UninterPayment.SDK.PaymentRequest
        {
            OrderId = order.PublicId,
            Amount = totalToPay,
            Method = sdkMethod,
            CardNumber = null,
            PixKey = null,
            WebhookUrl = _configuration["PaymentSettings:WebhookUrl"] ?? "http://localhost:5269/pagamento/webhook"
        };

        var sdkResult = await _uninterPaymentClient.ProcessPaymentAsync(sdkRequest, cancellation);

        if (sdkResult.Status == UninterPaymentStatus.Failed)
        {
            return Result<PaymentResponseDto>.Failure(new Error(sdkResult.ErrorMessage ?? "Falha ao processar o pagamento no gateway."));
        }

        var domainStatus = sdkResult.Status == UninterPaymentStatus.Approved 
            ? PaymentStatus.Paid 
            : PaymentStatus.Waiting;

        var payment = new Payment
        {
            Total = order.TotalPrice,
            TotalDiscount = order.TotalPrice - totalToPay,
            TotalPaid = domainStatus == PaymentStatus.Paid ? totalToPay : 0,
            PaymentMethod = parameter.PaymentMethod.Method,
            Status = domainStatus,
            ExternalPaymentId = sdkResult.TransactionId,
            Description = domainStatus == PaymentStatus.Paid
                ? "Aprovado na hora via cartão."
                : "Aguardando confirmação Pix."
        };

        _dbContext.Payments.Add(payment);

        var paymentOrder = new PaymentOrder
        {
            Order = order,
            Payment = payment,
            UsedLoyalityPoints = usedLoyaltyPoints
        };
        _dbContext.PaymentOrders.Add(paymentOrder);

        int loyalityPoints = 0;
        int? totalPointsInRestaurant = null;

        if (domainStatus == PaymentStatus.Paid)
        {
            var earnResult = await _loyalityProgramService
                .EarnPointsAsync(totalToPay, _currentUser.AccountId, _currentUser.RestaurantId, cancellation);
            loyalityPoints = earnResult.PointsAmount;
            totalPointsInRestaurant = earnResult.TotalPointsInRestaurant;
        }
        await _dbContext.SaveChangesAsync(cancellation);

        var responseDto = new PaymentResponseDto
        {
            OrderId = order.PublicId,
            Status = domainStatus,
            AmountPaid = payment.TotalPaid,
            EarnedLoyaliyPoints = loyalityPoints,
            TotalPointsInRestaurant = totalPointsInRestaurant
        };

        return Result<PaymentResponseDto>.Success(responseDto);
    }
}

