using ASA.Core;
using ASA.Core.Models;
using Microsoft.AspNetCore.Http;

namespace ASA.Host
{
    public class ActionSpecMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ActionSpec _spec;
        private readonly IStepExecutor _executor;

        public ActionSpecMiddleware(RequestDelegate next, ActionSpec spec, IStepExecutor executor)
        {
            _next = next;
            _spec = spec;
            _executor = executor;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Find matching endpoint
            var endpoint = FindMatchingEndpoint(context, _spec);
            if (endpoint == null)
            {
                await _next(context);
                return;
            }

            // Create execution context
            var executionContext = new AsaExecutionContext
            {
                Request = context.Request,
                Response = context.Response,
                RouteValues = context.Request.RouteValues,
                QueryParameters = context.Request.Query,
                Steps = new Dictionary<string, StepOutput>()
            };

            // Execute steps
            await _executor.ExecuteAsync(endpoint.Steps, executionContext);
        }

        private EndpointSpec FindMatchingEndpoint(HttpContext context, ActionSpec spec)
        {
            // Simple matching logic - in real implementation would be more sophisticated
            foreach (var endpoint in spec.Endpoints)
            {
                if (context.Request.Path.Value.Equals(endpoint.Path, StringComparison.OrdinalIgnoreCase) &&
                    context.Request.Method.Equals(endpoint.Method, StringComparison.OrdinalIgnoreCase))
                {
                    return endpoint;
                }
            }
            return null;
        }
    }
}