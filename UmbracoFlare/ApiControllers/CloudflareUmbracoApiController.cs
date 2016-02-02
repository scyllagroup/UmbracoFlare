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
        public StatusWithMessage PurgeCacheForUrls([FromBody]string[] urls)
        {
            if (urls == null || !urls.Any()) 
            { 
                return new StatusWithMessage( false, "You must provide urls to clear the cache for.") ;
            }
            
            List<StatusWithMessage> results = CloudflareManager.Instance.PurgePages(urls);

            if(results.Any(x => !x.Success))
            {
                return new StatusWithMessage(false, CloudflareManager.PrintResultsSummary(results));
            }
            else
            {
                return new StatusWithMessage(true, "Urls purged successfully.");
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
        public StatusWithMessage PurgeCacheForContentNode([FromBody] int nodeId)
        {
            if (nodeId <= 0) { return new StatusWithMessage(false, "You must provide a node id."); }

            if (!CloudflareConfiguration.Instance.PurgeCacheOn) { return new StatusWithMessage(false, CloudflareMessages.CLOULDFLARE_DISABLED); }

            IPublishedContent content = Umbraco.TypedContent(nodeId);

            if (content.GetPropertyValue<bool>("cloudflareDisabledOnPublish")) { return new StatusWithMessage(false, "You have Cloudflare purging disabled for this page under the 'Generic properties' tab"); }

            if (content == null) 
            {
                //if the content is null, there is a possibility its not published. lets see if we can find it through the content service.
                IContent c = Services.ContentService.GetById(nodeId);

                if(c != null && !c.Published){
                    //we found it through the content service and its NOT published
                    //if this is the case, then we should not allow them to purge the cache for an unpublished item. 
                    return new StatusWithMessage(false, "You must publish this item before we will clear the cache.");
                }
                else
                {
                    return new StatusWithMessage(false, "Could not find the content with the node id of" + nodeId ); 
                }
            }

            string url = content.UrlWithDomain();

            return PurgeCacheForUrls(new string[1] { url });

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
    }

    public class StaticFilePurgeParams
    {
        public string[] staticFiles { get; set; }
        public string forDomain { get; set; }
    }
}
