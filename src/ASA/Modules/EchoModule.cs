using ASA.Core;
using ASA.Core.Models;

namespace ASA.Modules
{
    // Echo Module - Simply returns a value
    public class EchoModule : IModule
    {
        private const string DefaultMessage = "Hello, World!";

        public string Name => "asa.modules/echo";
        public string Version => "1.0.0";
        
        public Task<StepOutput> ExecuteAsync(
            Dictionary<string, object> parameters, 
            AsaExecutionContext context)
        {
            parameters ??= new Dictionary<string, object>();
            
            object message = parameters.GetValueOrDefault("message");
            string output = message?.ToString() ?? DefaultMessage;
                
            return Task.FromResult(new StepOutput
            {
                Success = true,
                Data = output
            });
        }
    }
}
