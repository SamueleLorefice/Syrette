namespace Syrette.Tests;

public class ServiceContainerRegistrationTests
{
    [Fact(DisplayName = "AddSingleton<TInterface, TImplementation> registers a service that can be resolved")]
    public void AddSingleton_TInterface_TImplementation_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        Assert.NotNull(container.GetService<ITestService>());
    }

    [Fact(DisplayName = "AddSingleton<TInterface, TImplementation> with constructor args registers and resolves")]
    public void AddSingleton_TInterface_TImplementation_with_args_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestDeepService>(Guid.NewGuid());
        Assert.NotNull(container.GetService<ITestService>());
    }

    [Fact(DisplayName = "AddSingleton<TClass> (self-registration) registers and resolves")]
    public void AddSingleton_TClass_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddSingleton<TestServiceImpl>();
        Assert.NotNull(container.GetService<TestServiceImpl>());
    }

    [Fact(DisplayName = "AddSingleton<TClass> with constructor args registers and resolves")]
    public void AddSingleton_TClass_with_args_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddSingleton<TestDeepService>(Guid.NewGuid());
        Assert.NotNull(container.GetService<TestDeepService>());
    }

    [Fact(DisplayName = "AddTransient<TInterface, TImplementation> registers a service that can be resolved")]
    public void AddTransient_TInterface_TImplementation_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddTransient<ITestService, TestServiceImpl>();
        Assert.NotNull(container.GetService<ITestService>());
    }

    [Fact(DisplayName = "AddTransient<TInterface, TImplementation> with constructor args registers and resolves")]
    public void AddTransient_TInterface_TImplementation_with_args_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddTransient<ITestService, TestDeepService>(Guid.NewGuid());
        Assert.NotNull(container.GetService<ITestService>());
    }

    [Fact(DisplayName = "AddTransient<TClass> (self-registration) registers and resolves")]
    public void AddTransient_TClass_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddTransient<TestServiceImpl>();
        Assert.NotNull(container.GetService<TestServiceImpl>());
    }

    [Fact(DisplayName = "AddTransient<TClass> with constructor args registers and resolves")]
    public void AddTransient_TClass_with_args_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddTransient<TestDeepService>(Guid.NewGuid());
        Assert.NotNull(container.GetService<TestDeepService>());
    }

    [Fact(DisplayName = "Fluent chaining returns the same ServiceContainer instance")]
    public void Fluent_chaining_returns_same_container()
    {
        var container = new ServiceContainer();
        var result = container
            .AddSingleton<ITestService, TestServiceImpl>()
            .AddTransient<TestServiceImpl>();
        Assert.Same(container, result);
    }

    [Fact(DisplayName = "Registering the same (ServiceType, ImplementationType) pair twice throws InvalidOperationException")]
    public void Duplicate_registration_same_pair_throws()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        Assert.Throws<InvalidOperationException>(() =>
            container.AddSingleton<ITestService, TestServiceImpl>());
    }

    [Fact(DisplayName = "Registering two different implementations for the same service type is allowed and both are resolvable")]
    public void Duplicate_registration_different_impl_for_same_service_allowed()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        container.AddSingleton<ITestService, TestServiceAlt>();

        var services = container.GetServiceTypes<ITestService>();
        Assert.Equal(2, services.Count);
        Assert.Contains(typeof(TestServiceImpl), services);
        Assert.Contains(typeof(TestServiceAlt), services);
    }
}
