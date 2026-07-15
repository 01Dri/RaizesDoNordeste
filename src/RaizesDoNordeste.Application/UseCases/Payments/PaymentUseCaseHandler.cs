using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Payments;
using RaizesDoNordeste.Domain.Core.Payments.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using UninterPayment.SDK;

namespace RaizesDoNordeste.Application.UseCases.Payments;

public sealed class PaymentUseCaseHandler : IUseCaseHandler<PaymentRequestDto, PaymentResponseDto>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IValidator<PaymentRequestDto> _validator;
    private readonly ICurrentUser _currentUser;
    private readonly IUninterPaymentClient _uninterPaymentClient;

    public PaymentUseCaseHandler(
        ApplicationDbContext dbContext, 
        IValidator<PaymentRequestDto> validator, 
        ICurrentUser currentUser,
        IUninterPaymentClient uninterPaymentClient)
    {
        _dbContext = dbContext;
        _validator = validator;
        _currentUser = currentUser;
        _uninterPaymentClient = uninterPaymentClient;
    }

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
            throw new ArgumentException("Total do pedido está incorreto.");
        }

        // Map domain payment method to SDK payment method
        var sdkMethod = parameter.PaymentMethod.Method == PaymentMethod.Pix 
            ? UninterPaymentMethod.Pix 
            : UninterPaymentMethod.CreditCard;

        var sdkRequest = new UninterPayment.SDK.PaymentRequest
        {
            OrderId = order.PublicId,
            Amount = order.TotalPrice,
            Method = sdkMethod,
            CardNumber = null,
            PixKey = null,
            WebhookUrl = "http://localhost:5269/pagamento/webhook"
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
            TotalPaid = domainStatus == PaymentStatus.Paid ? order.TotalPrice : 0,
            PaymentMethod = parameter.PaymentMethod.Method,
            Status = domainStatus,
            Description = domainStatus == PaymentStatus.Paid 
                ? $"Aprovado na hora via cartão. Transação: {sdkResult.TransactionId}" 
                : $"Aguardando confirmação Pix. Transação: {sdkResult.TransactionId}"
        };

        _dbContext.Payments.Add(payment);

        var paymentOrder = new PaymentOrder
        {
            Order = order,
            Payment = payment
        };
        _dbContext.PaymentOrders.Add(paymentOrder);

        await _dbContext.SaveChangesAsync(cancellation);

        var responseDto = new PaymentResponseDto
        {
            OrderId = order.PublicId,
            Status = domainStatus,
            AmountPaid = payment.TotalPaid
        };

        return Result<PaymentResponseDto>.Success(responseDto);
    }
}

