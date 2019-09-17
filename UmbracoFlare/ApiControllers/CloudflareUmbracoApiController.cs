using UmbracoFlare.Configuration;
using UmbracoFlare.Manager;
using UmbracoFlare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.FileSystemPicker.Controllers;
using System.IO;
using Umbraco.Core.IO;
using UmbracoFlare.Helpers;
using Umbraco.Web.Routing;
using UmbracoFlare.Services;
using Umbraco.Core.Models.PublishedContent;

namespace UmbracoFlare.ApiControllers
{
    [PluginController("UmbracoFlare")]
    public class CloudflareUmbracoApiController : UmbracoAuthorizedApiController
    {
        //The Log
        private readonly ICloudflareManager cloudflareManager;
        private readonly ICloudflareService cloudflareService;
        private readonly IUrlWildCardManager wildCardManager;

        public CloudflareUmbracoApiController(ICloudflareManager cloudflareManager, ICloudflareService cloudflareService, IUrlWildCardManager wildCardManager)
        {
            this.cloudflareManager = cloudflareManager;
            this.cloudflareService = cloudflareService;
            this.wildCardManager = wildCardManager;
        }

        [HttpPost]
        public StatusWithMessage PurgeCacheForUrls([FromBody]PurgeCacheForUrlsRequestModel model)
        {
            /*Important to note that the urls can come in here in two different ways. 
             *1) They can come in here without domains on them. If that is the case then the domains property should have values.
             *      1a) They will need to have the urls built by appending each domain to each url. These urls technically might not exist
             *          but that is the responsibility of whoever called this method to ensure that. They will still go to cloudflare even know the
             *          urls physically do not exists, which is fine because it won't cause an error. 
             *2) They can come in here with domains, if that is the case then we are good to go, no work needed.
             * 
             * */

            if (model.Urls == null || !model.Urls.Any()) 
            { 
                return new StatusWithMessage( false, "You must provide urls to clear the cache for.") ;
            }

            List<string> builtUrls = new List<string>();

            //Check to see if there are any domains. If there are, then we know that we need to build the urls using the domains
            if(model.Domains != null && model.Domains.Any())
            {
                builtUrls.AddRange(UrlHelper.MakeFullUrlWithDomain(model.Urls, model.Domains, true));   
            }
            else
            {
                builtUrls = model.Urls.ToList();
            }

            builtUrls.AddRange(AccountForWildCards(builtUrls));
            
            IEnumerable<StatusWithMessage> results = cloudflareManager.PurgePages(builtUrls);

            if(results.Any(x => !x.Success))
            {
                return new StatusWithMessage(false, cloudflareManager.PrintResultsSummary(results));
            }
            else
            {
                return new StatusWithMessage(true, String.Format("{0} urls purged successfully.", results.Count(x => x.Success)));
            }
        }

        [HttpPost]
        public StatusWithMessage PurgeStaticFiles([FromBody]PurgeStaticFilesRequestModel model)
        {
            List<string> allowedFileExtensions = new List<string>(){".css", ".js", ".jpg", ".png", ".gif", ".aspx", ".html"};   
            string generalSuccessMessage = "Successfully purged the cache for the selected static files.";
            string generalErrorMessage = "Sorry, we could not purge the cache for the static files.";
            if (model.StaticFiles == null)
            {
                return new StatusWithMessage(false, generalErrorMessage);
            }

            if (!model.StaticFiles.Any())
            {
                return new StatusWithMessage(true, generalSuccessMessage);
            }

            List<StatusWithMessage> errors;
            IEnumerable<string> allFilePaths = GetAllFilePaths(model.StaticFiles, out errors);

            //Save a list of each individual file if it errors so we can give detailed errors to the user.
            List<StatusWithMessage> results = new List<StatusWithMessage>();

            List<string> fullUrlsToPurge = new List<string>();
            //build the urls with the domain we are on now
            foreach (string filePath in allFilePaths)
            {
                string extension = Path.GetExtension(filePath);

                if(!allowedFileExtensions.Contains(extension))
                {
                    //results.Add(new StatusWithMessage(false, String.Format("You cannot purge the file {0} because its extension is not allowed.", filePath)));
                }
                else
                {
                    fullUrlsToPurge.AddRange(UrlHelper.MakeFullUrlWithDomain(filePath, model.Hosts, true));                    
                }
            }

             results.AddRange(cloudflareManager.PurgePages(fullUrlsToPurge));

            if (results.Any(x => !x.Success))
            {
                return new StatusWithMessage(false, cloudflareManager.PrintResultsSummary(results));
            }
            else
            {
                return new StatusWithMessage(true, String.Format("{0} static files purged successfully.", results.Where(x => x.Success).Count()));
            }
        }



