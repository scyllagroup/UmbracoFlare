using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmbracoFlare.Configuration;
using UmbracoFlare.Models;

namespace UmbracoFlare.Manager
{
    public interface ICloudflareManager
    {
        ICloudflareConfiguration Configuration { get; }
        IUmbracoFlareDomainManager DomainManager { get; }
        StatusWithMessage PurgeEverything(string domain);
        IEnumerable<StatusWithMessage> PurgePages(IEnumerable<string> urls);
        Zone GetZone(string url = null);
        bool IsSSLEnabledOnCloudflare(string zoneId);
        IEnumerable<Zone> ListZones();
        string PrintResultsSummary(IEnumerable<StatusWithMessage> results);
    }
}
