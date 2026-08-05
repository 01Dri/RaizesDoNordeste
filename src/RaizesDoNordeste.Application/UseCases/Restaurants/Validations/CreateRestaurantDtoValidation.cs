using FluentValidation;
using RaizesDoNordeste.Application.Validations;
using RaizesDoNordeste.Domain.Core.Restaurants.DTO;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Restaurants.Validations
{
    public sealed class CreateRestaurantDtoValidation : AbstractValidator<CreateRestaurantDto>
    {
        public CreateRestaurantDtoValidation()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome da unidade é obrigatório.")
                .MaximumLength(150).WithMessage("O nome da unidade não pode exceder 150 caracteres.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("A descrição é obrigatória.");

            RuleFor(x => x.Phone)
                .Must(Phone.IsValid).WithMessage("O telefone informado é inválido.");

            RuleFor(x => x.Email)
                .SetValidator(new EmailValidation());

            RuleFor(x => x.Cnpj)
                .Must(Cnpj.IsValid).WithMessage("O CNPJ informado é inválido.");

            RuleFor(x => x.AddressStreet).NotEmpty().WithMessage("O logradouro é obrigatório.");
            RuleFor(x => x.AddressNumber).NotEmpty().WithMessage("O número do endereço é obrigatório.");
            RuleFor(x => x.AddressDistrict).NotEmpty().WithMessage("O bairro é obrigatório.");
            RuleFor(x => x.AddressCity).NotEmpty().WithMessage("A cidade é obrigatória.");
            RuleFor(x => x.AddressState).NotEmpty().WithMessage("O estado (UF) é obrigatório.");
            RuleFor(x => x.AddressZipCode).NotEmpty().WithMessage("O CEP é obrigatório.");
        }
    }
}
