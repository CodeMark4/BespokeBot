using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BespokeBot.Serialization
{
    public struct Quote
    {
        [JsonPropertyName("quote")]
        public string Text { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }
    }
}
