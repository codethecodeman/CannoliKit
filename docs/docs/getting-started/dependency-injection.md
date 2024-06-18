# DI Setup

> [!TIP]
> If you are new to dependency injection (DI), check out Microsoft's article [here](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection), and the Discord.Net guide [here](https://docs.discordnet.dev/guides/dependency_injection/basics.html).

## Introduction
CannoliKit is designed to use DI, which means your project must also be based around DI. 

## Example
Let's create a `Program.cs` that will set up our DI service container and initialize our services. You can find a full example in the [Demo project](https://github.com/codethecodeman/CannoliKit/tree/main/Demo).

### Create a new service collection

```csharp
public class Program
{
    public static Task Main() => new Program().MainAsync();

    internal async Task MainAsync()
    {
        // Create a new service collection.
        var collection = new ServiceCollection();
    }
}
```

### Add EF Core

```csharp
public class Program
{
    public static Task Main() => new Program().MainAsync();

    internal async Task MainAsync()
    {
        // ...

        // Add EF Core using your derived DbContext.
        // Here, we are using SQLite.
        // There are a lot of ways to set up the connection string. 
        // This is a simple example.
        collection.AddDbContext<GameNightDbContext>(opt =>
            opt.UseSqlite(@"Data Source=C:\Path\To\Your.db"));
    }
}
```

### Add Discord.Net socket client
See [Discord.Net documentation](https://docs.discordnet.dev/guides/dependency_injection/basics.html) for reference.

```csharp
public class Program
{
    public static Task Main() => new Program().MainAsync();

    internal async Task MainAsync()
    {
        // ...

        // Add a singleton DiscordSocketConfig.
        collection.AddSingleton(new DiscordSocketConfig()
        {
            // Your options here...
        });

        // Add a singleton DiscordSocketClient.
        // This will use the configuration above.
        collection.AddSingleton<DiscordSocketClient>();
    }
}
```

### Add Cannoli services

```csharp
public class Program
{
    public static Task Main() => new Program().MainAsync();

    internal async Task MainAsync()
    {
        // ...

        // Add Cannoli services.
        // You will need to indicate your derived DbContext type that implements ICannoliDbContext.
        collection.AddCannoliServices<GameNightDbContext>();
    }
}
```

### Add a logger

```csharp
public class Program
{
    public static Task Main() => new Program().MainAsync();

    internal async Task MainAsync()
    {
        // ...

        // Add a console logger.
        collection.AddLogging(c =>
        {
            c.SetMinimumLevel(LogLevel.Information);
            c.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            });
        });
    }
}
```

### Build service provider and initialize services

```csharp
public class Program
{
    public static Task Main() => new Program().MainAsync();

    internal async Task MainAsync()
    {
        // ...

        // Build a service provider.
        var serviceProvider = collection.BuildServiceProvider();

        // Get the Cannoli client service.
        var cannoliClient = serviceProvider.GetRequiredService<ICannoliClient>();

        // Set up Cannoli client. 
        // THIS MUST BE CALLED PRIOR TO CONNECTING YOUR BOT.
        await cannoliClient.SetupAsync();

        // Get the Discord socket client service.
        var discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();

        // Get a logger for your Discord client service.
        var discordClientLogger = serviceProvider.GetRequiredService<ILogger<DiscordSocketClient>>();

        // You might want to direct Discord socket client logging to your logger.
        // Here, we are using an extension method from the Demo project.
        discordClient.Log += m => discordClientLogger.HandleLogMessage(m);

        // Time to connect your bot! You will need to pass your token into the client.
        // Here, we are using a helper from the Demo project to get the token.
        await discordClient.LoginAsync(TokenType.Bot, ConfigurationHelper.GetDiscordToken());
        await discordClient.StartAsync();

        // Keep the application alive.
        await Task.Delay(-1);
    }
}
```

## Next Steps
Your bot is online and using CannoliKit! Now, it is time to implement features:
- [Cannoli Commands](../commands/overview.md)
- [Cannoli Processors](../processors/overview.md)
- [Cannoli Modules](../modules/introduction.md)