        private IEnumerable<string> GetAllFilePaths(string[] filesOrFolders, out List<StatusWithMessage> errors)
        {
            errors = new List<StatusWithMessage>();

            string rootOfApplication = IOHelper.MapPath("~/");

            List<string> filePaths = new List<string>();

            //the static files could have files or folders, we are sure at this point so we need to collect all of the 
            //files.
            FileSystemPickerApiController fileSystemApi = new FileSystemPickerApiController();

            foreach (string fileOrFolder in filesOrFolders)
            {
                if (String.IsNullOrEmpty(fileOrFolder)) continue;

                try
                {
                    //Map the file path to the server.
                    string fileOrFolderPath = IOHelper.MapPath(fileOrFolder);
                    FileAttributes attr = System.IO.File.GetAttributes(fileOrFolderPath);

                    //Check to see if its a folder
                    if ((attr & System.IO.FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        IEnumerable<FileInfo> filesInTheFolder = fileSystemApi.GetFilesIncludingSubDirs(fileOrFolderPath);

                        filePaths.AddRange(filesInTheFolder.Select(x =>
                        {
                            string directory = x.Directory.FullName.Replace(rootOfApplication, "");
                            directory = directory.Replace("\\", "/");
                            return directory + "/" + x.Name;
                        }));
                    }
                    else
                    {   
                        

                        if (!System.IO.File.Exists(fileOrFolderPath))
                        {
                            //File does not exist, continue and log the error.
                            errors.Add(new StatusWithMessage(false, String.Format("Could not find file with the path {0}", fileOrFolderPath)));
                            continue;
                        }

                        if (fileOrFolder.StartsWith("/"))
                        {
                            filePaths.Add(fileOrFolder.TrimStart('/'));
                        }
                        else
                        {
                            filePaths.Add(fileOrFolder);
                        }
                        
                    }
                }
                catch(Exception e)
                {

                }
                
            }

            return filePaths;
        }


        [HttpPost]
        public StatusWithMessage PurgeAll()
        {
            //it doesn't matter what domain we pick bc it will purge everything. 
            IEnumerable<string> domains = cloudflareManager.DomainManager.GetDomainsFromCloudflareZones();

            List<StatusWithMessage> results = new List<StatusWithMessage>();

            foreach(string domain in domains)
            {
                results.Add(cloudflareManager.PurgeEverything(domain));
            }

            return new StatusWithMessage() { Success = !results.Any(x => !x.Success), Message = cloudflareManager.PrintResultsSummary(results) };
        }


        [HttpGet]
        public CloudflareConfigModel GetConfig()
        {
            UserDetails ud = cloudflareService.GetUserDetails();
            var config = cloudflareManager.Configuration;
            return new CloudflareConfigModel()
            {
                PurgeCacheOn = config.PurgeCacheOn,
                //AdditionalUrls = CloudflareConfiguration.Instance.AdditionalZoneUrls,
                ApiKey = config.ApiKey,
                AccountEmail = config.AccountEmail,
                CredentialsAreValid = ud != null && ud.Success
            };
        }


        [HttpPost]
        public StatusWithMessage PurgeCacheForContentNode([FromBody] PurgeCacheForIdParams args)
        {
            if (args.nodeId <= 0) { return new StatusWithMessage(false, "You must provide a node id."); }

            if (!cloudflareManager.Configuration.PurgeCacheOn) { return new StatusWithMessage(false, CloudflareMessages.CLOULDFLARE_DISABLED); }

            List<string> domains = new List<string>();

            List<string> urlsToPurge = new List<string>();

            IPublishedContent content = Umbraco.Content(args.nodeId);


            List<string> urls = BuildUrlsToPurge(content, args.purgeChildren);

            StatusWithMessage resultFromPurge = PurgeCacheForUrls(new PurgeCacheForUrlsRequestModel() { Urls = urls, Domains = null });
            if(resultFromPurge.Success)
            {
                return new StatusWithMessage(true, String.Format("{0}", resultFromPurge.Message));
            }
            else
            {
                return resultFromPurge;
            }
        }


        [HttpPost]
        public CloudflareConfigModel UpdateConfigStatus([FromBody]CloudflareConfigModel config)
        {
            try
            {
                cloudflareManager.Configuration.PurgeCacheOn = config.PurgeCacheOn;
                cloudflareManager.Configuration.ApiKey = config.ApiKey;
                cloudflareManager.Configuration.AccountEmail = config.AccountEmail;

                return GetConfig();
            }
            catch (Exception e)
            {
                Logger.Error(typeof(CloudflareUmbracoApiController),"Could not update cloudflare purge cache on state.", e);
                return null;
            }
        }



        [HttpGet]
        public IEnumerable<string> GetAllowedDomains()
        {
            return cloudflareManager.DomainManager.AllowedDomains;
        }


        private List<string> BuildUrlsToPurge(IPublishedContent contentToPurge, bool includeChildren)
        {
            List<string> urls = new List<string>();
            
            if(contentToPurge == null)
            {
                return urls;
            }

            urls.AddRange(cloudflareManager.DomainManager.GetUrlsForNode(contentToPurge.Id, includeChildren));
            
            return urls;
        }


        private IEnumerable<string> AccountForWildCards(IEnumerable<string> urls)
        {
            IEnumerable<string> urlsWithWildCards = urls.Where(x => x.Contains('*'));

            if(urlsWithWildCards == null || !urlsWithWildCards.Any())
            {
                return urls;
            }


            return this.wildCardManager.GetAllUrlsForWildCardUrls(urlsWithWildCards);
        }
    }

    public class StaticFilePurgeParams
    {
        public string[] staticFiles { get; set; }
        public string forDomain { get; set; }
    }

    public class PurgeCacheForIdParams
    {
        public int nodeId { get; set; }
        public bool purgeChildren { get; set; }
    }
}
