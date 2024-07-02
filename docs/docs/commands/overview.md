# Commands Overview
**Cannoli Commands** allow you to define Discord global application commands and then handle their interactions using dependency injection (DI).

## Discord.Net Interaction Framework
 **While CannoliKit supports commands, you probably should not use them.** Discord.Net has a powerful feature called [Interaction Framework](https://docs.discordnet.dev/guides/int_framework/intro.html) that can be used to create many kinds of commands and interactions. Very likely, it will better suit your needs.

 ## Using Cannoli Commands

To create a command, your class needs to implement `ICannoliCommand`. This will require you to specify a `Name` for the command, and a `DeferralType` which allows you to optionally auto-defer. It will also require you to implement two methods, `BuildAsync()` to set up your command properties, and `RespondAsync(CannoliCommandContext context)` to handle interactions. The command will be automatically discovered at startup and registered with Discord.

## Lifetime

Cannoli Commands are transient. When a new interaction arrives for a command, a new instance of the class will be created using DI. If you need to access shared variables across requests, you may need to implement a singleton service.

## Example

```csharp
public class ProfileCommand : ICannoliCommand
{
    public string Name => "profile";
    public DeferralType DeferralType => DeferralType.Ephemeral;

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

    public async Task<ApplicationCommandProperties> BuildAsync()
    {
        var builder = new SlashCommandBuilder()
        {
            Description = "View your profile settings",
            ContextTypes = [
                InteractionContextType.Guild,
            ],
            DefaultMemberPermissions = GuildPermission.Administrator,
        };

        await Task.CompletedTask;

        return builder.Build();
    }
}
```

## Autocomplete
Your class can additionally implement `ICannoliAutocompleteCommand` to handle Discord autocomplete interactions.
