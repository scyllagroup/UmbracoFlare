using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoFlare.Models
{
    public class PurgeCacheForUrlsRequestModel
    {
        public IEnumerable<string> Urls { get; set; }
        public IEnumerable<string> Domains { get; set; }
    }
}
