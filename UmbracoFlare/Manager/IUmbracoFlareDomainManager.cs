using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmbracoFlare.Models;

namespace UmbracoFlare.Manager
{
    public interface IUmbracoFlareDomainManager
    {
        IEnumerable<Zone> AllowedZones { get; }
        IEnumerable<string> AllowedDomains { get; }
        IEnumerable<string> FilterToAllowedDomains(IEnumerable<string> domains);
        List<string> GetUrlsForNode(int contentId, bool includeDescendants = false);
        IEnumerable<string> GetDomainsFromCloudflareZones();
    }
}
