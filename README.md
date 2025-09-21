# Syrette
_An extremely tiny dependency injection C# library_

Syrette is a minimalistic dependency injection library for C#.
It aims to provide a simple and efficient way to achieve dependency injections in your applications without the overhead of larger frameworks.

It is designed to be used in minimalistic applications, such as console applications or small services, where simplicity and ease of implementation are preferred over extensive feature sets and robust systems.

## Features
- **Lightweight**: Minimal codebase with no external dependencies. 1 class, the service container. You don't need anything else.
- **Simple API**: You register services using two methods (one for singletons, one for transients) and resolve them with one.
- **Supports Singleton and Transient lifetimes**: Choose between singleton (one instance per container) and transient (new instance per resolution) lifetimes for your services.
- **Greedy matching constructor selection**: When resolving a service, the constructor with the most parameters that can be satisfied by the container is chosen.

## Limitations
- **No support for scoped lifetimes**
- **No support for property injection or method injection.**

## Usage
````csharp
//istantiate your service container
var container = new Syrette.ServiceContainer();
//register your services
container.RegisterSingleton<IMyService, MyService>();
container.RegisterTransient<IOtherService, OtherService>();
//you can also use fluent syntax
container.RegisterSingleton<IMyService, MyService>()
         .RegisterTransient<IOtherService, OtherService>()
         .RegisterSingleton<IThirdService, ThirdService>();

//resolve your services
var myService = container.GetService<IMyService>();
````

## Licence
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.