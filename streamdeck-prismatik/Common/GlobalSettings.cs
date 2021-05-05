using BarRaider.SdTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sibalzer.streamdeck.prismatik.Common
{
    class GlobalSettings
    {
        [JsonProperty(PropertyName = "apiKey")]
        public string ApiKey { get; set; }
    }
}
