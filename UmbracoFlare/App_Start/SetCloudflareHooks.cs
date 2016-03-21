using UmbracoFlare.Configuration;
using UmbracoFlare.Manager;
using UmbracoFlare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;
using UmbracoFlare.ImageCropperHelpers;
using UmbracoFlare.Models.CropModels;
using Umbraco.Web;
using Umbraco.Core.Logging;
using UmbracoFlare.Helpers;
using Umbraco.Web.Cache;
using Umbraco.Core.Cache;
using UmbracoFlare.ApiControllers;



namespace UmbracoFlare.App_Start
{
    public class SetCloudflareHooks : ApplicationEventHandler
    {
        public SetCloudflareHooks()
            : base()
        {
            ContentService.Published += PurgeCloudflareCache;
            ContentService.Published += UpdateContentIdToUrlCache;

            MediaService.Saved += PurgeCloudflareCacheForMedia;
            DataTypeService.Saved += RefreshImageCropsCache;
            TreeControllerBase.MenuRendering += AddPurgeCacheForContentMenu;
        }


        protected void UpdateContentIdToUrlCache(IPublishingStrategy strategy, PublishEventArgs<IContent> e)
        {
            UmbracoHelper uh = new UmbracoHelper(UmbracoContext.Current);
            
            foreach(IContent c in e.PublishedEntities)
            {
                if(c.HasPublishedVersion)
                {
                    IEnumerable<string> urls = UmbracoFlareDomainManager.Instance.GetUrlsForNode(c, false);

                    if(urls.Contains("#"))
                    {
                        //When a piece of content is first saved, we cannot get the url, if that is the case then we need to just
                        //invalidate the who ContentIdToUrlCache, that way when we request all of the urls agian, it will pick it up.
                        UmbracoUrlWildCardManager.Instance.DeletedContentIdToUrlCache();
                    }
                    else
                    {
                        UmbracoUrlWildCardManager.Instance.UpdateContentIdToUrlCache(c.Id, urls);
                    }

                }   


                //TODO: Does this need to be here?
                //We also need to update the descendants now because their urls changed
                IEnumerable<IContent> descendants = c.Descendants();

                foreach(IContent desc in descendants)
                {
                    IEnumerable<string> descUrls = UmbracoFlareDomainManager.Instance.GetUrlsForNode(desc.Id, false);

                    UmbracoUrlWildCardManager.Instance.UpdateContentIdToUrlCache(c.Id, descUrls);
                }
            }
        }

        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarting(umbracoApplication, applicationContext);
        }


        protected void RefreshImageCropsCache(IDataTypeService sender, SaveEventArgs<IDataTypeDefinition> e)
        {
            //A data type has saved, see if it was a 
            IEnumerable<IDataTypeDefinition> imageCroppers = ImageCropperManager.Instance.GetImageCropperDataTypes(true);
            IEnumerable<IDataTypeDefinition> freshlySavedImageCropper = imageCroppers.Intersect(e.SavedEntities);

            if(imageCroppers.Intersect(e.SavedEntities).Any())
            {
                //There were some freshly saved Image cropper data types so refresh the image crop cache.
                //We can do that by simply getting the crops
                ImageCropperManager.Instance.GetAllCrops(true); //true to bypass the cache & refresh it.
            }
        }


        protected void PurgeCloudflareCacheForMedia(IMediaService sender, SaveEventArgs<IMedia> e)
        {
            //If we have the cache buster turned off then just return.
            if (!CloudflareConfiguration.Instance.PurgeCacheOn) { return; }

            List<Crop> imageCropSizes = ImageCropperManager.Instance.GetAllCrops();
            List<string> urls = new List<string>();

            UmbracoHelper uh = new UmbracoHelper(UmbracoContext.Current);

            //GetUmbracoDomains
            IEnumerable<string> domains = UmbracoFlareDomainManager.Instance.AllowedDomains;
           
            //delete the cloudflare cache for the saved entities.
            foreach (IMedia media in e.SavedEntities)
            {
                if(media.IsNewEntity())
                {
                    //If its new we don't want to purge the cache as this causes slow upload times.
                    continue;
                }

                try
                {
                    //Check to see if the page has cache purging on publish disabled.
                    if (media.GetValue<bool>("cloudflareDisabledOnPublish"))
                    {
                        //it was disabled so just continue;
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    //continue;
                } 

                IPublishedContent publishedMedia = uh.TypedMedia(media.Id);

                if (publishedMedia == null)
                {
                    e.Messages.Add(new EventMessage("Cloudflare Caching", "We could not find the IPublishedContent version of the media: "+media.Id+" you are trying to save.", EventMessageType.Error));
                    continue;
                }
                foreach(Crop crop in imageCropSizes)
                {
                    urls.Add(publishedMedia.GetCropUrl(crop.alias));    

                }
                urls.Add(publishedMedia.Url);
            }


            List<StatusWithMessage> results = CloudflareManager.Instance.PurgePages(UrlHelper.MakeFullUrlWithDomain(urls, domains,true));

            if (results.Any() && results.Where(x => !x.Success).Any())
            {
                e.Messages.Add(new EventMessage("Cloudflare Caching", "We could not purge the Cloudflare cache. \n \n" + CloudflareManager.PrintResultsSummary(results), EventMessageType.Warning));
            }
            else if(results.Any())
            {
                e.Messages.Add(new EventMessage("Cloudflare Caching", "Successfully purged the cloudflare cache.", EventMessageType.Success));
            }
        }

        

        protected void PurgeCloudflareCache(IPublishingStrategy strategy, PublishEventArgs<IContent> e)
        {
            //If we have the cache buster turned off then just return.
            if (!CloudflareConfiguration.Instance.PurgeCacheOn) { return; }

            List<string> urls = new List<string>();
            //Else we can continue to delete the cache for the saved entities.
            foreach(IContent content in e.PublishedEntities)
            {
                try
                {
                    //Check to see if the page has cache purging on publish disabled.
                    if(content.GetValue<bool>("cloudflareDisabledOnPublish"))
                    {
                        //it was disabled so just continue;
                        continue;
                    }
                }
                catch(Exception ex)
                {
                    //continue;
                }

                urls.AddRange(UmbracoFlareDomainManager.Instance.GetUrlsForNode(content, false));
            }

            List<StatusWithMessage> results = CloudflareManager.Instance.PurgePages(urls);

            if (results.Any() && results.Where(x => !x.Success).Any())
            {
                e.Messages.Add(new EventMessage("Cloudflare Caching", "We could not purge the Cloudflare cache. \n \n" + CloudflareManager.PrintResultsSummary(results), EventMessageType.Warning));
            }
            else if (results.Any())
            {
                e.Messages.Add(new EventMessage("Cloudflare Caching", "Successfully purged the cloudflare cache.", EventMessageType.Success));
            }
        }

        private void AddPurgeCacheForContentMenu(TreeControllerBase sender, MenuRenderingEventArgs args)
        {
            if(sender.TreeAlias != "content")
            {
                //We aren't dealing with the content menu
                return;
            }

            MenuItem menuItem = new MenuItem("purgeCache", "Purge Cloudflare Cache");

            menuItem.Icon = "umbracoflare-tiny";

            menuItem.LaunchDialogView("/App_Plugins/UmbracoFlare/backoffice/treeViews/PurgeCacheDialog.html", "Purge Cloudflare Cache");

            args.Menu.Items.Insert(args.Menu.Items.Count - 1, menuItem);
        }

    }
}
