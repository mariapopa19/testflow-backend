using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestFlow.Application.Utils
{
    public static class ValidationTestUtils
    {
        public static object? GuessExampleValue(JsonValueKind kind)
        {
            return kind switch
            {
                JsonValueKind.String => "example",
                JsonValueKind.Number => 123,
                JsonValueKind.True => false,
                JsonValueKind.False => true,
                JsonValueKind.Array => new object[] { },
                JsonValueKind.Object => new Dictionary<string, object>(),
                _ => null
            };

        }
    }
}
