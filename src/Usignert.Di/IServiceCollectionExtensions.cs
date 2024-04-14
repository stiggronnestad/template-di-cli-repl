using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Usignert.Di
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, Type entryType)
        {
            var serviceDefinitions = GetServices(entryType);

            for (int i = 0; i < serviceDefinitions.type?.Length; i++)
            {
                Type type = serviceDefinitions.type[i];
                DiServiceAttribute attribute = serviceDefinitions.attribute[i];

                switch (attribute.Type)
                {
                    case DiServiceAttribute.DiServiceType.Singleton:
                        services.AddSingleton(type);
                        break;

                    case DiServiceAttribute.DiServiceType.Scoped:
                        services.AddScoped(type);
                        break;
                    case DiServiceAttribute.DiServiceType.Transient:
                        services.AddTransient(type);
                        break;

                    default:
                        break;
                }
            }

            return services;
        }

        public static IServiceCollection RegisterServices<T>(this IServiceCollection services)
        {
            RegisterServices(services, typeof(T));

            return services;
        }

        public static IServiceCollection RegisterServices(this IServiceCollection services, params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterServices(services, type);
            }

            return services;
        }

        private static (Type[] type, DiServiceAttribute[] attribute) GetServices(Type entryType)
        {
            var types = Assembly.GetAssembly(entryType)?.GetTypes().Where(a => a.GetCustomAttribute<DiServiceAttribute>() != null).ToArray();

            if (types == null)
            {
                return ([], []);
            }

            var lst = (from type in types
                       let attr = type.GetCustomAttribute<DiServiceAttribute>()
                       where attr != null
                       select attr).ToList();

            return (types, lst.ToArray());
        }

        private static (Type[] type, DiServiceAttribute[] attribute) GetServices<T>()
        {
            return GetServices(typeof(T));
        }
    }
}
