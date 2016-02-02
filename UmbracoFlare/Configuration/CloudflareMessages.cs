using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoFlare.Configuration
{
    public static class CloudflareMessages
    {
        public const string CLOULDFLARE_DISABLED = "We could not purge the cache because your settings indicate that cloudflare purging is off.";
        public const string CLOUDFLARE_API_ERROR = "There was an error from the Cloudflare API. Please check the logfile to see the issue.";
    }
}
