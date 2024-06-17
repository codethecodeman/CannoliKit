# Module Alerts

Cannoli Modules have built-in support for alert messages. This will automatically add information or error messages above your Discord embed.

## Basic Usage

```csharp
Alerts.SetInfoMessage(
    "Select an item from the list.");

Alerts.SetErrorMessage(
    "The value you entered is not valid.");
```

## Advanced Usage

```csharp
Alerts.SetInfoMessage(
  content: "Select an item from the menu to add it to your cart.",
  emoji: new Emoji("🛒"),
  color: Color.Green);
```