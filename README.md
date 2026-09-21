# Swallow.ChainOfInjection

Behind the quite ominous name is hiding a very generic and abstract way of registering a structure that resembles a Chain of Responsibility to any
DI-container, like [Simple Injector](https://simpleinjector.org/) or [ServiceCollection](https://docs.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection?view=dotnet-plat-ext-3.1).

The original concept came from this really helpful [Answer on StackOverflow](https://stackoverflow.com/a/55476379), but I've changed quite a lot of it
to make it generic and less "Expression"-y. The general idea, however, remains the same.

## Packages

| Package                                                                                                                                                                                                                                                                                | Description                                                                                     |
|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------|
| [![Swallow.ChainOfInjection](https://img.shields.io/nuget/v/Swallow.ChainOfInjection?style=for-the-badge&logo=nuget&label=Swallow.ChainOfInjection)](https://www.nuget.org/packages/Swallow.ChainOfInjection/)                                                                         | Contains the functionality of chaining and the required adaptor a DI container needs to provide |
| [![Swallow.ChainOfInjection.ServiceCollection](https://img.shields.io/nuget/v/Swallow.ChainOfInjection.ServiceCollection?style=for-the-badge&logo=nuget&label=Swallow.ChainOfInjection.ServiceCollection)](https://www.nuget.org/packages/Swallow.ChainOfInjection.ServiceCollection/) | Bindings to use the chaining with `IServiceCollection`                                          |
| [![Swallow.ChainOfInjection.SimpleInjector](https://img.shields.io/nuget/v/Swallow.ChainOfInjection.SimpleInjector?style=for-the-badge&logo=nuget&label=Swallow.ChainOfInjection.SimpleInjector)](https://www.nuget.org/packages/Swallow.ChainOfInjection.SimpleInjector/)             | Bindings to use the chainng with `SimpleInjector`                                               |
