using FluentValidation;
using RaizesDoNordeste.Domain.Core.Menus.DTO;

namespace RaizesDoNordeste.Application.UseCases.Menus.Validations
{
    public sealed class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Título é obrigatório.")
                .MaximumLength(150).WithMessage("Título deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Preço deve ser maior que zero.");

            RuleFor(x => x.PreparationTimeInMinutes)
                .GreaterThanOrEqualTo(0).WithMessage("Tempo de preparo não pode ser negativo.");
        }
    }
}
