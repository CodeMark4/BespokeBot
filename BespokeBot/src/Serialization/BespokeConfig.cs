using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BespokeBot.Serialization
{
    public struct BespokeConfig
    {
        private static string TOKEN = "MTExMzE1ODk3MjQzMDYyMjc0MA.GcaY_1.KanxL0G22o15b55Viuy85bJwna99-bTQNYm5jc";

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("prefix")]
        public string? Prefix { get; set; }

        [JsonPropertyName("ninjas_key")]
        public string? NinjasKey { get; set; }
    }
}
