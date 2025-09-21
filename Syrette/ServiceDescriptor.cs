namespace Syrette;

public class ServiceDescriptor {
    public required Type ServiceType { get; set; }
    public required Type ImplementationType { get; set; }
    public required ServiceLifetime Lifetime { get; set; }
    public List<Type> RequiredTypes { get; set; } = new();
}