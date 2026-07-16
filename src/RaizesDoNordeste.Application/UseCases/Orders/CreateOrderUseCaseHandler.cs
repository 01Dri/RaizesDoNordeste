using System.Collections.Immutable;
using System.Net;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Menus;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.Core.Orders.DTO;
using RaizesDoNordeste.Domain.Core.Payments;
using RaizesDoNordeste.Domain.Core.Stocks;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Orders;

public sealed class CreateOrderUseCaseHandler : IUseCaseHandler<CreateOrderDto, OrderResponseDto>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<CreateOrderDto> _validator;

    public CreateOrderUseCaseHandler(ApplicationDbContext dbContext, ICurrentUser currentUser, IValidator<CreateOrderDto> validator)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Result<OrderResponseDto>> HandleAsync(CreateOrderDto parameter,
        CancellationToken cancellation = default)
    {
        var validation = await _validator.ValidateAsync(parameter, cancellation);
        if (validation.ContainsErrors())
        {
            return validation.ToResultFailure<OrderResponseDto>();
        }

        /**
         * Fluxo criação de pedido
         *
         * 1 - Validar entradas
         * 2 - Validar se items necessários estão disponiveis (Em caso não, informar os items indisponiveis para o pedido)
         * 3 - Verificar se tem ingredientes disponiveis em cada item do menu, quando comporado a quantidade do item no pedido x quantidade do ingrediente no stock
         * 4 - Movimentação de estoque
         * 4 - Criar pedido
         */
        parameter.Items = BuildParameterItems(parameter);
        
        var itemsPublicIds = parameter.Items.Select(x => x.PublicMenuItemId).ToList();
        var menuItems = await _dbContext.MenuItems
            .Include(x => x.Menu)
            .Include(x => x.Ingredients)
            .ThenInclude(x => x.StockIngredient)
            .Where(x => itemsPublicIds.Contains(x.PublicId) && x.Menu.RestaurantId == _currentUser.RestaurantId).ToListAsync(cancellation);
        if (menuItems.Count == 0)
        {
            return Result<OrderResponseDto>.FailureNotFound("Itens do cardápio não encontrados");
        }

        if (menuItems.Count != itemsPublicIds.Count)
        {
            return Result<OrderResponseDto>.FailureNotFound("Alguns itens do cardápio não foram encontrados");
        }

        var menuItemsNotAvailable = menuItems.Where(x => !x.IsAvailable)
            .Select(x => x.Title).ToList();

        if (menuItemsNotAvailable.Count > 0)
        {
            return Result<OrderResponseDto>.Failure(new Error()
            {
                    Message = "Alguns itens do cardápio não estão disponíveis.",
                    Details = menuItemsNotAvailable.Select(x => new
                    {
                        Item = x,
                    })
            });
        }
        
        var menuItemsConsumption = BuildOrderStockConsumption(menuItems, parameter);
        var menuItemsWithoutIngredientStock = menuItemsConsumption.Where(x => !x.HaveIngredientStock).ToList();
        if (menuItemsWithoutIngredientStock.Count > 0)
        {
            return Result<OrderResponseDto>.Failure(new Error()
            {
                Message = "Alguns itens do cardápio não possuem ingredientes suficientes em estoque",
                Details = menuItemsWithoutIngredientStock.Select(x => new
                {
                    Item = x.Item.Title,
                    Ingredients = string.Join(", ", x.IngredientsWithoutStock)
                })
            });
        }

        var order = new Order()
        {
            Status = OrderStatus.Process,
            Channel = parameter.Channel,
            RestaurantId = _currentUser.RestaurantId,
            AccountId = _currentUser.AccountId
        };

        await _dbContext.AddAsync(order, cancellation);
        await _dbContext.SaveChangesAsync(cancellation);
        
        await HandleMenuItemsAsync(menuItemsConsumption, order);
        var totalPrice = menuItemsConsumption.Sum(x => x.TotalPrice);
        order.TotalPrice = totalPrice;
        await _dbContext.SaveChangesAsync(cancellation);
        var orderResponse = new OrderResponseDto()
        {
            Id = order.PublicId,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            AccountId = _currentUser.AccountId,
            AccountEmail = _currentUser.Email,
            Status = order.Status,
            Channel = order.Channel,
            TotalPrice = totalPrice,
            Items = order.Items.Select(x => new OrderItemResponseDto()
            {
                Id = x.Id.GetValueOrDefault(),
                MenuId = x.MenuItem.Menu.PublicId,
                MenuItemId = x.MenuItem.PublicId,
                MenuItemName = x.MenuItem.Title,
                UnitPrice = x.MenuItem.Price,
                Quantity = menuItemsConsumption.FirstOrDefault(m => m.ItemId == x.MenuItemId)!.TotalQuantity,
            }).ToImmutableList()
        };

        return Result<OrderResponseDto>.Success(orderResponse, HttpStatusCode.Created);
    }


    private List<CreateOrderItemDto> BuildParameterItems(CreateOrderDto parameter)
    {
        return parameter.Items
            .GroupBy(x => x.PublicMenuItemId)
            .Select(x => new CreateOrderItemDto()
            {
                PublicMenuItemId = x.Key,
                Quantity = x.Sum(i => i.Quantity)
            }).ToList();
    }


    private List<MenuItemOrderConsumptionDto>? BuildOrderStockConsumption(List<MenuItem> menuItems,
        CreateOrderDto parameter)
    {
        var result = new List<MenuItemOrderConsumptionDto>();

        // Saldo temporário para simular consumo
        var availableStock = menuItems
            .SelectMany(x => x.Ingredients)
            .Select(x => x.StockIngredient)
            .DistinctBy(x => x.Id)
            .ToDictionary(
                x => x.Id,
                x => x.Quantity);

        foreach (var orderItem in parameter.Items)
        {
            var menuItem = menuItems.FirstOrDefault(x => x.PublicId == orderItem.PublicMenuItemId);

            if (menuItem is null)
            {
                return null;
            }

            var menuItemConsumption = new MenuItemOrderConsumptionDto()
            {
                ItemId = menuItem.Id,
                Item = menuItem,
                TotalPrice = menuItem.Price * orderItem.Quantity,
                TotalQuantity = orderItem.Quantity
            };

            foreach (var ingredient in menuItem.Ingredients)
            {
                var stockIngredient = ingredient.StockIngredient;

                var quantityToConsume =
                    ingredient.QuantityUseToOrder * orderItem.Quantity;

                var currentStock =
                    availableStock[stockIngredient.Id];

                if (quantityToConsume > currentStock)
                {

                    menuItemConsumption.HaveIngredientStock = false;
                    menuItemConsumption.IngredientsWithoutStock.Add(ingredient.StockIngredient.Name);
                }

                availableStock[stockIngredient.Id] -= quantityToConsume;

                menuItemConsumption.IngredientConsumptions.Add(
                    new IngredientOrderConsumptionDto()
                    {
                        Ingredient = ingredient,
                        QuantityToUseInOrder = quantityToConsume,
                    });
            }
            

            result.Add(menuItemConsumption);
        }

        return result;
    }


    private async Task HandleMenuItemsAsync(List<MenuItemOrderConsumptionDto> menuItems, Order order)
    {
        foreach (var menuItem in menuItems)
        {
            foreach (var ingredientPreOrder in menuItem.IngredientConsumptions)
            {
                var stockIngredient =
                    ingredientPreOrder.Ingredient.StockIngredient;

                stockIngredient.Quantity -=
                    ingredientPreOrder.QuantityToUseInOrder;

                await _dbContext.StockIngredientMovements.AddAsync(
                    new StockIngredientMovement
                    {
                        StockIngredientId = stockIngredient.Id,
                        Type = StockMovementType.Consumption,
                        Quantity = ingredientPreOrder.QuantityToUseInOrder,
                        Description = "Consumo em um pedido",
                        CreatedAt = DateTime.UtcNow,
                        Order = order
                    });
                
            }

            menuItem.Item.IsAvailable =
                menuItem.Item.Ingredients.All(x =>
                    x.StockIngredient.Quantity >= x.QuantityUseToOrder);

            order.Items.Add(new OrderItem()
            {
                MenuItem = menuItem.Item,
                Quantity = menuItem.TotalQuantity
            });

            order.Status = OrderStatus.Chicken;
            
        }
        
    }
}

