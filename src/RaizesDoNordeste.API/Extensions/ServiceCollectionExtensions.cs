using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RaizesDoNordeste.Application.Patterns.Decorators;
using RaizesDoNordeste.Application.Patterns.Dispatchers;
using RaizesDoNordeste.Application.Patterns.Dispatchers.Orders;
using RaizesDoNordeste.Application.Patterns.Dispatchers.Orders.Handlers;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.API.Extensions
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
                service.AddScoped<IOrderStatusHandler, OrderStatusReadyHandler>();
                service.AddScoped<IDispatcher<OrderStatus, Order>, OrderStatusDispatcher>(s =>
                {
                    var handlers = GetOrderStatusHandlers();
                    var currentUser = s.GetRequiredService<ICurrentUser>();
                    var applicationDbContext = s.GetRequiredService<ApplicationDbContext>();
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

                foreach (var impl in implementations)
                {
                    service.RegisterSingleImplementation(serviceType, impl.Service, impl.Implementation);
                }    
            }

            private void RegisterSingleImplementation(Type serviceType, Type interfaceType, Type implementationType)
            {
                if (serviceType == typeof(IUseCaseHandler<,>))
                {
                    service.AddScoped(implementationType);
                    service.AddScoped(interfaceType, provider =>
                    {
                        var innerHandler = provider.GetRequiredService(implementationType);
                        var hasTransactional = implementationType.GetCustomAttribute<TransactionalAttribute>() != null;

                        if (hasTransactional)
                        {
                            var requestType = interfaceType.GetGenericArguments()[0];
                            var responseType = interfaceType.GetGenericArguments()[1];
                            var decoratorType = typeof(TransactionalUseCaseHandlerDecorator<,>).MakeGenericType(requestType, responseType);
                            return ActivatorUtilities.CreateInstance(provider, decoratorType, innerHandler);
                        }

                        return innerHandler;
                    });
                    return;
                }

                if (serviceType == typeof(IUseCaseHandler<>))
                {
                    service.AddScoped(implementationType);
                    service.AddScoped(interfaceType, provider =>
                    {
                        var innerHandler = provider.GetRequiredService(implementationType);
                        var hasTransactional = implementationType.GetCustomAttribute<TransactionalAttribute>() != null;

                        if (hasTransactional)
                        {
                            var responseType = interfaceType.GetGenericArguments()[0];
                            var decoratorType = typeof(TransactionalUseCaseHandlerDecorator<>).MakeGenericType(responseType);
                            return ActivatorUtilities.CreateInstance(provider, decoratorType, innerHandler);
                        }

                        return innerHandler;
                    });
                    return;
                }

                service.AddScoped(interfaceType, implementationType);
            }

            private static List<IOrderStatusHandler> GetOrderStatusHandlers()
            {
                var assembly = typeof(IOrderStatusHandler).Assembly;
                var implementations = assembly
                    .GetTypes()
                    .Where(t =>
                        t.IsClass &&
                        !t.IsAbstract &&
                        typeof(IOrderStatusHandler).IsAssignableFrom(t))
                    .ToList();

               return implementations
                    .Select(type => (IOrderStatusHandler)Activator.CreateInstance(type)!)
                    .ToList();
            }
        }
    }
}

