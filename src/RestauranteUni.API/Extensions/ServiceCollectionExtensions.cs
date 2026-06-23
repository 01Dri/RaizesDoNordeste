using System.Reflection;
using FluentValidation;
using RestauranteUni.Application.Patterns.Dispatchers;
using RestauranteUni.Application.Patterns.Dispatchers.Orders;
using RestauranteUni.Application.Patterns.Dispatchers.Orders.Handlers;
using RestauranteUni.Data;
using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.Core.Orders;
using RestauranteUni.Domain.Core.Users;
using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.API.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        extension(IServiceCollection service)
        {
            public IServiceCollection AddApplicationServices(params Type[] assemblyMarkers)
            {
                var assemblies = assemblyMarkers
                    .Select(x => x.Assembly)
                    .Distinct()
                    .ToArray();

                service.RegisterImplementations(typeof(IUseCaseHandler<>), assemblies);
                service.RegisterImplementations(typeof(IUseCaseHandler<,>), assemblies);
                service.RegisterImplementations(typeof(IValidator<>), assemblies);
                return service;
            }

            public IServiceCollection AddPatterns()
            {
                service.AddDispatchers();
                return service;
            }

            private void AddDispatchers()
            {
                
                /**
                 * Preciso pegar a instancia do dispatcher
                 * Depois os handlers, adicionar na instancia do dispatcher.
                 *
                 * Os handlers precisam estar no container, e o dispatcher também
                 */
    
                service.AddScoped<IOrderStatusHandler, OrderStatusOrderStatusReadyHandler>();
                service.AddScoped<IDispatcher<OrderStatus, Order>, OrderStatusDispatcher>(s =>
                {
                    var currentUser = s.GetRequiredService<ICurrentUser>();
                    var applicationDbContext = s.GetRequiredService<ApplicationDbContext>();
                    var handlers = new List<IOrderStatusHandler>()
                    {
                        new OrderStatusOrderStatusReadyHandler()
                    };
                    return new OrderStatusDispatcher(handlers, currentUser, applicationDbContext);
                });
                
            }

            private void RegisterImplementations(Type serviceType, IEnumerable<Assembly> assemblies)
            {
                var implementations = assemblies
                    .SelectMany(x => x.GetTypes())
                    .Where(x => x is { IsClass: true, IsAbstract: false })
                    .SelectMany(implementation => implementation.GetInterfaces()
                        .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == serviceType)
                        .Select(type => new
                        {
                            Service = type,
                            Implementation = implementation
                        }));

                foreach (var implementation in implementations)
                {
                    service.AddScoped(implementation.Service, implementation.Implementation);
                }    
            }
        }
    }
}
