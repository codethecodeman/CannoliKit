# Sending Modules To The User

To send a new module to the user, or respond to an interaction with a new module, you have two options.

## Option 1

Use an extension method which accepts a Cannoli Module.

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

## Option 2

Call BuildComponentsAsync() directly and plug the provided components into your response.

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
        var moduleComponents = await module.BuildComponentsAsync();

        await messageComponent.Channel.SendMessageAsync(
            text: moduleComponents.Text,
            embeds: moduleComponents.Embeds,
            components: moduleComponents.MessageComponent);
    }

    // ...
}
```