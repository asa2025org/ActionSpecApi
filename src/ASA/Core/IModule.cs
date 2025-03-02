using ASA.Core.Models;

namespace ASA.Core
{
    // Module interfaces
    public interface IModule
    {
        string Name { get; }
        string Version { get; }
        Task<StepOutput> ExecuteAsync(Dictionary<string, object> parameters, AsaExecutionContext context);
    }
}