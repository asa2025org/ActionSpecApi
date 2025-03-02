using ASA.Core;
using ASA.Core.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ASA.Modules
{
    public class ResponseFormatterModule : IModule
    {
        public string Name => "asa.modules/response-formatter";
        public string Version => "1.0.0";

        public async Task<StepOutput> ExecuteAsync(
            Dictionary<string, object> parameters, 
            AsaExecutionContext context)
        {
            // Set defaults
            var statusCode = parameters.TryGetValue("statusCode", out var status) ? 
                Convert.ToInt32(status) : 200;
            
            var contentType = parameters.TryGetValue("contentType", out var ct) ? 
                ct.ToString() : "application/json";

            var body = parameters.GetValueOrDefault("body");

            // Set response properties
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = contentType;

            // Set headers
            if (parameters.TryGetValue("headers", out var headersObj) && 
                headersObj is Dictionary<string, object> headers)
            {
                foreach (var header in headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToString();
                }
            }

            // Format the response body
            string formattedBody;
            if (contentType == "application/json")
            {
                formattedBody = body != null 
                    ? (body is string ? (string) body : JsonSerializer.Serialize(body)) 
                    : "{}";
            }
            else
            {
                formattedBody = body?.ToString() ?? string.Empty;
            }

            await context.Response.WriteAsync(formattedBody);

            return new StepOutput
            {
                Success = true,
                Data = new { status = context.Response.StatusCode, body = formattedBody }
            };
        }
    }
}
