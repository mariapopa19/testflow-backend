using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFlow.Application.Models.Requests
{
    public class RunValidationRequest
    {
        public Dictionary<string, string>? Headers { get; set; }
    }
}
