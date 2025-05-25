using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace TestFlow.Application.Models.Requests
{
    public class RunTestsRequest
    {
        [Required]
        public Guid EndpointId { get; set; } 
        [Required]
        public bool ArtificialIntelligence { get; set; } = false;
    }
}
