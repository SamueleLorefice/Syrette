namespace Syrette;

/// <summary>
/// Defines the lifetime of a service in dependency injection.
/// </summary>
public enum ServiceLifetime {
    /// <summary>
    /// Defines a singleton service, which is created once and shared throughout the application's lifetime.
    /// </summary>
    Lifetime,
    /// <summary>
    /// Defines a transient service, which is created anew each time it is requested.
    /// </summary>
    Transient
}