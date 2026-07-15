using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Menus;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.UseCases.Menus
{
    public sealed class CreateProductUseCaseHandler : IUseCaseHandler<CreateProductDto, ProductResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateProductDto> _validator;
        private readonly ICurrentUser _currentUser;

        public CreateProductUseCaseHandler(ApplicationDbContext context, IValidator<CreateProductDto> validator, ICurrentUser currentUser)
        {
            _context = context;
            _validator = validator;
            _currentUser = currentUser;
        }

        public async Task<Result<ProductResponseDto>> HandleAsync(CreateProductDto parameter, CancellationToken cancellation = default)
        {
            var validation = await _validator.ValidateAsync(parameter, cancellation);
            if (validation.ContainsErrors())
            {
                return validation.ToResultFailure<ProductResponseDto>();
            }

            var menu = await _context.Menus
                .FirstOrDefaultAsync(m => m.RestaurantId == _currentUser.RestaurantId, cancellation);

            if (menu == null)
            {
                return Result<ProductResponseDto>.Failure(new Error("Cardápio do restaurante do usuário não encontrado."), HttpStatusCode.BadRequest);
            }

            var item = new MenuItem
            {
                Title = parameter.Title,
                Description = parameter.Description,
                Price = parameter.Price,
                ImageUrl = parameter.ImageUrl,
                IsAvailable = parameter.IsAvailable,
                DisplayOrder = parameter.DisplayOrder,
                PreparationTimeInMinutes = parameter.PreparationTimeInMinutes,
                IsFeatured = parameter.IsFeatured,
                MenuId = menu.Id
            };

            await _context.MenuItems.AddAsync(item, cancellation);
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

            return Result<ProductResponseDto>.Success(response, HttpStatusCode.Created);
        }
    }
}
