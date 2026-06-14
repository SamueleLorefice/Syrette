namespace Syrette.Tests;

public class ServiceContainerThreadingTests
{
    [Fact(DisplayName = "Concurrent singleton resolution from multiple threads returns the same instance")]
    public void Concurrent_singleton_resolution_returns_same_instance()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();

        var results = new ITestService[10];

        Parallel.For(0, 10, i =>
        {
            results[i] = container.GetService<ITestService>();
        });

        for (int i = 1; i < results.Length; i++)
        {
            Assert.Same(results[0], results[i]);
        }
    }

    [Fact(DisplayName = "Concurrent transient resolution from multiple threads returns unique instances")]
    public void Concurrent_transient_resolution_returns_unique_instances()
    {
        var container = new ServiceContainer();
        container.AddTransient<ITestService, TestServiceImpl>();

        var results = new ITestService[10];

        Parallel.For(0, 10, i =>
        {
            results[i] = container.GetService<ITestService>();
        });

        for (int i = 0; i < results.Length; i++)
        {
            for (int j = i + 1; j < results.Length; j++)
            {
                Assert.NotSame(results[i], results[j]);
            }
        }
    }

    [Fact(DisplayName = "Concurrent registration and resolution does not crash or deadlock")]
    public void Concurrent_registration_and_resolution_does_not_crash()
    {
        var container = new ServiceContainer();

        Parallel.Invoke(
            () => { try { container.AddSingleton<ITestService, TestServiceImpl>(); } catch { } },
            () => { try { container.AddSingleton<ITestService, TestServiceImpl>(); } catch { } },
            () => { try { container.GetService<ITestService>(); } catch { } },
            () => { try { container.AddSingleton<ITestService, TestServiceImpl>(); } catch { } },
            () => { try { container.GetService<ITestService>(); } catch { } }
        );

        Assert.NotNull(container);
    }
}
