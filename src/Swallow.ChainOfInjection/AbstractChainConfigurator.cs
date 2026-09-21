namespace Swallow.ChainOfInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
///     A base implementation of a <see cref="IChainConfigurator{TChainMember,TLifestyle}" />, albeit without an underlying container.
/// </summary>
/// <typeparam name="TChainMember">The common type for all chain members.</typeparam>
/// <typeparam name="TFactory">The type of factory suitable for the DI-container.</typeparam>
/// <typeparam name="TLifestyle">The type used for lifestyle declaration with the container.</typeparam>
/// <remarks>
///     If your container is e.g. an <c>IServiceCollection</c>, you will probably use the method accepting a <c>Func{IServiceProvider, object}</c>;
///     this then is the type to set as <c>TFactory</c>.
/// </remarks>
public abstract class AbstractChainConfigurator<TChainMember, TFactory, TLifestyle> : IChainConfigurator<TChainMember, TLifestyle>
{
    private readonly IList<ChainMember> configuredChainMembers;
    private readonly TLifestyle defaultLifestyle;

    /// <summary>
    ///     Create a new <see cref="AbstractChainConfigurator{TChainMember,TFactory,TLifestyle}" />.
    /// </summary>
    protected AbstractChainConfigurator(TLifestyle defaultLifestyle)
    {
        configuredChainMembers = new List<ChainMember>();
        this.defaultLifestyle = defaultLifestyle;
    }

    /// <inheritdoc />
    public IChainConfigurator<TChainMember, TLifestyle> Add<TConcreteMember>() where TConcreteMember : TChainMember
    {
        return Add(type: typeof(TConcreteMember), lifestyle: defaultLifestyle);
    }

    /// <inheritdoc />
    public IChainConfigurator<TChainMember, TLifestyle> Add<TConcreteMember>(TLifestyle lifestyle) where TConcreteMember : TChainMember
    {
        return Add(type: typeof(TConcreteMember), lifestyle: lifestyle);
    }

    /// <inheritdoc />
    public IChainConfigurator<TChainMember, TLifestyle> Add(Type type)
    {
        return Add(type: type, lifestyle: defaultLifestyle);
    }

    /// <inheritdoc />
    public IChainConfigurator<TChainMember, TLifestyle> Add(Type type, TLifestyle lifestyle)
    {
        ThrowIfTypeIsNotCompatible(type);
        configuredChainMembers.Add(new(Type: type, Lifestyle: lifestyle));

        return this;
    }

    /// <inheritdoc />
    public void Configure()
    {
        ThrowIfNoChainMembersConfigured();
        for (var i = 0; i < configuredChainMembers.Count; ++i)
        {
            var currentMember = configuredChainMembers[i];
            var followingMember = configuredChainMembers.ElementAtOrDefault(i + 1);

            RegisterType(
                chainMember: currentMember,
                followingType: followingMember?.Type,
                targetType: i == 0 ? typeof(TChainMember) : currentMember.Type);
        }
    }

    /// <summary>
    ///     Register the given type with the DI container using the given factory and the given lifestyle.
    /// </summary>
    /// <param name="targetType">Type to register the result of the factory as.</param>
    /// <param name="factory">The factory which constructs the type.</param>
    /// <param name="lifestyle">The lifestyle to register the result of the factory as.</param>
    protected abstract void Register(Type targetType, TFactory factory, TLifestyle lifestyle);

    /// <summary>
    ///     Create a factory for the given constructor.
    /// </summary>
    /// <param name="constructor">The constructor which should be invoked.</param>
    /// <param name="parameterTypes">The types to supply to the constructor, in the exact order they are to be supplied.</param>
    /// <returns>The created factory, which is able to call the given constructor.</returns>
    /// <remarks>
    ///     Typically, you'll want to resolve each given type to an object and invoke the constructor with these objects.
    /// </remarks>
    protected abstract TFactory CreateFactory(ConstructorInfo constructor, IReadOnlyList<Type?> parameterTypes);

    private void RegisterType(ChainMember chainMember, Type? followingType, Type targetType)
    {
        var constructor = GetConstructor(chainMember.Type);
        var substitutedTypes = constructor.GetParameters()
            .Select(p => p.ParameterType.IsAssignableTo(typeof(TChainMember)) ? followingType : p.ParameterType)
            .ToList();

        var factory = CreateFactory(constructor: constructor, parameterTypes: substitutedTypes);
        Register(targetType: targetType, factory: factory, lifestyle: chainMember.Lifestyle);
    }

    private static ConstructorInfo GetConstructor(Type type)
    {
        var publicConstructors = type.GetConstructors().ToList();
        if (publicConstructors.Count != 1)
        {
            throw new InvalidOperationException($"The added type {type} needs to have exactly one public constructor.");
        }

        return publicConstructors.Single();
    }

    private static void ThrowIfTypeIsNotCompatible(Type type)
    {
        if (type.IsAssignableTo(typeof(TChainMember)) is false)
        {
            throw new ArgumentException($"The given type {type} does not implement {typeof(TChainMember)}.");
        }
    }

    private void ThrowIfNoChainMembersConfigured()
    {
        if (configuredChainMembers.Count == 0)
        {
            throw new InvalidOperationException(
                $"No implementation defined for {typeof(TChainMember)}. Please add one (or more) using '{nameof(Add)}'.");
        }
    }

    private sealed record ChainMember(Type Type, TLifestyle Lifestyle);
}
