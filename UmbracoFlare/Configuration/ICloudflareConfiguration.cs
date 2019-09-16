using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoFlare.Configuration
{
    public interface ICloudflareConfiguration
    {
        string ApiKey { get; set; }
        string AccountEmail { get; set; }
        bool PurgeCacheOn { get; set; }
    }
}
