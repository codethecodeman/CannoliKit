# Module Factory

To initialize a new module, inject `ICannoliModuleFactory` into your DI compatible class. You'll need to pass in a `SocketUser` that is initiating the interaction, and some optional configuration.

## Example

```csharp
public class ProfileCommand : ICannoliCommand
{
    private readonly ICannoliModuleFactory _moduleFactory;

    public ProfileCommand(
        ICannoliModuleFactory moduleFactory)
    {
        _moduleFactory = moduleFactory;
    }

    public async Task RespondAsync(CannoliCommandContext context)
    {
        var module = _moduleFactory.CreateModule<ProfileModule>(context.Command.User);
        await context.Command.FollowupAsync(module);
    }

    // ...
}
```