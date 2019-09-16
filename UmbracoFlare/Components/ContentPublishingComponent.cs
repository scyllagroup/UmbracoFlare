using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web;
using UmbracoFlare.Configuration;
using UmbracoFlare.Manager;
using UmbracoFlare.Models;

namespace UmbracoFlare.Components
{
    public class ContentPublishingComponent : IComponent
    {
        private readonly UmbracoHelper helper;
        private readonly ICloudflareManager cloudflareManager;
        private readonly IUmbracoFlareDomainManager domainManager;
        private readonly IUrlWildCardManager wildCardManager;

        public ContentPublishingComponent(
            UmbracoHelper helper, 
            ICloudflareManager cloudflareManager, 
            IUrlWildCardManager wildCardManager)
        {
            this.helper = helper;
            this.cloudflareManager = cloudflareManager;
            this.domainManager = cloudflareManager.DomainManager;
            this.wildCardManager = wildCardManager;
        }
        public void Initialize()
        {
            ContentService.Published += PurgeCloudflareCache;
            ContentService.Published += UpdateContentIdToUrlCache;
        }


        public void Terminate()
        {
            ContentService.Published -= PurgeCloudflareCache;
            ContentService.Published -= UpdateContentIdToUrlCache;
        }

        private void PurgeCloudflareCache(IContentService sender, ContentPublishedEventArgs e)
        {
            //If we have the cache buster turned off then just return.
            if (!cloudflareManager.Configuration.PurgeCacheOn) { return; }

            List<string> urls = new List<string>();
            //Else we can continue to delete the cache for the saved entities.
            foreach (IContent content in e.PublishedEntities)
            {
                try
                {
                    //Check to see if the page has cache purging on publish disabled.
                    if (content.GetValue<bool>("cloudflareDisabledOnPublish"))
                    {
                        //it was disabled so just continue;
                        continue;
                    }
                }
                catch (Exception)
                {
                    //ignore
                }

                urls.AddRange(domainManager.GetUrlsForNode(content.Id, false));
            }

            IEnumerable<StatusWithMessage> results = cloudflareManager.PurgePages(urls);

            if (results.Any() && results.Where(x => !x.Success).Any())
            {
                e.Messages.Add(new EventMessage("Cloudflare Caching", "We could not purge the Cloudflare cache. \n \n" + cloudflareManager.PrintResultsSummary(results), EventMessageType.Warning));
            }
            else if (results.Any())
            {
                e.Messages.Add(new EventMessage("Cloudflare Caching", "Successfully purged the cloudflare cache.", EventMessageType.Success));
            }
        }

        private void UpdateContentIdToUrlCache(IContentService sender, ContentPublishedEventArgs e)
        {

            foreach (IContent c in e.PublishedEntities)
            {
                if (c.Published)
                {
                    IEnumerable<string> urls = domainManager.GetUrlsForNode(c.Id, false);

                    if (urls.Contains("#"))
                    {
                        //When a piece of content is first saved, we cannot get the url, if that is the case then we need to just
                        //invalidate the who ContentIdToUrlCache, that way when we request all of the urls agian, it will pick it up.
                        wildCardManager.DeletedContentIdToUrlCache();
                    }
                    else
                    {
                        wildCardManager.UpdateContentIdToUrlCache(c.Id, urls);
                    }

                }


                //TODO: Does this need to be here?
                //We also need to update the descendants now because their urls changed
                int descendantsCount = sender.CountDescendants(c.Id);
                long outParam = 0;
                IEnumerable<IContent> descendants = sender.GetPagedDescendants(c.Id, 0, descendantsCount, out outParam);

                foreach (IContent desc in descendants)
                {
                    IEnumerable<string> descUrls = domainManager.GetUrlsForNode(desc.Id, false);

                    wildCardManager.UpdateContentIdToUrlCache(c.Id, descUrls);
                }
            }
        }

    }
}
