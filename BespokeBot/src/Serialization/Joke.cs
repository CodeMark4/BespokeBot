using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BespokeBot.Serialization
{
    public struct Joke
    {
        [JsonPropertyName("joke")]
        public string Text { get; set; }
    }
}
