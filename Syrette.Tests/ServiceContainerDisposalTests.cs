namespace Syrette.Tests;

public class ServiceContainerDisposalTests
{
    [Fact]
    public void Dispose_disposes_singletons_implementing_IDisposable()
    {
        var container = new ServiceContainer();
        container.AddSingleton<TestDisposableService>();
        var instance = container.GetService<TestDisposableService>();

        container.Dispose();
        Assert.True(instance.IsDisposed);
    }

    [Fact]
    public void Dispose_clears_singleton_cache()
    {
        var container = new ServiceContainer();
        container.AddSingleton<TestDisposableService>();
        container.GetService<TestDisposableService>();

        container.Dispose();

        Assert.Throws<InvalidOperationException>(() =>
            container.GetService<TestDisposableService>());
    }

    [Fact]
    public void Multiple_dispose_is_safe()
    {
        var container = new ServiceContainer();
        container.AddSingleton<TestDisposableService>();
        container.GetService<TestDisposableService>();

        container.Dispose();
        container.Dispose();
        Assert.True(true);
    }
}
