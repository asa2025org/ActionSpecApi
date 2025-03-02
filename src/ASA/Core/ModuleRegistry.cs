using ASA.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASA.Core
{
    public interface IModuleRegistry
    {
        IModule GetModule(string reference);
        void RegisterModule(IModule module);
    }
    
    public class ModuleRegistry : IModuleRegistry
    {
        private readonly Dictionary<string, IModule> _modules = new Dictionary<string, IModule>();
        private readonly ILogger<ModuleRegistry> _logger;

        public ModuleRegistry(
            IEnumerable<IModule> modules,
            ILogger<ModuleRegistry> logger)
        {            
            _logger = logger;

            foreach (var module in modules)
                RegisterModule(module);            
        }

        public IModule GetModule(string reference)
        {
            // Parse the reference: "namespace/module-name@version"
            var parts = reference.Split('@');
            var moduleId = parts[0];
            var version = parts.Length > 1 ? parts[1] : "latest";

            var key = $"{moduleId}@{version}";
            
            if (_modules.TryGetValue(key, out var module))
            {
                return module;
            }
            
            // If requesting specific version failed, try latest
            if (version != "latest")
            {
                var latestKey = $"{moduleId}@latest";
                if (_modules.TryGetValue(latestKey, out var latestModule))
                {
                    _logger.LogWarning("Module {ModuleId} version {Version} not found, using latest", 
                        moduleId, version);
                    return latestModule;
                }
            }
            
            // Try to find by prefix (namespace/module-name)
            var matchingKeys = _modules.Keys
                .Where(k => k.StartsWith(moduleId))
                .ToList();
                
            if (matchingKeys.Count == 1)
            {
                _logger.LogInformation("Using module {FoundModule} for reference {Reference}", 
                    matchingKeys[0], reference);
                return _modules[matchingKeys[0]];
            }
            else if (matchingKeys.Count > 1)
            {
                _logger.LogWarning("Multiple modules match reference {Reference}: {MatchingModules}",
                    reference, string.Join(", ", matchingKeys));
            }

            throw new KeyNotFoundException($"Module not found: {reference}");
        }

        public void RegisterModule(IModule module)
        {
            var key = $"{module.Name}@{module.Version}";
            
            if (_modules.ContainsKey(key))
            {
                _logger.LogWarning("Module {ModuleKey} already registered, overwriting", key);
            }
            
            _modules[key] = module;
            
            // Also register as latest
            var latestKey = $"{module.Name}@latest";
            _modules[latestKey] = module;
            
            _logger.LogInformation("Registered module {ModuleName} version {Version}", 
                module.Name, module.Version);
        }
    }
}
