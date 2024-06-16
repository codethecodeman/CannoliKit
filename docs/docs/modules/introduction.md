# Modules Overview

**Cannoli Modules** are multi-featured UI modules with a state and interactions that are persisted to a database. Think of them like stateful Discord embeds with containerized behavior that persist across application restarts. Module interactions by default have *single threaded guarantees*, so you can ensure module states are safely modified. 

These modules can be used to create many kinds of streamlined UI interactions.

## Lifecycle

![Module Lifecycle](../../images/ModuleLifecycle.svg "Module Lifecycle")

## Using Cannoli Modules

To create a module, create a class that inherits `CannoliModule<TContext, TState>` where:
- `TContext` is a derived `DbContext` that implements `ICannoliDbContext` (see [EF Core Setup](../getting-started/database.md)).
-  `TState` is a derived `CannoliModuleState`, and has a parameterless constructor `new()`.

This will require you to implement the method `BuildLayout()`, responsible for creating the Discord embed and components, like buttons, for this module any time it is refreshed. For comparison, if this were a 3D game engine, this method would be responsible for rendering the object.

## Utility Properties and Methods

The following utilities are inherited.

| Property    | Description |
| -------- | ------- |
| `DiscordClient`  | The corresponding `DiscordSocketClient`. |
| `Db` | An instance of your derived `DbContext`, e.g. `FooDbContext`. Changes are automatically saved. |
| `State`    | The module's persistent state. Changes are automatically saved. |
| `User`    | The `SocketUser` that started this interaction. |
| `Cancellation`    | Settings related to the user's ability to cancel/quit the module. |
| `Pagination`    | Settings related to setting up pagination. |
| `RouteManager`    | Utility for creating new Cannoli Routes. |
| `ReturnRoutes`    | Cannoli Routes that have been passed in from a referring module. |

| Method    | Description |
| -------- | ------- |
| `RefreshModule()`  | Modifies the Discord interaction's response with a refreshed module, using the `BuildLayout()` method. |

## Basic Setup

```csharp
internal class UserProfileModule : CannoliModule<FooDbContext, UserProfileState>
{
    public UserProfileModule(
        FooDbContext db,
        DiscordSocketClient discordClient,
        CannoliModuleFactoryConfiguration factoryConfiguration)
        : base(db, discordClient, factoryConfiguration) { }

    protected override async Task<CannoliModuleLayout> BuildLayout()
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = "Hello world!",
        };

        return new CannoliModuleLayout
        {
            EmbedBuilder = embedBuilder,
        };
    }
}
```


