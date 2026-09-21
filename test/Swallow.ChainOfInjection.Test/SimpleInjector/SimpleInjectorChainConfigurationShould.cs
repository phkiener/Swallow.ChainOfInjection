namespace Swallow.ChainOfInjection.Test.SimpleInjector;

using System;
using ChainOfInjection.SimpleInjector;
using global::SimpleInjector;
using global::SimpleInjector.Lifestyles;
using Microsoft.Extensions.DependencyInjection;

internal sealed class SimpleInjectorChainConfigurationShould
{
    private readonly Container container = new() { Options = { DefaultScopedLifestyle = new AsyncScopedLifestyle() } };

    [Test]
    public async Task ThrowException_WhenNoChainMembersHaveBeenConfigured()
    {
        var chainConfiguration = container.RegisterChain<IChainMember>();

        var exception = Assert.Throws<InvalidOperationException>(chainConfiguration.Configure);
        await Assert.That(exception.Message)
            .EqualTo($"No implementation defined for {typeof(IChainMember)}. Please add one (or more) using '{nameof(chainConfiguration.Add)}'.");
    }

    [Test]
    public async Task RegisterTypeWithCorrectLifestyle()
    {
        var chainConfiguration = container.RegisterChain<IChainMember>();
        chainConfiguration.Add<TerminatingMember>(Lifestyle.Transient).Configure();

        var registration = container.GetRegistration<IChainMember>();
        await using var scope = AsyncScopedLifestyle.BeginScope(container);
        var service = container.GetService<IChainMember>();

        await Assert.That(registration?.Lifestyle).EqualTo(Lifestyle.Transient);
        await Assert.That(service).IsTypeOf<TerminatingMember>();
    }

    [Test]
    public async Task RegisterChainMembersWithGivenDefaultLifestyle()
    {
        var chainConfiguration = container.RegisterChain<IChainMember>(Lifestyle.Singleton);
        chainConfiguration.Add<ChainingMember>().Add<TerminatingMember>().Configure();

        await Assert.That(container.GetRegistration<IChainMember>()!.Lifestyle).EqualTo(Lifestyle.Singleton);
        await Assert.That(container.GetRegistration<TerminatingMember>()!.Lifestyle).EqualTo(Lifestyle.Singleton);
    }
}
