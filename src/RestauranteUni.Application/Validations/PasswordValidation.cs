using FluentValidation;

namespace RestauranteUni.Application.Validations
{
    internal sealed class PasswordValidation : AbstractValidator<string>
    {
        public PasswordValidation()
        {
            RuleFor(x => x)
                .NotEmpty()
                .WithMessage("A senha é obrigatória")
                .MinimumLength(8)
                .WithMessage("A senha deve ter pelo menos 8 caracteres")
                .Matches("[A-Z]")
                .WithMessage("A senha deve conter pelo menos uma letra maiúscula")
                .Matches("[a-z]")
                .WithMessage("A senha deve conter pelo menos uma letra minúscula")
                .Matches("[0-9]")
                .WithMessage("A senha deve conter pelo menos um número");
        }
    }
}
