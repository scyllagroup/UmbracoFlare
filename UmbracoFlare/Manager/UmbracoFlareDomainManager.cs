using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using UmbracoFlare.Helpers;
using UmbracoFlare.Models;

namespace UmbracoFlare.Manager
{
    public class UmbracoFlareDomainManager
    {
        private static UmbracoFlareDomainManager _instance = null;

        private CloudflareManager _cloudflareManager;

        private IEnumerable<Zone> _allowedZones;
        private IEnumerable<string> _allowedDomains;

        private UmbracoFlareDomainManager()
        {
            _cloudflareManager = CloudflareManager.Instance;
        }

        public static UmbracoFlareDomainManager Instance 
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new UmbracoFlareDomainManager();
                }

                return _instance;
            }
        }

        public IEnumerable<Zone> AllowedZones {
            get
            {
                if(_allowedZones == null)
                {
                    _allowedZones = GetAllowedZonesAndDomains().Key;
                }
                return _allowedZones;
            }
        }

        public IEnumerable<string> AllowedDomains
        {
            get
            {
                if(_allowedDomains == null || !_allowedDomains.Any())
                {
                    _allowedDomains = GetAllowedZonesAndDomains().Value;
                }
                return _allowedDomains;
            }
        }


        /// <summary>
        /// Gets the domains from each cloudflare zone.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetDomainsFromCloudflareZones()
        {
            return this.AllowedZones.Select(x => x.Name);
        }

        /// <summary>
        /// This will take a list of domains and make sure that they are either equal to or a subdomain of the allowed domains in cloudflare.
        /// </summary>
        /// <param name="domains">The domains to filter.</param>
        /// <returns>The filtered list of domains.</returns>
        public IEnumerable<string> FilterToAllowedDomains(IEnumerable<string> domains)
        {
            List<string> filteredDomains = new List<string>();

            foreach(string allowedDomain in this.AllowedDomains)
            {
                foreach(string posDomain in domains)
                {
                    //Is the possible domain an allowed domain? 
                    if(posDomain.Contains(allowedDomain))
                    {
                        if(!filteredDomains.Contains(posDomain))
                        {
                            filteredDomains.Add(posDomain);
                        }
                    }
                }
            }
            return filteredDomains;
        }


        public List<string> GetUrlsForNode(int contentId, bool includeDescendants = false)
        {
            IContent content = ApplicationContext.Current.Services.ContentService.GetById(contentId);
            
            if(content == null)
            {
                return new List<string>();
            }

            return GetUrlsForNode(content, includeDescendants);
        }


        /// <summary>
        /// We want to get the domains associated with the content node. This includes getting the allowed domains on the node as well as
        /// searching up the tree to get the parents nodes since they are inherited.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        public List<string> GetUrlsForNode(IContent content, bool includeDescendants = false)
        {
            List<string> urls = new List<string>();

            urls.Add(UmbracoContext.Current.RoutingContext.UrlProvider.GetUrl(content.Id, true));
            urls.AddRange(UmbracoContext.Current.RoutingContext.UrlProvider.GetOtherUrls(content.Id));

            if(includeDescendants)
            {
                foreach(IContent desc in content.Descendants())
                {
                    urls.Add(UmbracoContext.Current.RoutingContext.UrlProvider.GetUrl(desc.Id, true));
                    urls.AddRange(UmbracoContext.Current.RoutingContext.UrlProvider.GetOtherUrls(desc.Id));
                }
            }

            return urls;
        }

        

        private List<string> RecursivelyGetParentsDomains(List<string> domains, IContent content)
        {
            //Termination case
            if(content == null)
            {
                return domains;
            }

            domains.AddRange(ApplicationContext.Current.Services.DomainService.GetAssignedDomains(content.Id, false).Select(x => x.DomainName));

            domains = RecursivelyGetParentsDomains(domains, content.Parent());

            return domains;
        }

        private List<string> GetDescendantsDomains(List<string> domains, IEnumerable<IContent> descendants)
        {
            if(descendants == null || !descendants.Any())
            {
                return domains;
            }

            foreach(IContent descendant in descendants)
            {
                domains.AddRange(ApplicationContext.Current.Services.DomainService.GetAssignedDomains(descendant.Id, false).Select(x => x.DomainName));
            }

            return domains;
        }

        /// <summary>
        /// This method will grab the list of zones from cloudflare. It will then check to make sure that the zone has some corresponding hostnames in umbraco.
        /// If it does, we will save the domain names & the zones.
        /// </summary>
        /// <returns>A key value pair where the Key is the zones, and the value is the domains</returns>
        private KeyValuePair<IEnumerable<Zone>, IEnumerable<string>> GetAllowedZonesAndDomains()
        {
            List<Zone> allowedZones = new List<Zone>();
            List<string> allowedDomains = new List<string>();

            //Get the list of domains from cloudflare.
            IEnumerable<Zone> allZones = _cloudflareManager.ListZones();

            IEnumerable<string> domainsInUmbraco = ApplicationContext.Current.Services.DomainService.GetAll(false).Select(x => new UriBuilder(x.DomainName).Uri.DnsSafeHost);

            foreach(Zone zone in allZones)
            {
                foreach(string domain in domainsInUmbraco)
                {
                    if (domain.Contains(zone.Name)) //if the domain url contains the zone url, then we know its the domain or a sub domain.
                    {
                        if(!allowedZones.Contains(zone))
                        {
                            //The allowed zones doesn't contain this zone yet, add it.
                            allowedZones.Add(zone);
                        }
                        if (!allowedDomains.Contains(domain))
                        {
                            //The allowed domains doens't contain this domain yet, add it.
                            allowedDomains.Add(domain);
                        }
                    }
                }
            }
            return new KeyValuePair<IEnumerable<Zone>, IEnumerable<string>>(allowedZones, allowedDomains);
        }



    }
}
