using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFlow.Application.Models.GoogleLogin
{
    public class GoogleUserInfo
    {
        public string? Sub { get; set; } = null;
        public string? Name { get; set; } = null;
        public string? Given_Name { get; set; } = null;
        public string? Family_Name { get; set; } = null;
        public string? Picture { get; set; } = null;
        public string? Email { get; set; } = null;
        public bool Email_Verified { get; set; }
        public string? Locale { get; set; } = null;
    }
}
