using log4net;
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

namespace UmbracoFlare.ApiControllers
{
    [PluginController("UmbracoFlare")]
    public class CloudflareUmbracoApiController : UmbracoAuthorizedApiController
    {
        //The Log
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [HttpPost]
        public StatusWithMessage PurgeCacheForUrls([FromBody]IEnumerable<string> urls)
        {
            if (urls == null || !urls.Any()) 
            { 
                return new StatusWithMessage( false, "You must provide urls to clear the cache for.") ;
            }

            urls = AccountForWildCards(urls);

            List<StatusWithMessage> results = CloudflareManager.Instance.PurgePages(urls);

            if(results.Any(x => !x.Success))
            {
                return new StatusWithMessage(false, CloudflareManager.PrintResultsSummary(results));
            }
            else
            {
                return new StatusWithMessage(true, String.Format("{0} urls purged successfully.", urls.Count()));
            }
        }

        [HttpPost]
        public StatusWithMessage PurgeStaticFiles([FromBody]string[] staticFiles)
        {
            List<string> allowedFileExtensions = new List<string>(){".css", ".js", ".jpg", ".png", ".gif", ".aspx", ".html"};   
            string generalSuccessMessage = "Successfully purged the cache for the selected static files.";
            string generalErrorMessage = "Sorry, we could not purge the cache for the static files.";
            if (staticFiles == null)
            {
                return new StatusWithMessage(false, generalErrorMessage);
            }

            if (!staticFiles.Any())
            {
                return new StatusWithMessage(true, generalSuccessMessage);
            }

            List<StatusWithMessage> errors;
            IEnumerable<string> allFilePaths = GetAllFilePaths(staticFiles, out errors);

            //Save a list of each individual file if it errors so we can give detailed errors to the user.
            List<StatusWithMessage> results = new List<StatusWithMessage>();

            string currentDomain = UrlHelper.GetCurrentDomainWithScheme(true);

            UrlHelper.GetCurrentDomainWithScheme(true);

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
                    string urlToPurge = currentDomain + filePath;
                    fullUrlsToPurge.Add(urlToPurge);
                }
            }

             results.AddRange(CloudflareManager.Instance.PurgePages(fullUrlsToPurge));

            if (results.Any(x => !x.Success))
            {
                return new StatusWithMessage(false, CloudflareManager.PrintResultsSummary(results));
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
            //get the current domain 
            string currentDomain = UrlHelper.GetCurrentDomainWithScheme(true);

            return CloudflareManager.Instance.PurgeEverything(currentDomain);
        }


        [HttpGet]
        public CloudflareConfigModel GetConfig()
        {
            CloudflareApiController cloudflareApi = new CloudflareApiController();
            UserDetails ud = cloudflareApi.GetUserDetails();

            return new CloudflareConfigModel()
            {
                PurgeCacheOn = CloudflareConfiguration.Instance.PurgeCacheOn,
                //AdditionalUrls = CloudflareConfiguration.Instance.AdditionalZoneUrls,
                ApiKey = CloudflareConfiguration.Instance.ApiKey,
                AccountEmail = CloudflareConfiguration.Instance.AccountEmail,
                CredentialsAreValid = ud != null && ud.Success
            };
        }


        [HttpPost]
        public StatusWithMessage PurgeCacheForContentNode([FromBody] PurgeCacheForIdParams args)
        {
            if (args.nodeId <= 0) { return new StatusWithMessage(false, "You must provide a node id."); }

            if (!CloudflareConfiguration.Instance.PurgeCacheOn) { return new StatusWithMessage(false, CloudflareMessages.CLOULDFLARE_DISABLED); }

            List<StatusWithMessage> statuses = new List<StatusWithMessage>();

            List<string> urlsToPurge = new List<string>();

            IPublishedContent content = Umbraco.TypedContent(args.nodeId);

            KeyValuePair<List<StatusWithMessage>, List<string>> statusesAndUrls = new KeyValuePair<List<StatusWithMessage>, List<string>>(statuses, urlsToPurge);

            statusesAndUrls = BuildUrlsToPurge(new List<IPublishedContent>() { content }, args.purgeChildren, statusesAndUrls);

            if (statusesAndUrls.Key.Count(x => x.Success) == 0 && !statusesAndUrls.Value.Any())
            {
                //No Successes
                return new StatusWithMessage(false, CloudflareManager.PrintResultsSummary(statusesAndUrls.Key));
            }

            StatusWithMessage resultFromPurge = PurgeCacheForUrls(statusesAndUrls.Value);

            if(resultFromPurge.Success)
            {
                return new StatusWithMessage(true, String.Format("{0}. There were {1} issues that are listed below: \n {2}", resultFromPurge.Message, statusesAndUrls.Key.Count(x => !x.Success), CloudflareManager.PrintResultsSummary(statusesAndUrls.Key)));
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
                CloudflareConfiguration.Instance.PurgeCacheOn = config.PurgeCacheOn;
                CloudflareConfiguration.Instance.ApiKey = config.ApiKey;
                CloudflareConfiguration.Instance.AccountEmail = config.AccountEmail;

                return GetConfig();
            }
            catch (Exception e)
            {
                Log.Error("Could not update cloudflare purge cache on state.", e);
                return null;
            }
        }



        [HttpGet]
        public IEnumerable<string> GetDomainsRegisteredWithUmbraco()
        {
            return Services.DomainService.GetAll(false).Select(x => x.DomainName);
        }


        private KeyValuePair<List<StatusWithMessage>, List<string>> BuildUrlsToPurge(IEnumerable<IPublishedContent> contentToPurge, bool includeChildren,  KeyValuePair<List<StatusWithMessage>, List<string>> statusesAndUrls)
        {
            //statusesAndUrls.Key => Statuses
            //SatusesAndUrls.Value => urls
            
            if(contentToPurge == null || !contentToPurge.Any())
            {
                return statusesAndUrls;
            }
            
            foreach (IPublishedContent content in contentToPurge)
            {
                if (content == null)
                {
                    statusesAndUrls.Key.Add(new StatusWithMessage(false, "We could not purge the cache for unpublished content."));
                }

                /*
                if (content.GetPropertyValue<bool>("cloudflareDisabledOnPublish"))
                {
                    statusesAndUrls.Key.Add(new StatusWithMessage(false, "You have Cloudflare purging disabled for page named " + content.Name + " under the 'Generic properties' tab"));
                    continue;
                }
                 * */

                //Add the url
                statusesAndUrls.Value.Add(content.UrlWithDomain());

                if (includeChildren)
                {
                    //recurse
                    statusesAndUrls = BuildUrlsToPurge(content.Children, includeChildren, statusesAndUrls);
                }
                else
                {
                    return statusesAndUrls;
                }
            }
            return statusesAndUrls;
        }


        private IEnumerable<string> AccountForWildCards(IEnumerable<string> urls)
        {
            IEnumerable<string> urlsWithWildCards = urls.Where(x => x.Contains('*'));

            if(urlsWithWildCards == null || !urlsWithWildCards.Any())
            {
                return urls;
            }

            UmbracoHelper uh = new UmbracoHelper(UmbracoContext.Current);

            return UmbracoUrlWildCardManager.Instance.GetAllUrlsForWildCardUrls(urlsWithWildCards, uh);
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
