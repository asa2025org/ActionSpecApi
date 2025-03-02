using YamlDotNet.Serialization;

namespace ASA.Core.Models
{
    // Main specification model
    public class ActionSpec
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string OpenApiSpec { get; set; }

        public List<EndpointSpec> Endpoints { get; set; } = new();
    }
}