namespace Syrette;

/// <summary>
/// Describes a service for dependency injection, including its type, implementation, lifetime, and required dependencies.
/// </summary>
public class ServiceDescriptor
{
    /// <summary>
    /// Gets or sets the type of the service to be provided.
    /// </summary>
    public required Type ServiceType { get; set; }

    /// <summary>
    /// Gets or sets the concrete type that implements the service.
    /// </summary>
    public required Type ImplementationType { get; set; }

    /// <summary>
    /// Gets or sets the lifetime of the service (e.g., Singleton or Transient).
    /// </summary>
    public required ServiceLifetime Lifetime { get; set; }

    /// <summary>
    /// Gets or sets the list of types required by the implementation (dependencies).
    /// </summary>
    public List<Type> RequiredTypes { get; set; } = new();
}