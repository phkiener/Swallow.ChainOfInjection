namespace Swallow.ChainOfInjection.ServiceCollection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Extensions to configure a chain of injection for a <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionChainConfiguration
{
    /// <summary>
    ///     Begin configuring a chain of injection.
    /// </summary>
    /// <param name="services">Service collection to use when registering types.</param>
    /// <typeparam name="T">Common type of the chain members.</typeparam>
    /// <returns>A chain configurator to use to register the chain members.</returns>
    /// <remarks>
    ///     Be sure to call <see cref="IChainConfigurator{TChainMember,TLifestyle}.Configure" /> on the returned chain configurator, since nothing
    ///     will be registered until doing so.
    /// </remarks>
    public static IChainConfigurator<T, ServiceLifetime> AddChain<T>(this IServiceCollection services) where T : class
    {
        return new ServiceCollectionChainConfigurator<T>(serviceCollection: services, serviceLifetime: ServiceLifetime.Scoped);
    }

    /// <summary>
    ///     Begin configuring a chain of injection.
    /// </summary>
    /// <param name="services">Service collection to use when registering types.</param>
    /// <param name="defaultLifestyle">Default lifestyle to register types with if none is explicitly given.</param>
    /// <typeparam name="T">Common type of the chain members.</typeparam>
    /// <returns>A chain configurator to use to register the chain members.</returns>
    /// <remarks>
    ///     Be sure to call <see cref="IChainConfigurator{TChainMember,TLifestyle}.Configure" /> on the returned chain configurator, since nothing
    ///     will be registered until doing so.
    /// </remarks>
    public static IChainConfigurator<T, ServiceLifetime> AddChain<T>(this IServiceCollection services, ServiceLifetime defaultLifestyle)
        where T : class
    {
        return new ServiceCollectionChainConfigurator<T>(serviceCollection: services, serviceLifetime: defaultLifestyle);
    }

    private sealed class ServiceCollectionChainConfigurator<T> : AbstractChainConfigurator<T, Func<IServiceProvider, object>, ServiceLifetime>
    {
        private readonly IServiceCollection serviceCollection;

        public ServiceCollectionChainConfigurator(IServiceCollection serviceCollection, ServiceLifetime serviceLifetime) : base(serviceLifetime)
        {
            this.serviceCollection = serviceCollection;
        }

        protected override void Register(Type targetType, Func<IServiceProvider, object> factory, ServiceLifetime lifestyle)
        {
            serviceCollection.Add(ServiceDescriptor.Describe(serviceType: targetType, implementationFactory: factory, lifetime: lifestyle));
        }

        protected override Func<IServiceProvider, object> CreateFactory(ConstructorInfo constructor, IReadOnlyList<Type?> parameterTypes)
        {
            return sp => constructor.Invoke(parameterTypes.Select(t => t is null ? null : sp.GetService(t)).ToArray());
        }
    }
}
