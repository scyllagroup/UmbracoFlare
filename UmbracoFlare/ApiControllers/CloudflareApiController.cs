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
using log4net;
using System.Reflection;
using Newtonsoft.Json;
using Umbraco.Web.Mvc;
using UmbracoFlare.Models;

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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string _apiKey;
        private static string _accountEmail;

        public CloudflareApiController()
            : base()
        {
            //Get the ApiKey and AccountEmail from the web.config settings.
            _apiKey = CloudflareConfiguration.Instance.ApiKey;
            _accountEmail = CloudflareConfiguration.Instance.AccountEmail;
        }



        /// <summary>
        /// Gets the user details for the current users credentials in the config. This is useful to test if the credentials are valid.  
        /// </summary>
        /// <returns>The user details object.</returns>
        public UserDetails GetUserDetails()
        {
            try
            {
                using(var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string url = CLOUDFLARE_API_BASE_URL + "user";

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(url),
                        Method = HttpMethod.Get
                    };

                    AddRequestHeaders(request);

                    var responseContent = client.SendAsync(request).Result.Content;

                    UserDetails userDetails = responseContent.ReadAsAsync<UserDetails>().Result;

                    if(!userDetails.Success)
                    {
                        Log.Error(String.Format("Could not get the user details for user email {0}", _accountEmail));
                        return null;
                    }

                    return userDetails;
                }
            }
            catch(Exception e)
            {
                Log.Error(String.Format("Could not get the user details for user email {0}", _accountEmail), e);
                return null;   
            }
        }


        /// <summary>
        /// Call the cloudflare api to get a list of zones associated with this api key / account email combo. If you pass in a name (domain name), it will return 
        /// that zone.
        /// </summary>
        /// <param name="name">The domain name of the zone that you wish to get the info about. If you want all of them, leave it blank.</param>
        /// <returns>A list of zones.</returns>
        public List<Zone> ListZones(string name = null, bool throwExceptionOnFail = false)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string url = CLOUDFLARE_API_BASE_URL + "zones";

                    if(!String.IsNullOrEmpty(name))
                    {
                        url += "?name="+ HttpUtility.UrlEncode(name);
                    }

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(url),
                        Method = HttpMethod.Get,
                    };

                    //Add in the ApiKey and AccountEmail
                    AddRequestHeaders(request);

                    var responseContent = client.SendAsync(request).Result.Content;

                    ListZonesResponse response = responseContent.ReadAsAsync<ListZonesResponse>().Result;

                    if (!response.Success)
                    {
                        //Something went wrong log the response
                        Log.Error(String.Format("Could not get the list of zones for name {0} because of {1}", name, response.Messages.ToString()));
                        return new List<Zone>();
                    }

                    //Return the zones from the response.
                    return response.Zones;
                }
            }
            catch(Exception e)
            {
                Log.Error(String.Format("Could not get the List of zones for name {0}", name), e);
                if(throwExceptionOnFail)
                {
                    throw e;
                }
                else
                {
                    //We didn't want to throw an exception so just return an empty list
                    return new List<Zone>();
                }
            }
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
            try
            {
                if (String.IsNullOrEmpty(zoneIdentifier)) throw new ArgumentNullException("zoneIdentifier");

                if ((urls == null || !urls.Any()) && !purgeEverything)
                {
                    //Its probably worthy to log that we were going to purge pages but no url was given
                    Log.Info("PurgeIndividualPages was called but there were no urls given to purge nor are we purging everything");
                    return true;
                }

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string json;

                    if (purgeEverything)
                    {
                        json = "{\"purge_everything\":true}";
                    }
                    else
                    {
                        json = String.Format("{{\"files\":{0}}}", JsonConvert.SerializeObject(urls));
                    }
                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(CLOUDFLARE_API_BASE_URL + "zones/" + zoneIdentifier + "/purge_cache"),
                        Method = HttpMethod.Delete,
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };

                    //Add in the ApiKey and AccountEmail
                    AddRequestHeaders(request);

                    
                    var responseContent = client.SendAsync(request).Result.Content;

                    var stringVersion = responseContent.ReadAsStringAsync().Result;

                    try
                    {
                        BasicCloudflareResponse response = responseContent.ReadAsAsync<BasicCloudflareResponse>().Result;


                        if (!response.Success)
                        {
                            //Something went wrong log the response
                            Log.Error(String.Format("Something went wrong because of {1}", response.Messages.ToString()));
                            return false;
                        }
                    }
                    catch(Exception e)
                    {
                        Log.Error(String.Format("Something went wrong getting the purge cache response back. The url that was used is {0}. The json that was used is {1}. The raw string value is {1}", request.RequestUri.ToString(), json, stringVersion));
                        return false;
                    }
                    

                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Error(String.Format("Failed to purge individual pages for urls {0}", urls.Select(x => {return x + "x";})), e);
                if(throwExceptionOnError)
                {
                    throw e;
                }
                else
                {
                    return false;
                }
            }
        }


        private void AddRequestHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("X-Auth-Key", _apiKey);
            request.Headers.Add("X-Auth-Email", _accountEmail);
        }
    }
}
