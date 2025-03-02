using ASA.Core;
using ASA.Core.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace ASA.Host
{
    public static class DynamicEndpointRegistrationExtensions
    {
        public static IEndpointRouteBuilder MapActionSpecEndpoints(
            this IEndpointRouteBuilder endpoints,
            ActionSpec spec,
            IStepExecutor stepExecutor,
            ILogger<ActionSpecMiddleware> logger)
        {
            foreach (var endpoint in spec.Endpoints)
            {
                // Register each endpoint from the spec
                var routePattern = endpoint.Path;
                var httpMethod = endpoint.Method.ToUpper();
                
                logger.LogInformation("Registering endpoint: {Method} {Path}", httpMethod, routePattern);
                
                endpoints.MapMethods(
                    routePattern,
                    new[] { httpMethod },
                    async context =>
                    {
                        try
                        {
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
                            await stepExecutor.ExecuteAsync(endpoint.Steps, executionContext);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error executing endpoint {Method} {Path}", 
                                httpMethod, routePattern);
                            
                            // Set error response if not already set
                            if (!context.Response.HasStarted)
                            {
                                context.Response.StatusCode = 500;
                                context.Response.ContentType = "application/json";
                                await context.Response.WriteAsync($"{{\"error\":\"Internal server error: {ex.Message}\"}}");
                            }
                        }
                    })
                    .WithDisplayName(endpoint.Description ?? $"{httpMethod} {routePattern}");
            }

            return endpoints;
        }
    }    
}
