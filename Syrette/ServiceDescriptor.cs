namespace Syrette;

/// <summary>
/// Describes a service for dependency injection, including its type, implementation, lifetime, and required dependencies.
/// </summary>
public class ServiceDescriptor
{
    /// <summary>
    /// Gets or sets the type of the service to be provided.
    /// </summary>
    public required Type ServiceType { get; init; }

    /// <summary>
    /// Gets or sets the concrete type that implements the service.
    /// </summary>
    public required Type ImplementationType { get; init; }

    /// <summary>
    /// Gets or sets the lifetime of the service (e.g., Singleton or Transient).
    /// </summary>
    public required ServiceLifetime Lifetime { get; init; }
    
    /// <summary>
    /// Arguments to be passed to the constructor of the implementation type.
    /// </summary>
    public List<object>? Arguments { get; init; }

    /// <summary>
    /// Returns a string with the specific type of service, its implementation, and its lifetime.
    /// </summary>
    /// <returns>{implementation Name} as {Service Name} ({Lifetime})</returns>
    public override string ToString() {
        return $"{ImplementationType.Name} as {ServiceType.Name} ({Lifetime})";
    }
}