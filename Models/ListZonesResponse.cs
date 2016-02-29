using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UmbracoFlare.Models
{

    class ListZonesResponse : BasicCloudflareResponse
    {
        [JsonProperty(PropertyName="result")]
        public List<Zone> Zones { get; set; }
    }
}
