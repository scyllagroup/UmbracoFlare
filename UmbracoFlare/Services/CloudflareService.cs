using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Logging;
using UmbracoFlare.Configuration;
using UmbracoFlare.Models;

namespace UmbracoFlare.Services
{
    public class CloudflareService : ICloudflareService
    {
        public const string CLOUDFLARE_API_BASE_URL = "https://api.cloudflare.com/client/v4/";

        //The Log
        private readonly ICloudflareConfiguration cloudflareConfiguration;
        private readonly IProfilingLogger logger;

        public CloudflareService(ICloudflareConfiguration cloudflareConfiguration, IProfilingLogger logger)
        {
            this.cloudflareConfiguration = cloudflareConfiguration;
            this.logger = logger;
        }
        public SslEnabledResponse GetSSLStatus(string zoneId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(CLOUDFLARE_API_BASE_URL + "zones/" + zoneId + "/settings/ssl"),
                        Method = HttpMethod.Get,
                    };

                    //Add in the ApiKey and AccountEmail
                    AddRequestHeaders(request);

                    var responseContent = client.SendAsync(request).Result.Content;

                    var stringVersion = responseContent.ReadAsStringAsync().Result;

                    try
                    {
                        SslEnabledResponse response = responseContent.ReadAsAsync<SslEnabledResponse>().Result;

                        if (!response.Success)
                        {
                            //Something went wrong log the response
                            logger.Error<CloudflareService>(String.Format("Something went wrong because of {0}", response.Messages.ToString()));
                            return null;
                        }
                        else
                        {
                            return response;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error<CloudflareService>(String.Format("Something went wrong getting the SSL response back. The url that was used is {0}. The json that was used is {1}. The raw string value is {1}", request.RequestUri.ToString(), "", stringVersion), e);
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error<CloudflareService>(String.Format("Failed to get SSL status for zoneId {0}", zoneId), e);
                throw e;
            }
        }

        public UserDetails GetUserDetails()
        {
            try
            {
                using (var client = new HttpClient())
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

                    if (!userDetails.Success)
                    {
                        logger.Error<CloudflareService>(String.Format("Could not get the user details for user email {0}", cloudflareConfiguration.AccountEmail));
                        return null;
                    }

                    return userDetails;
                }
            }
            catch (Exception e)
            {
                logger.Error<CloudflareService>(String.Format("Could not get the user details for user email {0}", cloudflareConfiguration.AccountEmail), e);
                return null;
            }
        }

        public IEnumerable<Zone> ListZones(string name = null, bool throwExceptionOnFail = false)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string url = CLOUDFLARE_API_BASE_URL + "zones";

                    if (!String.IsNullOrEmpty(name))
                    {
                        url += "?name=" + HttpUtility.UrlEncode(name);
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
                        logger.Error<CloudflareService>(String.Format("Could not get the list of zones for name {0} because of {1}", name, response.Messages.ToString()));
                        return Enumerable.Empty<Zone>();
                    }

                    //Return the zones from the response.
                    return response.Zones;
                }
            }
            catch (Exception e)
            {
                logger.Error<CloudflareService>(String.Format("Could not get the List of zones for name {0}", name), e);
                if (throwExceptionOnFail)
                {
                    throw e;
                }
                else
                {
                    //We didn't want to throw an exception so just return an empty list
                    return Enumerable.Empty<Zone>();
                }
            }
        }

        public bool PurgeCache(string zoneIdentifier, IEnumerable<string> urls, bool purgeEverything = false, bool throwExceptionOnError = false)
        {
            try
            {
                if (String.IsNullOrEmpty(zoneIdentifier)) throw new ArgumentNullException("zoneIdentifier");

                if ((urls == null || !urls.Any()) && !purgeEverything)
                {
                    //Its probably worthy to log that we were going to purge pages but no url was given
                    logger.Info<CloudflareService>("PurgeIndividualPages was called but there were no urls given to purge nor are we purging everything");
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
                            logger.Error<CloudflareService>(String.Format("Something went wrong because of {0}", response.Messages.ToString()));
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error<CloudflareService>(String.Format("Something went wrong getting the purge cache response back. The url that was used is {0}. The json that was used is {1}. The raw string value is {1}", request.RequestUri.ToString(), json, stringVersion), e);
                        return false;
                    }


                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Error<CloudflareService>(String.Format("Failed to purge individual pages for urls {0}", urls.Select(x => { return x + "x"; })), e);
                if (throwExceptionOnError)
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
            request.Headers.Add("X-Auth-Key", cloudflareConfiguration.ApiKey);
            request.Headers.Add("X-Auth-Email", cloudflareConfiguration.AccountEmail);
        }
    }
}
