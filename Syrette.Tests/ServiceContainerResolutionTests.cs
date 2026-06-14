namespace Syrette.Tests;

public class ServiceContainerResolutionTests
{
    [Fact]
    public void GetService_T_resolves_singleton()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        var instance = container.GetService<ITestService>();
        Assert.NotNull(instance);
        Assert.IsType<TestServiceImpl>(instance);
    }

    [Fact]
    public void GetService_T_resolves_transient()
    {
        var container = new ServiceContainer();
        container.AddTransient<ITestServiceB, TestServiceBImpl>();
        var instance = container.GetService<ITestServiceB>();
        Assert.NotNull(instance);
        Assert.IsType<TestServiceBImpl>(instance);
    }

    [Fact]
    public void Singleton_returns_same_instance()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        var a = container.GetService<ITestService>();
        var b = container.GetService<ITestService>();
        Assert.Same(a, b);
    }

    [Fact]
    public void Transient_returns_new_instance()
    {
        var container = new ServiceContainer();
        container.AddTransient<ITestServiceB, TestServiceBImpl>();
        var a = container.GetService<ITestServiceB>();
        var b = container.GetService<ITestServiceB>();
        Assert.NotSame(a, b);
    }

    [Fact]
    public void GetService_Type_non_generic_resolves()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        var instance = container.GetService(typeof(ITestService));
        Assert.NotNull(instance);
        Assert.IsType<TestServiceImpl>(instance);
    }

    [Fact]
    public void GetService_Type_with_args_resolves()
    {
        var container = new ServiceContainer();
        container.AddTransient<ITestService, TestDeepService>();
        var instance = container.GetService(typeof(ITestService), new object[] { Guid.NewGuid() });
        Assert.NotNull(instance);
    }

    [Fact]
    public void GetService_T_throws_for_unregistered()
    {
        var container = new ServiceContainer();
        Assert.Throws<InvalidOperationException>(() =>
            container.GetService<ITestService>());
    }

    [Fact]
    public void GetService_Type_throws_for_unregistered()
    {
        var container = new ServiceContainer();
        Assert.Throws<InvalidOperationException>(() =>
            container.GetService(typeof(ITestService)));
    }

    [Fact]
    public void TryGetService_returns_null_for_unregistered()
    {
        var container = new ServiceContainer();
        var result = container.TryGetService(typeof(ITestService));
        Assert.Null(result);
    }

    [Fact]
    public void TryGetService_returns_instance_when_registered()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        var result = container.TryGetService(typeof(ITestService));
        Assert.NotNull(result);
    }

    [Fact]
    public void Resolve_by_implementation_type_when_not_registered_directly()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        var instance = container.GetService(typeof(TestServiceImpl));
        Assert.NotNull(instance);
    }

    [Fact]
    public void Resolution_time_args_override_registration_time_args()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestDeepService>(Guid.Empty);
        var resolved = container.GetService(typeof(ITestService), new object[] { Guid.NewGuid() });
        Assert.NotNull(resolved);
        var deep = (TestDeepService)resolved;
        Assert.NotEqual(Guid.Empty, deep.Id);
    }

    [Fact]
    public void GetServices_returns_all_registered_implementations()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        container.AddSingleton<ITestService, TestServiceAlt>();
        var services = container.GetServices<ITestService>();
        Assert.Equal(2, services.Count);
        var types = services.Select(s => s.GetType()).ToList();
        Assert.Contains(typeof(TestServiceImpl), types);
        Assert.Contains(typeof(TestServiceAlt), types);
    }

    [Fact]
    public void GetServiceTypes_returns_all_implementation_types()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        container.AddSingleton<ITestService, TestServiceAlt>();
        var types = container.GetServiceTypes<ITestService>();
        Assert.Equal(2, types.Count);
        Assert.Contains(typeof(TestServiceImpl), types);
        Assert.Contains(typeof(TestServiceAlt), types);
    }
}
