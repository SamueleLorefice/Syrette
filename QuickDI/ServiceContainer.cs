using System.Reflection;

namespace QuickDI;

public class ServiceContainer {
    private List<ServiceDescriptor> _descriptors = new();
    private Dictionary<Type, object> _singletons = new();

    /// <summary>
    /// Get all registered implementation types for a given service type.
    /// </summary>
    /// <typeparam name="TServices"></typeparam>
    /// <returns></returns>
    public List<Type> GetServices<TServices>() => 
        _descriptors.Where(d => d.ServiceType == typeof(TServices))
            .Select(d => d.ImplementationType).ToList();
    
    public ServiceContainer AddSingleton<TInterface, TImplementation>()
        where TImplementation : class, TInterface {
        _descriptors.Add(new ServiceDescriptor {
            ServiceType = typeof(TInterface),
            ImplementationType = typeof(TImplementation),
            Lifetime = ServiceLifetime.Lifetime,
            RequiredTypes = typeof(TImplementation).GetConstructors().Single()
                .GetParameters().Select(p => p.ParameterType).ToList()
        });
        return this;
    }

    public ServiceContainer AddTransient<TInterface, TImplementation>()
        where TImplementation : class, TInterface {
        _descriptors.Add(new ServiceDescriptor {
            ServiceType = typeof(TInterface),
            ImplementationType = typeof(TImplementation),
            Lifetime = ServiceLifetime.Transient,
            RequiredTypes = typeof(TImplementation).GetConstructors().Single()
                .GetParameters().Select(p => p.ParameterType).ToList()
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
    
    public TInterface GetService<TInterface>() {
        var descriptor = _descriptors.FirstOrDefault(d => d.ServiceType == typeof(TInterface));
        
        if (descriptor == null) throw new Exception($"Service of type {typeof(TInterface)} not registered.");
        
        // Ensure all required dependencies are registered
        //TODO: some services might be asking for specific implementations, not interfaces. We should check for that too.
        var missing = descriptor.RequiredTypes
            //filter all required types that are not in the registered descriptors
            .Where(t => _descriptors.All(d => d.ServiceType != t))
            .Select(t => t.Name)
            .ToList();
        
        if (missing.Any())
            throw new Exception($"Cannot create service of type {typeof(TInterface)}. Missing dependencies: {string.Join(", ", missing)}");
        
        // Transient: create a new instance each time
        if (descriptor.Lifetime != ServiceLifetime.Lifetime) {
            var service = Instantiate<TInterface>(descriptor);
            return service;
        }

        // Singleton: return existing instance
        if (_singletons.TryGetValue(descriptor.ServiceType, out object? singleton)) return (TInterface)singleton;
        
        // or create a new one if not yet created.
        var newSingleton = Instantiate<TInterface>(descriptor);
        _singletons[descriptor.ServiceType] = newSingleton!;
        return newSingleton;
    }

    private TInterface Instantiate<TInterface>(ServiceDescriptor descriptor) {
        var par = descriptor.ImplementationType
            .GetConstructors().Single()
            .GetParameters()
            .Select(p => p.ParameterType)
            .ToList();
        
        object[] parameters = new object[par.Count];
        
        for (int i = 0; i < par.Count; i++)
            parameters[i] = GetService(par[i]);
        
        var service = (TInterface?)Activator.CreateInstance(descriptor.ImplementationType, parameters);

        return service ?? throw new Exception($"Could not create instance of type {descriptor.ImplementationType}");
    }
}