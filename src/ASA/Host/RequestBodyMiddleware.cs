using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text;

namespace ASA.Host
{
    /// <summary>
    /// Middleware to parse and store the request body for later use in ASA steps
    /// </summary>
    public class RequestBodyMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestBodyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only process if content type is JSON and there is content
            if (context.Request.ContentType != null && 
                context.Request.ContentType.Contains("application/json") && 
                context.Request.ContentLength > 0)
            {
                // Enable buffering so we can read the body multiple times
                context.Request.EnableBuffering();

                // Read the body
                var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
                await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                
                // Reset the position to allow the request body to be read again later
                context.Request.Body.Position = 0;

                // Parse JSON body
                var bodyText = Encoding.UTF8.GetString(buffer);
                
                try
                {
                    // Try to parse as JSON
                    var bodyJson = JsonSerializer.Deserialize<object>(bodyText);
                    
                    // Store the parsed body in the HttpContext.Items collection
                    context.Items["ASA:RequestBody"] = bodyJson;
                }
                catch (Exception)
                {
                    // If parsing fails, store as raw text
                    context.Items["ASA:RequestBody"] = bodyText;
                }
            }

            // Call the next middleware
            await _next(context);
        }
    }
}
