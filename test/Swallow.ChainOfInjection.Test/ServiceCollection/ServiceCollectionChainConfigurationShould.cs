namespace Swallow.ChainOfInjection.Test.ServiceCollection;

using System;
using System.Linq;
using ChainOfInjection.ServiceCollection;
using Microsoft.Extensions.DependencyInjection;

internal sealed class ServiceCollectionChainConfigurationShould
{
    private readonly IServiceCollection serviceCollection = new ServiceCollection();

    [Test]
    public async Task ThrowException_WhenNoChainMembersHaveBeenConfigured()
    {
        var chainConfiguration = serviceCollection.AddChain<IChainMember>();

        var exception = Assert.Throws<InvalidOperationException>(chainConfiguration.Configure);
        await Assert.That(exception.Message)
            .EqualTo($"No implementation defined for {typeof(IChainMember)}. Please add one (or more) using '{nameof(chainConfiguration.Add)}'.");
    }

    [Test]
    public async Task RegisterTypeWithCorrectLifestyle()
    {
        var chainConfiguration = serviceCollection.AddChain<IChainMember>();
        chainConfiguration.Add<TerminatingMember>(ServiceLifetime.Transient).Configure();

        var serviceDescriptor = serviceCollection.Single();
        var service = serviceCollection.BuildServiceProvider().GetService<IChainMember>();

        await Assert.That(serviceDescriptor.ServiceType).EqualTo(typeof(IChainMember));
        await Assert.That(serviceDescriptor.Lifetime).EqualTo(ServiceLifetime.Transient);
        await Assert.That(service).IsTypeOf<TerminatingMember>();
    }

    [Test]
    public async Task RegisterChainMembersWithGivenDefaultLifestyle()
    {
        var chainConfiguration = serviceCollection.AddChain<IChainMember>(ServiceLifetime.Singleton);
        chainConfiguration.Add<ChainingMember>().Add<TerminatingMember>().Configure();

        await Assert.That(serviceCollection).All().Satisfy(sd => sd.HasProperty(x => x.Lifetime, ServiceLifetime.Singleton));
    }
}
