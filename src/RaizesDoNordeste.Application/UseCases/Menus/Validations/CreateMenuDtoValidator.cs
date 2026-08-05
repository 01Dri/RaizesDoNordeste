using FluentValidation;
using RaizesDoNordeste.Domain.Core.Menus.DTO;

namespace RaizesDoNordeste.Application.UseCases.Menus.Validations
{
    public class CreateMenuDtoValidator : AbstractValidator<CreateMenuDto>
    {
        public CreateMenuDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome do cardápio é obrigatório.");
        }
    }
}
