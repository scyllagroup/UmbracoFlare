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
using UmbracoFlare.Manager;
using UmbracoFlare.Models;

namespace UmbracoFlare.Components
{
    public class FileSavingComponent : IComponent
    {
        private readonly ICloudflareManager cloudflareManager;
        private readonly IUmbracoFlareDomainManager domainManager;
        public FileSavingComponent(ICloudflareManager cloudflareManager)
        {
            this.cloudflareManager = cloudflareManager;
            this.domainManager = cloudflareManager.DomainManager;
        }
        public void Initialize()
        {
            FileService.SavedScript += PurgeCloudflareCacheForScripts;
            FileService.SavedStylesheet += PurgeCloudflareCacheForStylesheets;
        }

        public void Terminate()
        {
            FileService.SavedScript -= PurgeCloudflareCacheForScripts;
            FileService.SavedStylesheet -= PurgeCloudflareCacheForStylesheets;
        }

        private void PurgeCloudflareCacheForScripts(IFileService sender, SaveEventArgs<Script> e)
        {
            var files = e.SavedEntities.Select(se => se as File);
            PurgeCloudflareCacheForFiles(files, e);
        }

        private void PurgeCloudflareCacheForStylesheets(IFileService sender, SaveEventArgs<Stylesheet> e)
        {
            var files = e.SavedEntities.Select(se => se as File);
            PurgeCloudflareCacheForFiles(files, e);
        }

        private void PurgeCloudflareCacheForFiles<T>(IEnumerable<File> files, SaveEventArgs<T> e)
        {
            //If we have the cache buster turned off then just return.
            if (!cloudflareManager.Configuration.PurgeCacheOn) { return; }

            List<string> urls = new List<string>();
            //GetUmbracoDomains
            IEnumerable<string> domains = domainManager.AllowedDomains;

            foreach (var file in files)
            {
                if (!file.IsNew())
                {
                    urls.Add(file.VirtualPath);
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
