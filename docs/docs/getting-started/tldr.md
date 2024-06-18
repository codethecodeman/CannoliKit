# TL;DR

After [installing CannoliKit](../installation.md) and other related packages, you need to complete the following steps to set up your project:

- Define a derived `DbContext` that implements `ICannoliDbContext`, referred to below as `FooDbContext`.
- Implement dependency injection (DI). 
- Add `DiscordSocketClient` as a singleton service.
- Add a `DbContext` service using `FooDbContext`. Its lifetime is expected to be `Scoped`.
- Add Cannoli services using the `IServiceCollection.AddCannoliServices<T>()` extension method, where `T` is `FooDbContext`.
- Add an `ILogger` service.
- Prior to connecting your bot, get the `ICannoliClient` service and call the `SetupAsync()` method.

## Next Steps
Once you are set up, check out CannoliKit features: 
- [Cannoli Commands](../commands/overview.md)
- [Cannoli Processors](../processors/overview.md)
- [Cannoli Modules](../modules/introduction.md)