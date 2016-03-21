using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoFlare.Models
{
    public class PurgeStaticFilesRequestModel
    {
        public string[] StaticFiles { get; set; }
        public IEnumerable<string> Hosts { get; set; }
    }
}
