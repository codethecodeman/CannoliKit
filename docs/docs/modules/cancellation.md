# Module Cancellation

Cannoli Modules have built-in support for cancellation. This will automatically add a "Cancel" button to the nearest available component row whenever module refresh occurs. Using the button will cause the module to delete itself, along with its state.

## Enable

```csharp
Cancellation.IsEnabled = true;
```

## Custom Label
```csharp
Cancellation.ButtonLabel = "Exit";
```

## Custom Routing
Defining a custom route will override the default cancellation behavior. The module with neither delete itself nor its state.

```csharp
Cancellation.SetRoute(
  RouteManager.CreateMessageComponentRouteAsync(
    callback: OnExit));
```