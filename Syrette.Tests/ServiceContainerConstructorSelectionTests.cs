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

    [Fact(DisplayName = "Greedy constructor picks the ctor with the most parameters satisfiable by registered services")]
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

    [Fact(DisplayName = "Greedy constructor picks the ctor with all parameters satisfiable over a partial match")]
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

    [Fact(DisplayName = "Greedy constructor falls back to 1-param ctor when only 1 service is available")]
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

    [Fact(DisplayName = "Optional parameter uses its default value when the parameter type is not registered")]
    public void Optional_parameter_used_when_not_registered()
    {
        var container = new ServiceContainer();
        container.AddSingleton<ITestService, TestServiceImpl>();
        container.AddSingleton<CtorWithOptional>();

        var instance = container.GetService<CtorWithOptional>();
        Assert.NotNull(instance.A);
        Assert.Equal("default", instance.Name);
    }

    [Fact(DisplayName = "Registration-time args satisfy a constructor parameter by type matching")]
    public void Registration_args_satisfy_constructor()
    {
        var container = new ServiceContainer();
        container.AddSingleton<CtorWithExactMatch, CtorWithExactMatch>(42);

        var instance = container.GetService<CtorWithExactMatch>();
        Assert.Equal(42, instance.Value);
    }

    [Fact(DisplayName = "Resolution-time args satisfy a constructor parameter when no registration-time args exist")]
    public void Resolution_args_satisfy_constructor()
    {
        var container = new ServiceContainer();
        container.AddSingleton<CtorWithExactMatch, CtorWithExactMatch>();

        var instance = (CtorWithExactMatch)container.GetService(
            typeof(CtorWithExactMatch), new object[] { 99 });
        Assert.Equal(99, instance.Value);
    }

    [Fact(DisplayName = "Resolution-time args override registration-time args of the same type in constructor selection")]
    public void Resolution_args_override_registration_args()
    {
        var container = new ServiceContainer();
        container.AddSingleton<CtorWithExactMatch, CtorWithExactMatch>(10);

        var instance = (CtorWithExactMatch)container.GetService(
            typeof(CtorWithExactMatch), new object[] { 20 });
        Assert.Equal(20, instance.Value);
    }

    [Fact(DisplayName = "Throws InvalidOperationException when no constructor has all required parameters satisfiable")]
    public void No_suitable_constructor_throws()
    {
        var container = new ServiceContainer();
        container.AddSingleton<NoSatisfiableCtor>();
        Assert.Throws<InvalidOperationException>(() =>
            container.GetService<NoSatisfiableCtor>());
    }
}
