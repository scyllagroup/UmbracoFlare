using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmbracoFlare.Models;

namespace UmbracoFlare.Services
{
    public interface ICloudflareService
    {
        UserDetails GetUserDetails();
        SslEnabledResponse GetSSLStatus(string zoneId);
        IEnumerable<Zone> ListZones(string name = null, bool throwExceptionOnFail = false);
        bool PurgeCache(string zoneIdentifier, IEnumerable<string> urls, bool purgeEverything = false, bool throwExceptionOnError = false);
    }
}
