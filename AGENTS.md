# Syrette — agent guide

Minimal C# DI library targeting net8.0;net9.0;net10.0.

## Build commands

```bash
dotnet build Syrette          # library only
dotnet build                  # everything (includes DISandbox console app)
dotnet pack Syrette -c Release --output .
```

SDK: .NET 10.0+ (see `global.json` — `rollForward: latestMinor`, `allowPrerelease: true`).

## Project structure

| Path | Purpose |
|---|---|
| `Syrette/` | Library (class lib). Entrypoints: `ServiceContainer.cs`, `ServiceDescriptor.cs`, `ServiceLifetime.cs` |
| `DISandbox/` | Unofficial manual-test sandbox. Console app referencing `Syrette`. Not part of the shipped package. |

No test project exists. CI has the test step commented out (`nuget-pkg-build.yml:24`).

## NuGet publishing

- CI (Gitea Actions) triggers on `v*` tags or `workflow_dispatch`.
- Pushes to both nuget.org and a Gitea package feed.
- Version is `0.0.1.8-alpha` per `Syrette.csproj`.

## Library API

- `ServiceContainer` exposes only `AddSingleton` / `AddTransient` (multiple overloads) and `GetService<T>()`, `GetService(Type, args)`, `TryGetService(Type, args)`, `GetServices<T>()`, `GetServiceTypes<T>()`.
- All registration methods return `ServiceContainer` for fluent chaining.
- Constructor selection is **greedy**: picks the constructor with the most parameters satisfiable by registered services or explicit args.
- Only Singleton and Transient lifetimes; no scoped, no property/method injection.
- `ServiceContainer : IDisposable` — disposes singleton instances that implement `IDisposable`.
- Duplicate registrations (same `(ServiceType, ImplementationType)`) throw `InvalidOperationException`.
- Resolution-time args (`GetService(Type, args)`) override registration-time args of the same type (type-based merge).
