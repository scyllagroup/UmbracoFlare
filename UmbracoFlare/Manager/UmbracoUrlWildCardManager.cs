using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace UmbracoFlare.Manager
{
    public class UmbracoUrlWildCardManager
    {
        private Dictionary<int, string> _contentIdToUrlCache;
        private const string CACHE_KEY = "UmbracoUrlWildCardManager.ContentIdToUrlCache";

        private static UmbracoUrlWildCardManager _instance;

        private UmbracoUrlWildCardManager()
        {
            _contentIdToUrlCache = HttpRuntime.Cache[CACHE_KEY] as Dictionary<int, string>;
        }

        public static UmbracoUrlWildCardManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UmbracoUrlWildCardManager();
                }

                return _instance;
            }
        }


        public IEnumerable<string> GetAllUrlsForWildCardUrls(IEnumerable<string> wildCardUrls, UmbracoHelper uh)
        {
            List<string> resolvedUrls = new List<string>();

            if (wildCardUrls == null || !wildCardUrls.Any())
            {
                return resolvedUrls;
            }

            IEnumerable<string> allContentUrls = GetAllContentUrls(uh);

            foreach(string wildCardUrl in wildCardUrls)
            {
                if(!wildCardUrl.Contains('*'))
                {
                    //it doesnt even contain a wildcard.
                    continue;
                }

                //Make one for modifing
                string mutableWildCardUrl = wildCardUrl;

                //take off the trailing slash if there is one
                mutableWildCardUrl = mutableWildCardUrl.TrimEnd('/');

                //take off the *
                mutableWildCardUrl = mutableWildCardUrl.TrimEnd('*');

                //We can get wild cards by seeing if any of the urls start with the mutable wild card url
                resolvedUrls.AddRange(allContentUrls.Where(x => x.StartsWith(mutableWildCardUrl)));
            }

            return resolvedUrls;
        }


        /// <summary>
        /// Gets the urls of every content item in the content section and caches the results. Is there a faster way to do this? 
        /// </summary>
        /// <param name="uh"></param>
        /// <returns></returns>
        private IEnumerable<string> GetAllContentUrls(UmbracoHelper uh)
        {
            if(this._contentIdToUrlCache != null && this._contentIdToUrlCache.Any())
            {
                //just return the cache
                return this._contentIdToUrlCache.Select(x => x.Value);
            }

            Dictionary<int, string> cache = new Dictionary<int, string>();
            List<string> urls = new List<string>();

            //Id like to use UmbracoContext.Current.ContentCache.GetByRoute() somehow but you cant always guarantee that urls
            //will be in  hierarchical order because of rewriteing, etc.

            IEnumerable<IPublishedContent> roots = uh.TypedContentAtRoot();
            
            foreach(IPublishedContent content in roots)
            {
                cache.Add(content.Id, content.Url);
                urls.Add(content.Url);
                foreach(IPublishedContent childContent in content.Descendants())
                {
                    cache.Add(childContent.Id, childContent.Url);
                    urls.Add(childContent.Url);
                }
            }

            this._contentIdToUrlCache = cache;
            //Add to the cache
            HttpRuntime.Cache.Insert(CACHE_KEY, cache, null, DateTime.Now.AddDays(1), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);

            return urls;
        }

        public void UpdateContentIdToUrlCache(int id, string url)
        {
            if(this._contentIdToUrlCache == null || !this._contentIdToUrlCache.Any())
            {
                //No cache yet.
                return;
            }
            
            if(this._contentIdToUrlCache.ContainsKey(id))
            {
                this._contentIdToUrlCache[id] = url;
            }
            else
            {
                this._contentIdToUrlCache.Add(id, url);
            }

        }


    }
}
