namespace Swallow.ChainOfInjection;

using System;

/// <summary>
///     The interface outlining the flow of configuring a chain.
/// </summary>
/// <typeparam name="TChainMember">The common type for all chain members.</typeparam>
/// <typeparam name="TLifestyle">The type used for lifestyle declaration with the container.</typeparam>
/// <remarks>
///     When building your own chain configurator, you should inherit from
///     <see cref="AbstractChainConfigurator{TChainMember,TFactory,TLifestyle}" /> instead of implementing this interface by hand.
/// </remarks>
public interface IChainConfigurator<in TChainMember, in TLifestyle>
{
    /// <summary>
    ///     Add a type to the chain.
    /// </summary>
    /// <typeparam name="T">Type to add to the chain.</typeparam>
    /// <returns>
    ///     <c>this</c>
    /// </returns>
    IChainConfigurator<TChainMember, TLifestyle> Add<T>() where T : TChainMember;

    /// <summary>
    ///     Add a type to the chain.
    /// </summary>
    /// <param name="lifestyle">Lifestyle to register the type as.</param>
    /// <typeparam name="T">Type to add to the chain.</typeparam>
    /// <returns>
    ///     <c>this</c>
    /// </returns>
    IChainConfigurator<TChainMember, TLifestyle> Add<T>(TLifestyle lifestyle) where T : TChainMember;

    /// <summary>
    ///     Add a type to the chain.
    /// </summary>
    /// <param name="type">Type to add to the chain.</param>
    /// <returns>
    ///     <c>this</c>
    /// </returns>
    /// <exception cref="ArgumentException"> when the given type does not implement the common chain member type.</exception>
    IChainConfigurator<TChainMember, TLifestyle> Add(Type type);

    /// <summary>
    ///     Add a type to the chain.
    /// </summary>
    /// <param name="type">Type to add to the chain.</param>
    /// <param name="lifestyle">Lifestyle to register the type as.</param>
    /// <returns>
    ///     <c>this</c>
    /// </returns>
    /// <exception cref="ArgumentException"> when the given type does not implement the common chain member type.</exception>
    IChainConfigurator<TChainMember, TLifestyle> Add(Type type, TLifestyle lifestyle);

    /// <summary>
    ///     Setup the configured chain in the DI-container.
    /// </summary>
    /// <exception cref="InvalidOperationException">when no chain member has been <see cref="Add{T}()" />ed.</exception>
    /// <exception cref="InvalidOperationException">when a chain member does not have exactly one public constructor.</exception>
    void Configure();
}

