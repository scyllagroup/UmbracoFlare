using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoFlare.Manager
{
    public interface IUrlWildCardManager
    {
        void DeletedContentIdToUrlCache();
        void UpdateContentIdToUrlCache(int id, IEnumerable<string> urls);
        IEnumerable<string> GetAllUrlsForWildCardUrls(IEnumerable<string> urlsWithWildCards);
    }
}
