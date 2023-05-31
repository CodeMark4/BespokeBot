using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BespokeBot.Serialization
{
    public struct Fact
    {
        [JsonPropertyName("fact")]
        public string Text { get; set; }
    }
}
