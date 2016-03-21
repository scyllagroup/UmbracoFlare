using log4net;
using UmbracoFlare.ApiControllers;
using UmbracoFlare.Configuration;
using UmbracoFlare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UmbracoFlare.Helpers;

namespace UmbracoFlare.Manager
{
    public class CloudflareManager
    {
        private static CloudflareManager _instance = null;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private CloudflareConfiguration _config = CloudflareConfiguration.Instance;
        private CloudflareApiController _api;
        private IEnumerable<Zone> _zonesCache = null;

        public static CloudflareManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new CloudflareManager();
                }

                return _instance;
            }
        }

        private CloudflareManager()
        {
            _api = new CloudflareApiController();
        }


        public StatusWithMessage PurgeEverything(string domain)
        {
            //If the setting is turned off, then don't do anything.
            if (!CloudflareConfiguration.Instance.PurgeCacheOn) return new StatusWithMessage() { Success = false, Message = "Clould flare for umbraco is turned of as indicated in the config file." };


            //We only want the host and not the scheme or port number so just to ensure that is what we are getting we will
            //proccess it as a uri.
            Uri domainAsUri;
            try
            {
                domainAsUri= new Uri(domain);
                domain = domainAsUri.Authority;
            }
            catch(Exception e)
            {
                //So if we are here it didn't parse as an uri so we will assume that it was given in the correct format (without http://)
            }

            //Get the zone for the given domain
            Zone websiteZone = GetZone(domain);

            if (websiteZone == null)
            {
                //this will already be logged in the GetZone method so just relay that it was bad.
                return new StatusWithMessage(false, String.Format("We could not purge the cache because the domain {0} is not valid with the provided api key and email combo. Please ensure this domain is registered under these credentials on your cloudflare dashboard.", domain));
            }

            bool statusFromApi = this._api.PurgeCache(websiteZone.Id, null, true);

            if(!statusFromApi)
            {
                return new StatusWithMessage(false, CloudflareMessages.CLOUDFLARE_API_ERROR);
            }
            else
            {
                return new StatusWithMessage(true, ""); 
            }
        }
        

        public List<StatusWithMessage> PurgePages(IEnumerable<string> urls)
        {
            //If the setting is turned off, then don't do anything.
            if (!CloudflareConfiguration.Instance.PurgeCacheOn) return new List<StatusWithMessage>(){new StatusWithMessage(false, CloudflareMessages.CLOULDFLARE_DISABLED)};

            urls = UmbracoFlareDomainManager.Instance.FilterToAllowedDomains(urls);

            //Separate all of these into individual groups where the domain is the same that way we save some cloudflare requests.
            IEnumerable<IGrouping<string, string>> groupings = urls.GroupBy(url => UrlHelper.GetDomainFromUrl(url,true));

            List<StatusWithMessage> results = new List<StatusWithMessage>();

            //Now loop through each group.
            foreach (IGrouping<string, string> domainUrlGroup in groupings)
            {

                //get the domain without the scheme or port.
                Uri domain = new UriBuilder(domainUrlGroup.Key).Uri;

                //Get the zone for the current website as configured by the "zoneUrl" config setting in the web.config.
                Zone websiteZone = GetZone(domain.DnsSafeHost);

                if (websiteZone == null)
                {
                    //this will already be logged in the GetZone method so just relay that it was bad.
                    results.Add(new StatusWithMessage(false, String.Format("Could not retrieve the zone from cloudflare with the domain(url) of {0}", domain)));
                    continue; //to the next domain group.
                }

                //Make the request to the api using the urls from this domain group.
                bool apiResult = this._api.PurgeCache(websiteZone.Id, domainUrlGroup);

                if (!apiResult)
                {
                    results.Add(new StatusWithMessage(false, CloudflareMessages.CLOUDFLARE_API_ERROR));
                }
                else
                {
                    foreach (string url in domainUrlGroup)
                    {
                        //We need to  add x number of statuswithmessages that are true where x is the number urls
                        results.Add(new StatusWithMessage(true, String.Format("Purged for url {0}", url)));
                    }
                }
            }
            //return the results of all of the api calls.
            return results;
        }


        /// <summary>
        /// This will get a zone by domain(url)
        /// </summary>
        /// <param name="url">The url of the domain that we are getting the domain for.</param>
        /// <returns>The retreived zone</returns>
        public Zone GetZone(string url = null)
        {
            IEnumerable<Zone> zones = UmbracoFlareDomainManager.Instance.AllowedZones.Where(x => url.Contains(x.Name));

            //List<Zone> zones = this._api.ListZones(url);

            if(zones == null || !zones.Any())
            {
                Log.Error(String.Format("Could not retrieve the zone from cloudflare with the domain(url) of {0}", url));
                return null;
            }

            return zones.First();
        }


        public bool IsSSLEnabledOnCloudflare(string zoneId)
        {
            SslEnabledResponse sslResponse = this._api.GetSSLStatus(zoneId);

            return sslResponse.Result.Value != "off";
        }

        public IEnumerable<Zone> ListZones()
        {
            if(_zonesCache == null)
            {
                _zonesCache = this._api.ListZones();
            }
            return _zonesCache;
        }

        
        public static string PrintResultsSummary(IEnumerable<StatusWithMessage> results)
        {
            StringBuilder sb = new StringBuilder();
            
            //Show the number of successes
            sb.AppendLine(String.Format("There were {0} successes.", results.Count(x => x.Success)));

            foreach(StatusWithMessage failedStatus in results.Where(x=> !x.Success))
            {
                sb.AppendLine("Failed for reason: " + failedStatus.Message + ".  ");
            }

            return sb.ToString();
        }

        

    }
}
