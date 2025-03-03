using ASA.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ASA.Core
{
    public interface IContextProvider
    {
        object ResolveExpression(string expression, AsaExecutionContext context);
    }

    public class ContextProvider : IContextProvider
    {
        private readonly ILogger<ContextProvider> _logger;
        private static readonly Regex ExpressionPattern = new Regex(@"\$\{\{\s*([^}]+)\s*\}\}", RegexOptions.Compiled);
        private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        public ContextProvider(ILogger<ContextProvider> logger)
        {
            _logger = logger;
        }

        public object ResolveExpression(string expression, AsaExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return null;
            }

            var cacheKey = $"{expression}-{context.GetHashCode()}"; // Create a unique cache key
            if (_cache.TryGetValue(cacheKey, out var cachedResult))
            {
                return cachedResult;
            }

            object result;
            // Check if the expression is a template with embedded expressions
            if (expression.Contains("${{"))
            {
                result = ResolveTemplate(expression, context);
            }
            else
            {
                // Simple path resolution
                var parts = expression.Split('.');
                if (parts.Length < 2)
                {
                    _logger.LogWarning("Invalid expression: {Expression}", expression);
                    return null;
                }

                var rootName = parts[0];
                var path = string.Join(".", parts.Skip(1));

                result = rootName switch
                {
                    "steps" => ResolveStepPath(path, context),
                    "request" => ResolveRequestPath(path, context),
                    "config" => ResolveConfigPath(path, context),
                    "env" => ResolveEnvironmentPath(path),
                    _ => null
                };
            }
            _cache.TryAdd(cacheKey, result);
            return result;
        }

        private string ResolveTemplate(string template, AsaExecutionContext context)
        {
            return ExpressionPattern.Replace(template, match =>
            {
                var innerExpression = match.Groups[1].Value.Trim();
                var result = ResolveExpression(innerExpression, context);
                return result is string stringResult ? stringResult : result != null ? JsonSerializer.Serialize(result) : "{}";
            });
        }

        private object ResolveStepPath(string path, AsaExecutionContext context)
        {
            var parts = path.Split('.');
            if (parts.Length < 2)
            {
                return null;
            }

            var stepName = parts[0];
            var outputPath = parts[1];

            if (!context.Steps.TryGetValue(stepName, out var stepOutput))
            {
                _logger.LogWarning("Step not found: {StepName}", stepName);
                return null;
            }

            if (outputPath == "data")
            {
                return stepOutput.Data;
            }

            // Navigate deeper into the output data
            if (parts.Length > 2 && stepOutput.Data != null)
            {
                var remainingPath = string.Join(".", parts.Skip(2));
                return NavigateObjectPath(stepOutput.Data, remainingPath);
            }

            return null;
        }

        private object ResolveRequestPath(string path, AsaExecutionContext context)
        {
            var parts = path.Split('.');
            var requestPart = parts[0];

            switch (requestPart)
            {
                case "path":
                    if (parts.Length > 1 && context.RouteValues.TryGetValue(parts[1], out var routeValue))
                    {
                        return routeValue;
                    }
                    return null;

                case "query":
                    if (parts.Length > 1 && context.QueryParameters.TryGetValue(parts[1], out var queryValue))
                    {
                        return queryValue.ToString();
                    }
                    return null;

                case "body":
                    if (context.Request.Body != null)
                    {
                        // Note: This assumes the body has been read elsewhere and stored as a property
                        // You'd need to enhance this with body parsing logic
                        // For now we're returning null as a placeholder
                        return null;
                    }
                    return null;

                case "headers":
                    if (parts.Length > 1 && context.Request.Headers.TryGetValue(parts[1], out var headerValue))
                    {
                        return headerValue.ToString();
                    }
                    return null;

                default:
                    return null;
            }
        }

        private object ResolveConfigPath(string path, AsaExecutionContext context)
        {
            // This would typically be implemented to access configuration values
            // For now, returning null as a placeholder
            _logger.LogWarning("Config resolution not yet implemented for path: {Path}", path);
            return null;
        }

        private object ResolveEnvironmentPath(string path)
        {
            return Environment.GetEnvironmentVariable(path);
        }

        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, System.Reflection.PropertyInfo>> _propertyCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, System.Reflection.PropertyInfo>>();

        private object NavigateObjectPath(object obj, string path)
        {
            if (obj == null || string.IsNullOrEmpty(path))
            {
                return obj;
            }

            var parts = path.Split('.');
            var current = obj;

            foreach (var part in parts)
            {
                if (current == null)
                {
                    return null;
                }

                if (current is JsonObject jsonObject)
                {
                    if (jsonObject.TryGetPropertyValue(part, out var node))
                    {
                        current = node;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (current is IDictionary<string, object> dict)
                {
                    if (dict.TryGetValue(part, out var value))
                    {
                        current = value;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    // Try reflection for regular objects
                    var type = current.GetType();
                    var property = _propertyCache.GetOrAdd(type, t => new ConcurrentDictionary<string, System.Reflection.PropertyInfo>())
                        .GetOrAdd(part, p => type.GetProperty(p));

                    if (property != null)
                    {
                        current = property.GetValue(current);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return current;
        }

        public string ResolveExpression(string expression, IDictionary<string, object> context)
        {
            if (string.IsNullOrEmpty(expression) || !expression.Contains("{"))
                return expression;

            return Regex.Replace(expression, @"\{([^}]+)\}", match =>
            {
                var key = match.Groups[1].Value.Trim();
                if (context.TryGetValue(key, out var value))
                {
                    return value?.ToString() ?? string.Empty;
                }
                return match.Value;
            });
        }
    }
}