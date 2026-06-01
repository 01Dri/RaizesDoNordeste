using FluentValidation;
using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.API.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            params Type[] assemblyMarkers)
        {
            var assemblies = assemblyMarkers
                .Select(x => x.Assembly)
                .Distinct()
                .ToArray();

            services.RegisterImplementations(typeof(IUseCaseHandler<,>), assemblies);
            services.RegisterImplementations(typeof(IValidator<>), assemblies);

            return services;
        }

        private static void RegisterImplementations(
            this IServiceCollection services,
            Type serviceType,
            IEnumerable<System.Reflection.Assembly> assemblies)
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
                services.AddScoped(implementation.Service, implementation.Implementation);
            }
        }
    }
}
