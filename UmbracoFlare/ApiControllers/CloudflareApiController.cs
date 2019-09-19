using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Threading.Tasks;
using Umbraco.Web.WebApi;
using System.Net.Http;
using System.Net.Http.Headers;
using UmbracoFlare.Configuration;
using System.Reflection;
using Newtonsoft.Json;
using Umbraco.Web.Mvc;
using UmbracoFlare.Models;
using UmbracoFlare.Services;

namespace UmbracoFlare.ApiControllers
{
    /// <summary>
    /// A way to interact with the Cloudflare API. Docs can be found here: https://api.cloudflare.com
    /// </summary>
    [PluginController("UmbracoFlare")]
    ///umbraco/backoffice/UmbracoFlare/CloudflareApi/
    public class CloudflareApiController : UmbracoAuthorizedApiController
    {
        //The API Endpoint.
        public const string CLOUDFLARE_API_BASE_URL = "https://api.cloudflare.com/client/v4/";

        //The Log
        private readonly ICloudflareService cloudflareService;

        public CloudflareApiController(ICloudflareService cloudflareService)
            : base()
        {
            //Get the ApiKey and AccountEmail from the web.config settings.
            this.cloudflareService = cloudflareService;
        }



        /// <summary>
        /// Gets the user details for the current users credentials in the config. This is useful to test if the credentials are valid.  
        /// </summary>
        /// <returns>The user details object.</returns>
        public UserDetails GetUserDetails()
        {
            return cloudflareService.GetUserDetails();
        }


        /// <summary>
        /// Gets the user details for the current users credentials in the config. This is useful to test if the credentials are valid.  
        /// </summary>
        /// <returns>The user details object.</returns>
        public SslEnabledResponse GetSSLStatus(string zoneId)
        {
            return cloudflareService.GetSSLStatus(zoneId);
        }

        /// <summary>
        /// Call the cloudflare api to get a list of zones associated with this api key / account email combo. If you pass in a name (domain name), it will return 
        /// that zone.
        /// </summary>
        /// <param name="name">The domain name of the zone that you wish to get the info about. If you want all of them, leave it blank.</param>
        /// <returns>A list of zones.</returns>
        public IEnumerable<Zone> ListZones(string name = null, bool throwExceptionOnFail = false)
        {
            return cloudflareService.ListZones(name, throwExceptionOnFail);
        }

        


        /// <summary>
        /// This will call the Cloudflare api and will purge the individual pages or files given in the urls parameter.
        /// </summary>
        /// <param name="urls">The urls of the pages/files that you want to purge the cache for on cloudflare. If it is empty or null, the function will just return 
        /// and no api call will be made.</param>
        /// <param name="zoneIdentifier">This is the id of the zone you want to purge the urls from. Can be obtained through ListZones</param>
        /// <param name="purgeEverything">If set to true, the urls will be ignored and we will purge everything.</param>
        /// <returns>A bool indicating whether the call was made successfully.</returns>
        public bool PurgeCache(string zoneIdentifier, IEnumerable<string> urls, bool purgeEverything = false, bool throwExceptionOnError = false)
        {
            return cloudflareService.PurgeCache(zoneIdentifier, urls, purgeEverything, throwExceptionOnError);
        }
    }
}
