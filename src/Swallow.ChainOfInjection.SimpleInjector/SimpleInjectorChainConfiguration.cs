namespace Swallow.ChainOfInjection.SimpleInjector;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using global::SimpleInjector;

/// <summary>
///     Extensions to configure a chain of injection for a <see cref="Container" />.
/// </summary>
public static class SimpleInjectorChainConfiguration
{
    /// <summary>
    ///     Begin configuring a chain of injection.
    /// </summary>
    /// <param name="container">Container to use when registering types.</param>
    /// <typeparam name="T">Common type of the chain members.</typeparam>
    /// <returns>A chain configurator to use to register the chain members.</returns>
    /// <remarks>
    ///     Be sure to call <see cref="IChainConfigurator{TChainMember,TLifestyle}.Configure" /> on the returned chain configurator, since nothing
    ///     will be registered until doing so.
    /// </remarks>
    public static IChainConfigurator<T, Lifestyle> RegisterChain<T>(this Container container) where T : class
    {
        return new SimpleInjectorChainConfigurator<T>(container: container, lifestyle: Lifestyle.Scoped);
    }

    /// <summary>
    ///     Begin configuring a chain of injection.
    /// </summary>
    /// <param name="container">Container to use when registering types.</param>
    /// <param name="defaultLifestyle">Default lifestyle to register types with if none is explicitly given.</param>
    /// <typeparam name="T">Common type of the chain members.</typeparam>
    /// <returns>A chain configurator to use to register the chain members.</returns>
    /// <remarks>
    ///     Be sure to call <see cref="IChainConfigurator{TChainMember,TLifestyle}.Configure" /> on the returned chain configurator, since nothing
    ///     will be registered until doing so.
    /// </remarks>
    public static IChainConfigurator<T, Lifestyle> RegisterChain<T>(this Container container, Lifestyle defaultLifestyle) where T : class
    {
        return new SimpleInjectorChainConfigurator<T>(container: container, lifestyle: defaultLifestyle);
    }

    private sealed class SimpleInjectorChainConfigurator<T> : AbstractChainConfigurator<T, Func<object>, Lifestyle>
    {
        private readonly Container container;

        public SimpleInjectorChainConfigurator(Container container, Lifestyle lifestyle) : base(lifestyle)
        {
            this.container = container;
        }

        protected override void Register(Type targetType, Func<object> factory, Lifestyle lifestyle)
        {
            container.Register(serviceType: targetType, instanceCreator: factory, lifestyle: lifestyle);
        }

        protected override Func<object> CreateFactory(ConstructorInfo constructor, IReadOnlyList<Type?> parameterTypes)
        {
            return () => constructor.Invoke(parameterTypes.Select(t => t is null ? null : container.GetInstance(t)).ToArray());
        }
    }
}
