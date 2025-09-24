using System.Reflection;

namespace Syrette;

/// <summary>
/// Container for managing service registrations and resolutions.
/// </summary>
public class ServiceContainer {
    private readonly List<ServiceDescriptor> descriptors = new();
    private readonly Dictionary<Type, object> singletons = new();

    /// <summary>
    /// Get all registered implementation types for a given service type.
    /// </summary>
    /// <typeparam name="TServices"></typeparam>
    /// <returns></returns>
    public List<Type> GetServiceTypes<TServices>() =>
        descriptors.Where(d => d.ServiceType == typeof(TServices))
            .Select(d => d.ImplementationType).ToList();

    /// <summary>
    /// Get all registered services for a given service type.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public List<TService> GetServices<TService>() where TService : class =>
        descriptors.Where(d => d.ServiceType == typeof(TService))
            .Select(d => (TService)GetService(d.ImplementationType)).ToList();

    /// <summary>
    /// Registers a singleton service with its implementation.
    /// </summary>
    /// <typeparam name="TInterface">Interface the service is implementing</typeparam>
    /// <typeparam name="TImplementation">Implementation type of the service</typeparam>
    public ServiceContainer AddSingleton<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface {
        descriptors.Add(new() {
            ServiceType = typeof(TInterface),
            ImplementationType = typeof(TImplementation),
            Lifetime = ServiceLifetime.Singleton
        });
        return this;
    }

    /// <summary>
    /// Registers a singleton service with its implementation.
    /// </summary>
    /// <param name="args">Arguments to be passed to the constructor, in order of appearance if of the same type.</param>
    /// <typeparam name="TInterface">Interface the service is implementing</typeparam>
    /// <typeparam name="TImplementation">Implementation type of the service</typeparam>
    public ServiceContainer AddSingleton<TInterface, TImplementation>(params object[] args)
        where TInterface : class
        where TImplementation : class, TInterface {
        descriptors.Add(new() {
            ServiceType = typeof(TInterface),
            ImplementationType = typeof(TImplementation),
            Lifetime = ServiceLifetime.Singleton,
            Arguments = args.ToList()
        });
        return this;
    }

    /// <summary>
    /// Registers a singleton service where the service type is the same as the implementation type.
    /// </summary>
    /// <typeparam name="TClass">Class type of the service</typeparam>
    public ServiceContainer AddSingleton<TClass>()
        where TClass : class {
        descriptors.Add(new() {
            ServiceType = typeof(TClass),
            ImplementationType = typeof(TClass),
            Lifetime = ServiceLifetime.Singleton
        });
        return this;
    }

    /// <summary>
    /// Registers a singleton service with its implementation.
    /// </summary>
    /// <param name="args">Arguments to be passed to the constructor, in order of appearance if of the same type.</param>
    /// <typeparam name="TImplementation">Implementation type of the service</typeparam>
    public ServiceContainer AddSingleton<TImplementation>(params object[] args)
        where TImplementation : class {
        descriptors.Add(new() {
            ServiceType = typeof(TImplementation),
            ImplementationType = typeof(TImplementation),
            Lifetime = ServiceLifetime.Singleton,
            Arguments = args.ToList()
        });
        return this;
    }

    /// <summary>
    /// Registers a transient service with its implementation.
    /// </summary>
    /// <typeparam name="TInterface">Interface the service is implementing</typeparam>
    /// <typeparam name="TImplementation">Implementation type of the service</typeparam>
    public ServiceContainer AddTransient<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface {
        descriptors.Add(new() {
            ServiceType = typeof(TInterface),
            ImplementationType = typeof(TImplementation),
            Lifetime = ServiceLifetime.Transient
        });
        return this;
    }

    /// <summary>
    /// Registers a transient service with its implementation.
    /// </summary>
    /// <param name="args">Arguments to be passed to the constructor, in order of appearance if of the same type.</param>
    /// <typeparam name="TInterface">Interface the service is implementing</typeparam>
    /// <typeparam name="TImplementation">Implementation type of the service</typeparam>
    public ServiceContainer AddTransient<TInterface, TImplementation>(params object[] args)
        where TInterface : class
        where TImplementation : class, TInterface {
        descriptors.Add(new() {
            ServiceType = typeof(TInterface),
            ImplementationType = typeof(TImplementation),
            Lifetime = ServiceLifetime.Transient,
            Arguments = args.ToList()
        });
        return this;
    }

    /// <summary>
    /// Registers a transient service where the service type is the same as the implementation type.
    /// </summary>
    /// <typeparam name="TClass">Class type of the service</typeparam>
    public ServiceContainer AddTransient<TClass>()
        where TClass : class {
        descriptors.Add(new() {
            ServiceType = typeof(TClass),
            ImplementationType = typeof(TClass),
            Lifetime = ServiceLifetime.Transient
        });
        return this;
    }

