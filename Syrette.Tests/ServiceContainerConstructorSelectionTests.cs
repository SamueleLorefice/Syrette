namespace Syrette.Tests;

public class ServiceContainerConstructorSelectionTests
{
    public class CtorWithOptional
    {
        public ITestService? A { get; }
        public string? Name { get; }

        public CtorWithOptional(ITestService a, string? name = "default")
        {
            A = a;
            Name = name;
        }
    }

    public class CtorWithExactMatch
    {
        public int Value { get; }

        public CtorWithExactMatch(int value) { Value = value; }
        public CtorWithExactMatch(int value, string label) { Value = value; }
    }

    public class NoSatisfiableCtor
    {
        public NoSatisfiableCtor(int value) { }
    }

    [Fact]
    public void Greedy_picks_most_satisfiable_constructor()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        container.AddSingleton<ITestServiceB, TestServiceBImpl>();

        container.AddSingleton<TestMultiCtor>();
        var instance = container.GetService<TestMultiCtor>();

        Assert.NotNull(instance.A);
        Assert.NotNull(instance.B);
        Assert.Null(instance.C);
    }

    [Fact]
    public void Greedy_picks_all_satisfied_over_partial()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        container.AddSingleton<ITestServiceB, TestServiceBImpl>();
        container.AddSingleton<ITestServiceC, TestServiceCImpl>();

        container.AddSingleton<TestMultiCtor>();
        var instance = container.GetService<TestMultiCtor>();

        Assert.NotNull(instance.A);
        Assert.NotNull(instance.B);
        Assert.NotNull(instance.C);
    }

    [Fact]
    public void Greedy_falls_back_to_1_param_when_only_1_satisfiable()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();

        container.AddSingleton<TestMultiCtor>();
        var instance = container.GetService<TestMultiCtor>();

        Assert.NotNull(instance.A);
        Assert.Null(instance.B);
        Assert.Null(instance.C);
    }

    [Fact]
    public void Optional_parameter_used_when_not_registered()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        container.AddSingleton<CtorWithOptional>();

        var instance = container.GetService<CtorWithOptional>();
        Assert.NotNull(instance.A);
        Assert.Equal("default", instance.Name);
    }

    [Fact]
    public void Registration_args_satisfy_constructor()
    {
        var container = new ServiceContainer();
        container.AddSingleton<CtorWithExactMatch, CtorWithExactMatch>(42);

        var instance = container.GetService<CtorWithExactMatch>();
        Assert.Equal(42, instance.Value);
    }

    [Fact]
    public void Resolution_args_satisfy_constructor()
    {
        var container = new ServiceContainer();
        container.AddSingleton<CtorWithExactMatch, CtorWithExactMatch>();

        var instance = (CtorWithExactMatch)container.GetService(
            typeof(CtorWithExactMatch), new object[] { 99 });
        Assert.Equal(99, instance.Value);
    }

    [Fact]
    public void Resolution_args_override_registration_args()
    {
        var container = new ServiceContainer();
        container.AddSingleton<CtorWithExactMatch, CtorWithExactMatch>(10);

        var instance = (CtorWithExactMatch)container.GetService(
            typeof(CtorWithExactMatch), new object[] { 20 });
        Assert.Equal(20, instance.Value);
    }

    [Fact]
    public void No_suitable_constructor_throws()
    {
        var container = new ServiceContainer();
        container.AddSingleton<NoSatisfiableCtor>();
        Assert.Throws<InvalidOperationException>(() =>
            container.GetService<NoSatisfiableCtor>());
    }
}
