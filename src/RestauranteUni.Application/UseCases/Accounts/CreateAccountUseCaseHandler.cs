using FluentValidation;
using RestauranteUni.Application.Extensions;
using RestauranteUni.Data;
using RestauranteUni.Domain;
using RestauranteUni.Domain.Accounts;
using RestauranteUni.Domain.Accounts.DTO;
using RestauranteUni.Domain.Accounts.Roles;
using RestauranteUni.Domain.UseCases;
using RestauranteUni.Domain.Utils;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Application.UseCases.Accounts
{
    public class CreateAccountUseCaseHandler : IUseCaseHandler<CreateAccountDto, CreateAccountUseCaseResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateAccountDto> _validator;
        private readonly IHasher _hasher;

        public CreateAccountUseCaseHandler(ApplicationDbContext context, IValidator<CreateAccountDto> validator, IHasher hasher)
        {
            _context = context;
            _validator = validator;
            _hasher = hasher;
        }

        public async Task<Result<CreateAccountUseCaseResponseDto>> HandleAsync(CreateAccountDto parameter, CancellationToken cancellation = default)
        {
            var validations = await _validator.ValidateAsync(parameter, cancellation);
            if (validations != null && validations.Errors.Count > 0)
            {
                return validations.ToResultFailure<CreateAccountUseCaseResponseDto>();
            }

            var email = new Email(parameter.Email);
            var account = new Account()
            {
                Email = email,
                Password = _hasher.HashPassword(parameter.Password),
                RoleAccounts =
                [
                    RoleAccount.Create(RoleType.Customer, RoleStatus.Enable)
                ]
            };

            await _context.Accounts.AddAsync(account, cancellation);
            await _context.SaveChangesAsync(cancellation);
            return Result<CreateAccountUseCaseResponseDto>.Success(new CreateAccountUseCaseResponseDto()
            {
                Id = account.Id,
                Email = account.Email,
                Roles = account.RoleAccounts
                    .Where(x => x.RoleId.HasValue)
                    .Select(x => x.RoleId!.Value)
                    .ToList()
            });

        }
    }
}
