using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BespokeBot.Serialization
{
    public class TriviaQuestion
    {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("question")]
        public string Question { get; set; }

        [JsonPropertyName("answer")]
        public string Answer { get; set; }
    }
}
