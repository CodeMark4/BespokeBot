using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BespokeBot
{
    public class BespokeData
    {
        public string NinjasKey { get; set; }

        public DbHelper DbHelper { get; set; }

        public string GetEmoteForCloudPct(int cloudPct)
        {
            if (cloudPct <= 10)
            {
                return ":sunny:";
            }

            if (cloudPct <= 40)
            {
                return ":white_sun_small_cloud:";
            }

            if (cloudPct <= 60)
            {
                return ":partly_sunny:";
            }

            return ":white_sun_cloud:";
        }
    }
}