    /// <summary>
    /// Registers a transient service where the service type is the same as the implementation type.
    /// </summary>
    /// <param name="args">Arguments to be passed to the constructor, in order of appearance if of the same type.</param>
    /// <typeparam name="TClass">Class type of the service</typeparam>
    public ServiceContainer AddTransient<TClass>(params object[] args)
        where TClass : class {
        descriptors.Add(new() {
            ServiceType = typeof(TClass),
            ImplementationType = typeof(TClass),
            Lifetime = ServiceLifetime.Transient,
            Arguments = args.ToList()
        });
        return this;
    }

    // you can't call generic methods with an unknown type at compile time
    // so we use reflection to call the generic GetService<T> method with the provided type
    // Basically we build the method GetService<serviceType>() at runtime and then call it.
    // "Classic black magic sorcery" in reflection.
    private object GetService(Type serviceType) {
        var method = typeof(ServiceContainer)
            .GetMethod(nameof(GetService))!
            .MakeGenericMethod(serviceType);
        return method.Invoke(this, null)!;
    }
    
    private object? TryGetService(Type serviceType) {
        var method = typeof(ServiceContainer)
            .GetMethod(nameof(GetService))!
            .MakeGenericMethod(serviceType);
        try {
            return method.Invoke(this, null)!;
        } catch {
            return null!;
        }
    }

    /// <summary>
    /// Resolves and returns an instance of the requested service type.
    /// </summary>
    /// <typeparam name="TService">Interface type of the service being requested</typeparam>
    /// <returns>Resolved service instance</returns>
    public TService GetService<TService>() {
        var descriptor = descriptors.FirstOrDefault(d => d.ServiceType == typeof(TService) || d.ImplementationType == typeof(TService));

        if (descriptor == null) throw new Exception($"Service of type {typeof(TService)} not registered.");

        var ctors = descriptor.ImplementationType.GetConstructors();
        var par = descriptor.Arguments ?? new List<object>();
        int max = -1;
        ConstructorInfo? bestCtor = null;

        foreach (var ctor in ctors) {
            var parameters = ctor.GetParameters();
            //check if all parameters are registered services or optional or have been provided as arguments
            if (parameters.Any(p => descriptors.All(d => d.ServiceType != p.ParameterType) && par.All(a => a.GetType() != p.ParameterType) && !p.IsOptional))
                continue;

            //check if this constructor has more registered parameters than the previous best
            int satisfiedParams = parameters.Count(p => descriptors.Any(d => d.ServiceType == p.ParameterType));
            satisfiedParams += par.Count(arg => parameters.Any(p => p.ParameterType == arg.GetType()));

            if (satisfiedParams > max) {
                max = satisfiedParams;
                bestCtor = ctor;
            }
        }

        if (bestCtor == null)
            throw new Exception($"Cannot create service of type {typeof(TService)}. No suitable constructor found.");

        // Transient: create a new instance each time
        if (descriptor.Lifetime != ServiceLifetime.Singleton) {
            var service = Instantiate<TService>(descriptor, bestCtor);
            return service;
        }

        // Singleton: return existing instance
        if (singletons.TryGetValue(descriptor.ServiceType, out object? singleton)) return (TService)singleton;

        // or create a new one if not yet created.
        var newSingleton = Instantiate<TService>(descriptor, bestCtor);
        singletons[descriptor.ServiceType] = newSingleton!;
        return newSingleton;
    }

    private TInterface Instantiate<TInterface>(ServiceDescriptor descriptor, ConstructorInfo? ctor = null) {
        if (ctor == null && descriptor.ImplementationType.GetConstructors().Length > 1)
            throw new Exception($"Multiple constructors found for type {descriptor.ImplementationType}. Please provide a specific constructor.");

        List<ParameterInfo> par;
        List<object> args = descriptor.Arguments ?? new List<object>();

        if (ctor == null)
            par = descriptor.ImplementationType
                .GetConstructors().Single()
                .GetParameters()
                //.Select(p => p.ParameterType)
                .ToList();
        else
            par = ctor.GetParameters()
                //.Select(p => p.ParameterType)
                .ToList();

        object[] parameters = new object[par.Count];

        for (int i = 0; i < par.Count; i++) {
            object? arg = args.FirstOrDefault(a => a.GetType() == par[i].ParameterType);
            if (arg != null) { // this parameter is satisfied by a provided argument
                parameters[i] = arg;
                args.Remove(arg); // remove to handle multiple parameters of the same type
                continue;
            }
            
            arg = TryGetService(par[i].ParameterType);
            if (arg != null) {
                // this parameter is satisfied by a registered service
                parameters[i] = arg;
                continue;
            }
            
            if (par[i].IsOptional) {
                // this parameter is optional and not provided, use default value
                parameters[i] = par[i].DefaultValue!;
                continue;
            }
            throw new Exception($"Cannot resolve parameter {par[i].Name} of type {par[i].ParameterType} for service {descriptor.ImplementationType}");
        }

        var service = (TInterface?)Activator.CreateInstance(descriptor.ImplementationType, parameters);

        return service ?? throw new Exception($"Could not create instance of type {descriptor.ImplementationType}");
    }
}