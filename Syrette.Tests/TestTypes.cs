namespace Syrette.Tests;

public interface ITestService { }
public class TestServiceImpl : ITestService { }
public class TestServiceAlt : ITestService { }

public interface ITestServiceB { }
public class TestServiceBImpl : ITestServiceB { }

public interface ITestServiceC { }
public class TestServiceCImpl : ITestServiceC { }

public class TestMultiCtor
{
    public ITestService? A { get; }
    public ITestServiceB? B { get; }
    public ITestServiceC? C { get; }

    public TestMultiCtor(ITestService a) { A = a; }
    public TestMultiCtor(ITestService a, ITestServiceB b) { A = a; B = b; }
    public TestMultiCtor(ITestService a, ITestServiceB b, ITestServiceC c) { A = a; B = b; C = c; }
}

public class TestDeepService : ITestService
{
    public Guid Id { get; }

    public TestDeepService(Guid id) { Id = id; }
}

public class TestDisposableService : IDisposable
{
    public bool IsDisposed { get; private set; }
    public void Dispose() => IsDisposed = true;
}
