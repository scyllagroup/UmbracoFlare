using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoFlare.Models
{
    public class CloudflareConfigModel
    {
        public bool PurgeCacheOn { get; set; }
        public IEnumerable<string> AdditionalUrls { get; set; }
        public string ApiKey { get; set; }
        public string AccountEmail { get; set; }
        public bool CredentialsAreValid { get; set; }
    }
}
