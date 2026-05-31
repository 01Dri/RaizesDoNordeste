using FluentValidation;
using RestauranteUni.Domain.Account.DTO;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Application.UseCases.Account.Validations
{
    public class CreateAccountDtoValidation : AbstractValidator<CreateAccountDto>
    {
        public CreateAccountDtoValidation()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("E-mail is required")
                .Must(Email.IsValid)
                .WithMessage("Invalid e-mail");


            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must have at least 8 characters")
                .Matches("[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]")
                .WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]")
                .WithMessage("Password must contain at least one number");
        }
    }
}
