# EF Core Setup

> [!TIP]
> If you are new to Entity Framework Core (EF Core), check out Microsoft's [guide](https://learn.microsoft.com/en-us/ef/core/).

## Introduction
CannoliKit uses EF Core to support its various features. It can also be useful to your project, allowing you to store and query information from a database. 

Any project that uses CannoliKit must define a derived `DbContext` that implements `ICannoliDbContext`.

## Example

Suppose your bot will let Discord users play games and keep track of scores. Your derived `DbContext` might look like this:

```csharp
public class GameNightDbContext : DbContext, ICannoliDbContext
{
    // These two DbSets are required by ICannoliDbContext and are used by CannoliKit.
    public DbSet<CannoliSaveState> CannoliSaveStates { get; set; } = null!;
    public DbSet<CannoliRoute> CannoliRoutes { get; set; } = null!;

    // These DbSets are used by your new bot idea, Game Night.
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Score> Scores { get; set; } = null!;
}
```

## Next Steps
Once you have created your custom DbContext, continue on to [DI Setup](dependency-injection.md).