using ASA.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ASA.Core
{
    public interface IStepExecutor
    {
        Task ExecuteAsync(List<StepSpec> steps, AsaExecutionContext context);
    }
    
    public class StepExecutor : IStepExecutor
    {
        private readonly IModuleRegistry _moduleRegistry;
        private readonly IContextProvider _contextProvider;
        private readonly ILogger<StepExecutor> _logger;

        public StepExecutor(
            IModuleRegistry moduleRegistry,
            IContextProvider contextProvider,
            ILogger<StepExecutor> logger)
        {
            _moduleRegistry = moduleRegistry;
            _contextProvider = contextProvider;
            _logger = logger;
        }

        public async Task ExecuteAsync(List<StepSpec> steps, AsaExecutionContext context)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                
                // Check if step should be skipped (conditional)
                if (!string.IsNullOrEmpty(step.If))
                {
                    var condition = _contextProvider.ResolveExpression(step.If, context);
                    
                    bool shouldExecute = false;
                    if (condition is bool boolCondition)
                    {
                        shouldExecute = boolCondition;
                    }
                    else if (condition is string strCondition)
                    {
                        shouldExecute = !string.IsNullOrEmpty(strCondition) && 
                                       !strCondition.Equals("false", StringComparison.OrdinalIgnoreCase) &&
                                       !strCondition.Equals("0");
                    }
                    else
                    {
                        shouldExecute = condition != null;
                    }
                    
                    if (!shouldExecute)
                    {
                        _logger.LogInformation("Skipping step {StepName} due to condition: {Condition}", step.Name, step.If);
                        continue;
                    }
                }
                
                string? error = null;
                try
                {
                    _logger.LogInformation("Executing step {StepIndex}: {StepName}", i + 1, step.Name);
                    
                    // Get module
                    var module = _moduleRegistry.GetModule(step.Uses);
                    
                    // Resolve parameter values
                    var resolvedParams = ResolveParameters(step.With, context);
                    
                    // Execute module
                    var output = await module.ExecuteAsync(resolvedParams, context);
                    
                    if (output.Success)
                    {
                        // Store output
                        context.Steps[step.Name] = output;
                        _logger.LogInformation("Step {StepName} succeeded: {Data}", step.Name, output.Data);
                    }
                    else
                    {
                        error = output.Error;
                    }
                }
                catch (Exception ex)
                {
                    error = ex.ToString();
                }
                
                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogError("Step {StepName} failed: {Error}", step.Name, error);
                    
                    // Set error response
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync($"{{\"error\": \"Step execution failed: {error}\"}}");
                    return;
                }
            }
        }
        
        private Dictionary<string, object> ResolveParameters(
            Dictionary<string, object> parameters, 
            AsaExecutionContext context)
        {
            var result = new Dictionary<string, object>();

            //if (paramValue is string stringValue)
            //{
            //    // Process string parameters for variable substitution
            //    var pattern = @"\$\{\{\s*(.*?)\s*\}\}";
            //    return Regex.Replace(stringValue, pattern, match =>
            //    {
            //        var expression = match.Groups[1].Value;
            //        var resolved = _contextProvider.ResolveExpression(expression, context);
            //        return resolved?.ToString() ?? string.Empty;
            //    });
            //}
            //else if (paramValue is Dictionary<string, object> dict)
            //{
            //    // Process nested dictionaries
            //    var result = new Dictionary<string, object>();
            //    foreach (var item in dict)
            //    {
            //        result[item.Key] = ResolveParameter(item.Value, context);
            //    }
            //    return result;
            //}
            //else if (paramValue is List<object> list)
            //{
            //    // Process lists
            //    var result = new List<object>();
            //    foreach (var item in list)
            //    {
            //        result.Add(ResolveParameter(item, context));
            //    }
            //    return result;
            //}
            
            foreach (var param in parameters)
            {
                if (param.Value is string strValue && strValue.Contains("${{"))
                {
                    // This is an expression to resolve
                    var resolved = _contextProvider.ResolveExpression(strValue, context);
                    result[param.Key] = resolved;
                }
                else
                {
                    // Pass through as-is
                    result[param.Key] = param.Value;
                }
            }
            
            return result;
        }
    }
}
