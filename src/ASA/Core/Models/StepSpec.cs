namespace ASA.Core.Models
{
    // Step specification
    public class StepSpec
    {
        public string Name { get; set; }
        public string Uses { get; set; }
        public string If { get; set; }
        public Dictionary<string, object> With { get; set; } = new Dictionary<string, object>();
    }
}