namespace Syrette.Tests;

public class ServiceContainerDisposalTests
{
    [Fact(DisplayName = "Dispose calls Dispose on singleton instances that implement IDisposable")]
    public void Dispose_disposes_singletons_implementing_IDisposable()
    {
        var container = new ServiceContainer();
        container.AddSingleton<TestDisposableService>();
        var instance = container.GetService<TestDisposableService>();

        container.Dispose();
        Assert.True(instance.IsDisposed);
    }

    [Fact(DisplayName = "Dispose clears the singleton cache so subsequent resolution throws")]
    public void Dispose_clears_singleton_cache()
    {
        var container = new ServiceContainer();
        container.AddSingleton<TestDisposableService>();
        container.GetService<TestDisposableService>();

        container.Dispose();

        Assert.Throws<InvalidOperationException>(() =>
            container.GetService<TestDisposableService>());
    }

    [Fact(DisplayName = "Calling Dispose multiple times does not throw")]
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
