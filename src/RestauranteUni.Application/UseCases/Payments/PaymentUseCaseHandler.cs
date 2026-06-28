using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestauranteUni.Application.Extensions;
using RestauranteUni.Data;
using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.Core.Payments.DTO;
using RestauranteUni.Domain.Core.Users;
using RestauranteUni.Domain.UseCases;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Application.UseCases.Payments;

public sealed class PaymentUseCaseHandler : IUseCaseHandler<PaymentRequestDto, PaymentResponseDto>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IValidator<PaymentRequestDto> _validator;
    private readonly ICurrentUser _currentUser;

    public PaymentUseCaseHandler(ApplicationDbContext dbContext, IValidator<PaymentRequestDto> validator, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _validator = validator;
        _currentUser = currentUser;
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
        throw new NotImplementedException();
    }
}
