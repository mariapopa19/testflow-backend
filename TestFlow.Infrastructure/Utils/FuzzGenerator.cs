    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TestFlow.Infrastructure.Utils
{
    public static class FuzzGenerator
    {
        public static List<JObject> Generate(string jsonSchema)
        {
            // Pentru acum: returnăm doar câteva inputuri invalide hardcodate
            return new List<JObject>
            {
                new JObject { ["field1"] = 1234567890123456789 }, // valoare prea mare
                new JObject { ["field1"] = true },                 // tip greșit
                new JObject { ["field2"] = JValue.CreateNull() }, // null nepermis
                new JObject()                                     // complet gol
            };
        }
    }
}
