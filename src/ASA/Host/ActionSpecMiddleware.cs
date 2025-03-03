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
        private readonly Dictionary<string, EndpointSpec> _endpointCache = new Dictionary<string, EndpointSpec>();

        public ActionSpecMiddleware(RequestDelegate next, ActionSpec spec, IStepExecutor executor)
        {
            _next = next;
            _spec = spec;
            _executor = executor;

            // Build endpoint cache
            foreach (var endpoint in _spec.Endpoints)
            {
                var key = $"{endpoint.Path}-{endpoint.Method}";
                _endpointCache[key] = endpoint;
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Find matching endpoint
            var key = $"{context.Request.Path.Value}-{context.Request.Method}";
            if (!_endpointCache.TryGetValue(key, out var endpoint))
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
    }
}
