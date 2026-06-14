namespace Syrette.Tests;

public class ServiceContainerRegistrationTests
{
    [Fact]
    public void AddSingleton_TInterface_TImplementation_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        Assert.NotNull(container.GetService<ITestService>());
    }

    [Fact]
    public void AddSingleton_TInterface_TImplementation_with_args_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestDeepService>(Guid.NewGuid());
        Assert.NotNull(container.GetService<ITestService>());
    }

    [Fact]
    public void AddSingleton_TClass_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddSingleton<TestServiceImpl>();
        Assert.NotNull(container.GetService<TestServiceImpl>());
    }

    [Fact]
    public void AddSingleton_TClass_with_args_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddSingleton<TestDeepService>(Guid.NewGuid());
        Assert.NotNull(container.GetService<TestDeepService>());
    }

    [Fact]
    public void AddTransient_TInterface_TImplementation_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddTransient<ITestService, TestServiceImpl>();
        Assert.NotNull(container.GetService<ITestService>());
    }

    [Fact]
    public void AddTransient_TInterface_TImplementation_with_args_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddTransient<ITestService, TestDeepService>(Guid.NewGuid());
        Assert.NotNull(container.GetService<ITestService>());
    }

    [Fact]
    public void AddTransient_TClass_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddTransient<TestServiceImpl>();
        Assert.NotNull(container.GetService<TestServiceImpl>());
    }

    [Fact]
    public void AddTransient_TClass_with_args_registers_successfully()
    {
        var container = new ServiceContainer();
        container.AddTransient<TestDeepService>(Guid.NewGuid());
        Assert.NotNull(container.GetService<TestDeepService>());
    }

    [Fact]
    public void Fluent_chaining_returns_same_container()
    {
        var container = new ServiceContainer();
        var result = container
            .AddSingleton<ITestService, TestServiceImpl>()
            .AddTransient<TestServiceImpl>();
        Assert.Same(container, result);
    }

    [Fact]
    public void Duplicate_registration_same_pair_throws()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        Assert.Throws<InvalidOperationException>(() =>
            container.AddSingleton<ITestService, TestServiceImpl>());
    }

    [Fact]
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
