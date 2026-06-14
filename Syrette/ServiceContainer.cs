using System.Reflection;

namespace Syrette;

public class ServiceContainer : IDisposable {
    private readonly List<ServiceDescriptor> descriptors = new();
    private readonly Dictionary<Type, object> singletons = new();
    private readonly object singletonLock = new();

    public List<Type> GetServiceTypes<TServices>() =>
        descriptors.Where(d => d.ServiceType == typeof(TServices))
            .Select(d => d.ImplementationType).ToList();

    public List<TService> GetServices<TService>() where TService : class =>
        descriptors.Where(d => d.ServiceType == typeof(TService))
            .Select(d => (TService)ResolveService(d.ImplementationType, null)).ToList();

    public ServiceContainer AddSingleton<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface {
        AddDescriptor(typeof(TInterface), typeof(TImplementation), ServiceLifetime.Singleton, null);
        return this;
    }

    public ServiceContainer AddSingleton<TInterface, TImplementation>(params object[] args)
        where TInterface : class
        where TImplementation : class, TInterface {
        AddDescriptor(typeof(TInterface), typeof(TImplementation), ServiceLifetime.Singleton, args);
        return this;
    }

    public ServiceContainer AddSingleton<TClass>()
        where TClass : class {
        AddDescriptor(typeof(TClass), typeof(TClass), ServiceLifetime.Singleton, null);
        return this;
    }

    public ServiceContainer AddSingleton<TImplementation>(params object[] args)
        where TImplementation : class {
        AddDescriptor(typeof(TImplementation), typeof(TImplementation), ServiceLifetime.Singleton, args);
        return this;
    }

    public ServiceContainer AddTransient<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface {
        AddDescriptor(typeof(TInterface), typeof(TImplementation), ServiceLifetime.Transient, null);
        return this;
    }

    public ServiceContainer AddTransient<TInterface, TImplementation>(params object[] args)
        where TInterface : class
        where TImplementation : class, TInterface {
        AddDescriptor(typeof(TInterface), typeof(TImplementation), ServiceLifetime.Transient, args);
        return this;
    }

    public ServiceContainer AddTransient<TClass>()
        where TClass : class {
        AddDescriptor(typeof(TClass), typeof(TClass), ServiceLifetime.Transient, null);
        return this;
    }

    public ServiceContainer AddTransient<TClass>(params object[] args)
        where TClass : class {
        AddDescriptor(typeof(TClass), typeof(TClass), ServiceLifetime.Transient, args);
        return this;
    }

    private void AddDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, object[]? args) {
        if (descriptors.Any(d => d.ServiceType == serviceType && d.ImplementationType == implementationType)) {
            throw new InvalidOperationException(
                $"A registration for '{implementationType.Name}' as '{serviceType.Name}' already exists.");
        }

        descriptors.Add(new ServiceDescriptor {
            ServiceType = serviceType,
            ImplementationType = implementationType,
            Lifetime = lifetime,
            Arguments = args?.ToList()
        });
    }

    public object GetService(Type serviceType, object[]? args = null) {
        return ResolveService(serviceType, args);
    }

    public TService GetService<TService>() {
        return (TService)ResolveService(typeof(TService), null);
    }

    public object? TryGetService(Type serviceType, object[]? args = null) {
        try {
            return ResolveService(serviceType, args);
        }
        catch (InvalidOperationException) {
            return null;
        }
    }

    private object ResolveService(Type serviceType, object[]? resolutionArgs) {
        var descriptor = descriptors.FirstOrDefault(d =>
            d.ServiceType == serviceType || d.ImplementationType == serviceType);

        if (descriptor == null) {
            throw new InvalidOperationException(
                $"Service of type '{serviceType.Name}' not registered.");
        }

        var mergedArgs = descriptor.Arguments != null
            ? new List<object>(descriptor.Arguments)
            : new List<object>();

        if (resolutionArgs != null) {
            foreach (var arg in resolutionArgs) {
                var argType = arg.GetType();
                var index = mergedArgs.FindIndex(a => a.GetType() == argType);

                if (index >= 0) {
                    mergedArgs[index] = arg;
                } else {
                    mergedArgs.Add(arg);
                }
            }
        }

        var ctors = descriptor.ImplementationType.GetConstructors();
        ConstructorInfo? bestCtor = null;
        int max = -1;

        foreach (var ctor in ctors) {
            var parameters = ctor.GetParameters();

            if (parameters.Any(p =>
                    descriptors.All(d => d.ServiceType != p.ParameterType) &&
                    mergedArgs.All(a => !p.ParameterType.IsAssignableFrom(a.GetType())) &&
                    !p.IsOptional)) {
                continue;
            }

            int satisfied = parameters.Count(p =>
                descriptors.Any(d => d.ServiceType == p.ParameterType));

            int argSatisfied = 0;

            foreach (var param in parameters) {
                if (!descriptors.Any(d => d.ServiceType == param.ParameterType) &&
                    mergedArgs.Any(a => param.ParameterType.IsAssignableFrom(a.GetType()))) {
                    argSatisfied++;
                }
            }

            satisfied += argSatisfied;

            if (satisfied > max) {
                max = satisfied;
                bestCtor = ctor;
            }
        }

        if (bestCtor == null) {
            throw new InvalidOperationException(
                $"Cannot create service of type '{serviceType.Name}'. No suitable constructor found.");
        }

        if (descriptor.Lifetime == ServiceLifetime.Singleton) {
            lock (singletonLock) {
                if (singletons.TryGetValue(descriptor.ImplementationType, out var singleton)) {
                    return singleton;
                }

                var instance = Instantiate(descriptor.ImplementationType, bestCtor, mergedArgs);
                singletons[descriptor.ImplementationType] = instance;
                return instance;
            }
        }

        return Instantiate(descriptor.ImplementationType, bestCtor, mergedArgs);
    }

    private object Instantiate(Type implementationType, ConstructorInfo ctor, List<object> args) {
        var parameters = ctor.GetParameters();
        var resolvedParams = new object?[parameters.Length];
        var usedArgs = new List<object>();

        for (var i = 0; i < parameters.Length; i++) {
            var paramType = parameters[i].ParameterType;

            var arg = args.FirstOrDefault(a =>
                !usedArgs.Contains(a) && paramType.IsAssignableFrom(a.GetType()));

            if (arg != null) {
                resolvedParams[i] = arg;
                usedArgs.Add(arg);
                continue;
            }

            var ctorArg = TryGetService(paramType);
            if (ctorArg != null) {
                resolvedParams[i] = ctorArg;
                continue;
            }

            if (parameters[i].IsOptional) {
                resolvedParams[i] = parameters[i].DefaultValue;
                continue;
            }

            throw new InvalidOperationException(
                $"Cannot resolve parameter '{parameters[i].Name}' of type '{paramType.Name}' for service '{implementationType.Name}'.");
        }

        var instance = Activator.CreateInstance(implementationType, resolvedParams);

        return instance ??
               throw new InvalidOperationException(
                   $"Could not create instance of type '{implementationType.Name}'.");
    }

    public void Dispose() {
        foreach (var disposable in singletons.Values.OfType<IDisposable>()) {
            disposable.Dispose();
        }

        singletons.Clear();
        descriptors.Clear();
    }
}