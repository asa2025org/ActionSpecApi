using ASA.Core;
using ASA.Core.Models;
using ASA.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ASA.Host
{
    public static class ActionSpecApiExtensions
    {
        public static IServiceCollection AddActionSpecApi(this IServiceCollection services, string specFilePath)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var specYaml = File.ReadAllText(specFilePath);
            var spec = deserializer.Deserialize<ActionSpec>(specYaml);
            services.AddSingleton(spec);

            // Logging
            services.AddLogging();
            
            // Register module providers
            services.AddSingleton<IContextProvider, ContextProvider>();
            services.AddSingleton<IStepExecutor, StepExecutor>();
            services.AddSingleton<IModuleRegistry, ModuleRegistry>();

            // Modules
            services.AddSingleton<IModule, EchoModule>();
            services.AddSingleton<IModule, ResponseFormatterModule>();
            
            return services;
        }

        public static IApplicationBuilder UseActionSpecApi(this WebApplication app)
        {
            app.UseRouting();

            app.UseMiddleware<ActionSpecMiddleware>();

            // First, use the request body parser middleware
            app.UseMiddleware<RequestBodyMiddleware>();
            
            // Then, configure endpoints using the ASA spec
            app.UseEndpoints(endpoints =>
            {
                var spec = app.Services.GetRequiredService<ActionSpec>();
                var stepExecutor = app.Services.GetRequiredService<IStepExecutor>();
                var logger = app.Services.GetRequiredService<ILogger<ActionSpecMiddleware>>();
                
                endpoints.MapActionSpecEndpoints(spec, stepExecutor, logger);
            });
            
            return app;
        }
    }
}