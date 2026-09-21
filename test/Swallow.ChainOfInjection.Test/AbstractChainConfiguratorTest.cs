namespace Swallow.ChainOfInjection.Test;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class AbstractChainConfiguratorTest
{
    private readonly TestChainConfigurator testChainConfigurator = new();

    [Test]
    public void Configure_NoMemberHasBeenAdded_ThrowsException()
    {
        Assert.Throws<InvalidOperationException>(() => testChainConfigurator.Configure());
    }

    [Test]
    public async Task Configure_SingleChainingTypeAdded_RegistersTypeWithCorrectLifestyle()
    {
        const int lifestyle = 1;
        testChainConfigurator.Add<ChainingMember>(lifestyle).Configure();

        var registeredType = testChainConfigurator.GetRegistrationFor<IChainMember>();
        await Assert.That(registeredType?.Lifestyle).EqualTo(lifestyle);
    }

    [Test]
    public async Task Configure_SingleChainingTypeAdded_RegistersTypeAsFirstMemberWithNextAsNull()
    {
        testChainConfigurator.Add<ChainingMember>().Configure();

        var registeredObject = testChainConfigurator.Get<IChainMember>() as ChainingMember;
        await Assert.That(registeredObject).IsNotNull();
        await Assert.That(registeredObject.Next).IsNull();
    }

    [Test]
    public async Task Configure_ChainingTypeAndTerminatingTypeAdded_RegistersChainingTypeAsFirstMemberWithNextAsTerminatingType()
    {
        testChainConfigurator.Add<ChainingMember>().Add<TerminatingMember>().Configure();

        var registeredObject = testChainConfigurator.Get<IChainMember>() as ChainingMember;
        await Assert.That(registeredObject).IsNotNull();
        await Assert.That(registeredObject.Next).IsTypeOf<TerminatingMember>();
    }

    [Test]
    public async Task Configure_TerminatingTypeAndChainingAdded_RegistersTerminatingType()
    {
        testChainConfigurator.Add<TerminatingMember>().Add<ChainingMember>().Configure();

        await Assert.That(testChainConfigurator.Get<IChainMember>()).IsTypeOf<TerminatingMember>();
    }

    [Test]
    public async Task Configure_TerminatingTypeGivenAsParameter_RegistersTerminatingType()
    {
        testChainConfigurator.Add(typeof(TerminatingMember)).Configure();

        await Assert.That(testChainConfigurator.Get<IChainMember>()).IsTypeOf<TerminatingMember>();
    }

    [Test]
    public void Configure_TypeGivenThatHasNoPublicConstructor_ThrowsException()
    {
        Assert.Throws<InvalidOperationException>(() => testChainConfigurator.Add(typeof(MemberWithoutPublicConstructor)).Configure());
    }

    [Test]
    public void Configure_TypeGivenThatHasMoreThanOnePublicConstructor_ThrowsException()
    {
        Assert.Throws<InvalidOperationException>(() => testChainConfigurator.Add(typeof(MemberWithMultipleConstructors)).Configure());
    }

    [Test]
    public void Add_TypeGivenThatIsNotAChainMember_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => testChainConfigurator.Add(typeof(MemberThatIsNotAChainMember)).Configure());
    }

    [Test]
    public async Task Add_TypeThatAlsoRequiresANonChainMemberType_UsesAlreadyExistingRegistrationForOtherType()
    {
        // Arrange
        testChainConfigurator.Register("Hello World!");

        // Act
        testChainConfigurator.Add<MemberThatAlsoRequiresANonMember>().Add<TerminatingMember>().Configure();

        // Assert
        var registeredObject = testChainConfigurator.Get<IChainMember>() as MemberThatAlsoRequiresANonMember;
        await Assert.That(registeredObject).IsNotNull();
        await Assert.That(registeredObject.SomeString).EqualTo("Hello World!");
        await Assert.That(registeredObject.Next).IsTypeOf<TerminatingMember>();
    }

    private sealed class TestChainConfigurator() : AbstractChainConfigurator<IChainMember, TestChainConfigurator.Factory, int>(0)
    {
        public delegate object? Factory();
        private readonly IDictionary<Type, RegisteredType> registeredTypes = new Dictionary<Type, RegisteredType>();

        protected override void Register(Type targetType, Factory factory, int lifestyle)
        {
            registeredTypes[targetType] = new(Lifestyle: lifestyle, GetInstance: factory);
        }

        protected override Factory CreateFactory(ConstructorInfo constructor, IReadOnlyList<Type?> parameterTypes)
        {
            return () => constructor.Invoke(parameterTypes.Select(t => t is null ? null : registeredTypes.TryGetValue(t, out var type) ? type.GetInstance() : null).ToArray());
        }

        public void Register<T>(T value) where T : notnull
        {
            registeredTypes.Add(key: typeof(T), value: new(Lifestyle: 0, GetInstance: () => value));
        }

        public RegisteredType? GetRegistrationFor<T>()
        {
            return registeredTypes.TryGetValue(key: typeof(T), value: out var registeredType) ? registeredType : null;
        }

        public T? Get<T>()
        {
            return (T?)GetRegistrationFor<T>()?.GetInstance();
        }

        public sealed record RegisteredType(int Lifestyle, Factory GetInstance);
    }
}
