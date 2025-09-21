using Syrette;

namespace DISandbox; 

interface IService {
    public void Log(string message);
}

class Service : IService {
    public void Log(string message) {
        Console.WriteLine($"[Service] {message}");
    }
}

interface IOtherService {
    public Guid Id { get; }
    
    public void ShowId() => Console.WriteLine($"[OtherService] ID: {Id}");
}

class GuidService : IOtherService {
    public Guid Id { get; } = Guid.NewGuid();
}
public interface INotRegisteredService {
    void DoSomething();
}

class GuidDependantService {
    private readonly IService logService;
    private readonly IOtherService? guidService;

    public GuidDependantService(IService logService, INotRegisteredService guidService) {
        this.logService = logService;
    }

    public GuidDependantService(IService logService, IOtherService guidService) {
        this.logService = logService;
        this.guidService = guidService;
    }

    public void LogWithId(string message) {
        logService.Log($"[GuidDependantService] {message} (ID: {guidService.Id})");
    }
}

static class Program {
    static void Main(string[] args) {
        var container = new ServiceContainer()
            .AddSingleton<IService, Service>()
            .AddTransient<IOtherService, GuidService>()
            .AddTransient<GuidDependantService, GuidDependantService>();

        var service = container.GetService<IService>();
        service.Log("Hello, Dependency Injection!");
        container.GetService<IOtherService>().ShowId();
        container.GetService<GuidDependantService>().LogWithId("Hello, sent from the dependency.");
        container.GetService<IService>().Log("Goodbye, Dependency Injection!");
    }
}