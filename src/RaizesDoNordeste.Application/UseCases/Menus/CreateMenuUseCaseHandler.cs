using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Menus;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Menus
{
    public sealed class CreateMenuUseCaseHandler : IUseCaseHandler<CreateMenuDto, MenuResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateMenuDto> _validator;
        private readonly ICurrentUser _currentUser;

        public CreateMenuUseCaseHandler(ApplicationDbContext context, IValidator<CreateMenuDto> validator, ICurrentUser currentUser)
        {
            _context = context;
            _validator = validator;
            _currentUser = currentUser;
        }

        public async Task<Result<MenuResponseDto>> HandleAsync(CreateMenuDto parameter, CancellationToken cancellation = default)
        {
            var validation = await _validator.ValidateAsync(parameter, cancellation);
            if (validation.ContainsErrors())
            {
                return validation.ToResultFailure<MenuResponseDto>();
            }

            var targetRestaurantId = parameter.RestaurantId ?? _currentUser.RestaurantId;

            if (targetRestaurantId == Guid.Empty)
            {
                return Result<MenuResponseDto>.Failure(new Error("Restaurante não informado."), HttpStatusCode.BadRequest);
            }

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == targetRestaurantId, cancellation);

            if (restaurant == null)
            {
                return Result<MenuResponseDto>.FailureNotFound("Restaurante não encontrado.");
            }

            var menuExists = await _context.Menus
                .AnyAsync(m => m.RestaurantId == targetRestaurantId && m.Name == parameter.Name, cancellation);

            if (menuExists)
            {
                return Result<MenuResponseDto>.Failure(
                    new Error("Já existe um cardápio com este nome para esta unidade."),
                    HttpStatusCode.Conflict
                );
            }

            var menu = new Menu
            {
                PublicId = Guid.NewGuid(),
                Name = parameter.Name,
                RestaurantId = targetRestaurantId
            };

            await _context.Menus.AddAsync(menu, cancellation);
            await _context.SaveChangesAsync(cancellation);

            var response = new MenuResponseDto
            {
                Name = menu.Name,
                RestaurantName = restaurant.Name,
                RestaurantId = restaurant.Id,
                Items = []
            };

            return Result<MenuResponseDto>.Success(response);
        }
    }
}
