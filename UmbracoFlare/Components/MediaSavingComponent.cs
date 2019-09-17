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
using UmbracoFlare.Extensions;
using UmbracoFlare.Helpers;
using UmbracoFlare.ImageCropperHelpers;
using UmbracoFlare.Manager;
using UmbracoFlare.Models;

namespace UmbracoFlare.Components
{
    public class MediaSavingComponent : IComponent
    {
        private readonly ICloudflareManager cloudflareManager;
        private readonly IUmbracoFlareDomainManager domainManager;
        private readonly IImageCropperManager imageCropperManager;
        private readonly UmbracoHelper helper;

        public MediaSavingComponent(
                ICloudflareManager cloudflareManager, 
                IImageCropperManager imageCropperManager,
                UmbracoHelper helper
            )
        {
            this.cloudflareManager = cloudflareManager;
            this.domainManager = cloudflareManager.DomainManager;
            this.imageCropperManager = imageCropperManager;
            this.helper = helper;
        }
        public void Initialize()
        {
            MediaService.Saved += PurgeCloudflareCacheForMedia;
        }
        public void Terminate()
        {
            MediaService.Saved -= PurgeCloudflareCacheForMedia;
        }
        private void PurgeCloudflareCacheForMedia(IMediaService sender, SaveEventArgs<IMedia> e)
        {
            //If we have the cache buster turned off then just return.
            if (!cloudflareManager.Configuration.PurgeCacheOn) { return; }

            var imageCropSizes = imageCropperManager.GetAllCrops();
            List<string> urls = new List<string>();

            //GetUmbracoDomains
            IEnumerable<string> domains = domainManager.AllowedDomains;

            //delete the cloudflare cache for the saved entities.
            foreach (IMedia media in e.SavedEntities)
            {
                if (!media.IsNew())
                {
                    try
                    {
                        //Check to see if the page has cache purging on publish disabled.
                        if (media.GetValue<bool>("cloudflareDisabledOnPublish"))
                        {
                            //it was disabled so just continue;
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        //continue;
                    }

                    var publishedMedia = helper.Media(media.Id);

                    if (publishedMedia == null)
                    {
                        e.Messages.Add(new EventMessage("Cloudflare Caching", "We could not find the IPublishedContent version of the media: " + media.Id + " you are trying to save.", EventMessageType.Error));
                        continue;
                    }
                    foreach (var crop in imageCropSizes)
                    {
                        urls.Add(publishedMedia.GetCropUrl(crop.alias));

                    }
                    urls.Add(publishedMedia.Url);
                    }
            }

            IEnumerable<StatusWithMessage> results = cloudflareManager.PurgePages(UrlHelper.MakeFullUrlWithDomain(urls, domains, true));

            if (results.Any() && results.Where(x => !x.Success).Any())
            {
                e.Messages.Add(new EventMessage("Cloudflare Caching", "We could not purge the Cloudflare cache. \n \n" + cloudflareManager.PrintResultsSummary(results), EventMessageType.Warning));
            }
            else if (results.Any())
            {
                e.Messages.Add(new EventMessage("Cloudflare Caching", "Successfully purged the cloudflare cache.", EventMessageType.Success));
            }
        }

    }
}
