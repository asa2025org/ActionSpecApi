using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ASA.Core.Models
{
    // Execution context
    public class AsaExecutionContext
    {
        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
        public IQueryCollection QueryParameters { get; set; }
        public Dictionary<string, StepOutput> Steps { get; set; }
    }
}