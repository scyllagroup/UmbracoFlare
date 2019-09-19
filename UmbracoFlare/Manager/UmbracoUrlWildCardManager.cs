using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace UmbracoFlare.Manager
{
    public class RuntimeCacheUrlWildCardManager: IUrlWildCardManager
    {
        private Dictionary<int, IEnumerable<string>> _contentIdToUrlCache;
        private const string CACHE_KEY = "UmbracoUrlWildCardManager.ContentIdToUrlCache";
        private readonly IUmbracoContextFactory umbracoContextFactory;
        private readonly IUmbracoFlareDomainManager domainManager;

        public RuntimeCacheUrlWildCardManager(
                IUmbracoFlareDomainManager domainManager,
                IUmbracoContextFactory umbracoContextFactory
            )
        {
            _contentIdToUrlCache = HttpRuntime.Cache[CACHE_KEY] as Dictionary<int, IEnumerable<string>>;
            this.domainManager = domainManager;
            this.umbracoContextFactory = umbracoContextFactory;
        }


        public IEnumerable<string> GetAllUrlsForWildCardUrls(IEnumerable<string> wildCardUrls)
        {
            List<string> resolvedUrls = new List<string>();

            if (wildCardUrls == null || !wildCardUrls.Any())
            {
                return resolvedUrls;
            }

            IEnumerable<string> allContentUrls = GetAllContentUrls();

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
        /// <returns></returns>
        private IEnumerable<string> GetAllContentUrls()
        {
            if(this._contentIdToUrlCache != null && this._contentIdToUrlCache.Any())
            {
                //just return the cache
                return this._contentIdToUrlCache.SelectMany(x => x.Value);
            }

            Dictionary<int, IEnumerable<string>> cache = new Dictionary<int, IEnumerable<string>>();
            List<string> urls = new List<string>();

            //Id like to use UmbracoContext.Current.ContentCache.GetByRoute() somehow but you cant always guarantee that urls
            //will be in  hierarchical order because of rewriteing, etc.

            IEnumerable<IPublishedContent> roots = Enumerable.Empty<IPublishedContent>();
            using (var contextReference = umbracoContextFactory.EnsureUmbracoContext())
            {
                contextReference.UmbracoContext.Content.GetAtRoot();
            }
            
            foreach(IPublishedContent content in roots)
            {
                IEnumerable<string> contentUrls = domainManager.GetUrlsForNode(content.Id, false);

                cache.Add(content.Id, contentUrls);
                urls.AddRange(contentUrls);

                foreach(IPublishedContent childContent in content.Descendants())
                {
                    IEnumerable<string> childContentUrls = domainManager.GetUrlsForNode(childContent.Id, false);

                    cache.Add(childContent.Id, childContentUrls);
                    urls.AddRange(childContentUrls);
                }
            }

            this._contentIdToUrlCache = cache;
            //Add to the cache
            HttpRuntime.Cache.Insert(CACHE_KEY, cache, null, DateTime.Now.AddDays(1), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);

            return urls;
        }

        public void UpdateContentIdToUrlCache(int id, IEnumerable<string> urls)
        {
            if(this._contentIdToUrlCache == null || !this._contentIdToUrlCache.Any())
            {
                //No cache yet.
                return;
            }
            
            if(this._contentIdToUrlCache.ContainsKey(id))
            {
                this._contentIdToUrlCache[id] = urls;
            }
            else
            {
                this._contentIdToUrlCache.Add(id, urls);
            }

        }


        public void DeletedContentIdToUrlCache()
        {
            HttpRuntime.Cache.Remove(CACHE_KEY);

            this._contentIdToUrlCache = null;

        }

    }
}
