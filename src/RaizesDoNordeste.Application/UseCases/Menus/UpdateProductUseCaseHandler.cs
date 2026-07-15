using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.UseCases.Menus
{
    public sealed class UpdateProductUseCaseHandler : IUseCaseHandler<UpdateProductDto, ProductResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<UpdateProductDto> _validator;
        private readonly ICurrentUser _currentUser;

        public UpdateProductUseCaseHandler(ApplicationDbContext context, IValidator<UpdateProductDto> validator, ICurrentUser currentUser)
        {
            _context = context;
            _validator = validator;
            _currentUser = currentUser;
        }

        public async Task<Result<ProductResponseDto>> HandleAsync(UpdateProductDto parameter, CancellationToken cancellation = default)
        {
            var validation = await _validator.ValidateAsync(parameter, cancellation);
            if (validation.ContainsErrors())
            {
                return validation.ToResultFailure<ProductResponseDto>();
            }

            var item = await _context.MenuItems
                .Include(i => i.Menu)
                .FirstOrDefaultAsync(i => i.Id == parameter.Id, cancellation);

            if (item == null)
            {
                return Result<ProductResponseDto>.FailureNotFound("Produto não encontrado.");
            }

            if (item.Menu == null || item.Menu.RestaurantId != _currentUser.RestaurantId)
            {
                return Result<ProductResponseDto>.Failure(new Error("Você não tem permissão para editar este produto."), HttpStatusCode.Forbidden);
            }

            item.Title = parameter.Title;
            item.Description = parameter.Description;
            item.Price = parameter.Price;
            item.ImageUrl = parameter.ImageUrl;
            item.IsAvailable = parameter.IsAvailable;
            item.DisplayOrder = parameter.DisplayOrder;
            item.PreparationTimeInMinutes = parameter.PreparationTimeInMinutes;
            item.IsFeatured = parameter.IsFeatured;

            await _context.SaveChangesAsync(cancellation);

            var response = new ProductResponseDto
            {
                Id = item.Id,
                PublicId = item.PublicId,
                Title = item.Title,
                Description = item.Description,
                Price = item.Price,
                ImageUrl = item.ImageUrl,
                IsAvailable = item.IsAvailable,
                DisplayOrder = item.DisplayOrder,
                PreparationTimeInMinutes = item.PreparationTimeInMinutes,
                IsFeatured = item.IsFeatured,
                MenuId = item.MenuId ?? 0L
            };

            return Result<ProductResponseDto>.Success(response);
        }
    }
}
