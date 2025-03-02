namespace ASA.Core.Models
{
    // Endpoint specification
    public class EndpointSpec
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public string Description { get; set; }
        public List<StepSpec> Steps { get; set; } = new List<StepSpec>();
    }
}