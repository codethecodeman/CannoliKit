# Installation

## CannoliKit
Release builds of CannoliKit can be added to your project via NuGet:
- (Coming soon)

## Microsoft Entity Framework Core (EF Core)
CannoliKit uses [EF Core](https://learn.microsoft.com/en-us/ef/core/) to support its various features. 

You will need to add the following packages to your project:
- [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore)
- [Microsoft.EntityFrameworkCore.Tools](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Tools)

EF Core needs a database provider. You may choose your own. For many projects, SQLite is a great fit:
- [Microsoft.EntityFrameworkCore.Sqlite](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite)

## Microsoft Dependency Injection (DI)
CannoliKit is designed to use [DI](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection). 

You will need to add the following packages to your project:
- [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection)
- [Microsoft.Extensions.DependencyInjection.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Abstractions)

## Logging
CannoliKit requires you add an `ILogger` service to your DI service container. If you want to log to the console, use:
- [Microsoft.Extensions.Logging.Console](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Console)

## Next Steps

Once you have added packages to your project, you are ready to [Get Started](getting-started/database.md). Or, if you are an experienced developer and want a quick summary, jump to [TL;DR](getting-started/tldr.md).