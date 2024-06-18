# Module Expiration

Modules expire after a certain UTC date/time. By default, after 12 hours from initial creation. 

Their state will be automatically purged from the database. If a user attempts to interact with an expired module, the Discord message will update to indicate the expiration.

`Sorry, this interaction has expired. Please try again.`

## Setting Expiration Date/Time

You can control expiration through the inherited `State` property. For a rolling window, you can set the expiration within your `BuildLayout()` implementation or route callbacks.

```csharp
State.ExpiresOn = DateTime.UtcNow.AddDays(3);
```