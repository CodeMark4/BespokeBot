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
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("prefix")]
        public string? Prefix { get; set; }

        [JsonPropertyName("ninjas_key")]
        public string? NinjasKey { get; set; }

        [JsonPropertyName("db_connection")]
        public string? DBConnection { get; set; }
    }
}
