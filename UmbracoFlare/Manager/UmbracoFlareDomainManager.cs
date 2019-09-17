using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Routing;
using UmbracoFlare.Helpers;
using UmbracoFlare.Models;
using UmbracoFlare.Services;

namespace UmbracoFlare.Manager
{
    public class UmbracoFlareDomainManager : IUmbracoFlareDomainManager
    {

        private readonly ICloudflareService cloudflareService;
        private readonly IContentService contentService;
        private readonly IDomainService domainService;
        private readonly UmbracoContext umbracoContext;
        private IEnumerable<Zone> _allowedZones;
        private IEnumerable<string> _allowedDomains;

        public UmbracoFlareDomainManager(
                ICloudflareService cloudflareService, 
                IContentService contentService, 
                IDomainService domainService, 
                UmbracoContext umbracoContext
            )
        {
            this.cloudflareService = cloudflareService;
            this.contentService = contentService;
            this.domainService = domainService;
            this.umbracoContext = umbracoContext;
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
            IContent content = contentService.GetById(contentId);
            
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

            string url = GetUrl(content);

            urls.AddRange(UrlHelper.MakeFullUrlWithDomain(url, RecursivelyGetParentsDomains(new List<string>(), content)));
            urls.AddRange(GetotherUrls(content));

            if (includeDescendants)
            {
                var numDescendants = contentService.CountDescendants(content.Id);
                long outParam;
                foreach (IContent desc in contentService.GetPagedDescendants(content.Id, 0, numDescendants, out outParam))
                {
                    urls.Add(GetUrl(desc));
                    urls.AddRange(GetotherUrls(desc));
                }
            }

            return urls;
        }

        private string GetUrl(IContent content)
        {
            return umbracoContext.UrlProvider.GetUrl(content.Id);
        }

        private IEnumerable<string> GetotherUrls(IContent content)
        {
            return umbracoContext.UrlProvider.GetOtherUrls(content.Id).Where(x => x.IsUrl).Select(x => x.Text);
        }


        private List<string> RecursivelyGetParentsDomains(List<string> domains, IContent content)
        {
            //Termination case
            if(content == null)
            {
                return domains;
            }

            domains.AddRange(domainService.GetAssignedDomains(content.Id, false).Select(x => x.DomainName));

            domains = RecursivelyGetParentsDomains(domains, contentService.GetParent(content));

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
                domains.AddRange(domainService.GetAssignedDomains(descendant.Id, false).Select(x => x.DomainName));
            }

            return domains;
        }

        private IEnumerable<Zone> _zonesCache = null;
        public IEnumerable<Zone> ListZones()
        {
            if (_zonesCache == null || !_zonesCache.Any())
            {
                _zonesCache = this.cloudflareService.ListZones();
            }
            return _zonesCache;
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
            IEnumerable<Zone> allZones = ListZones();

            IEnumerable<string> domainsInUmbraco = domainService.GetAll(false).Select(x => new UriBuilder(x.DomainName).Uri.DnsSafeHost);

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


        public IEnumerable<string> GetDomainsFromCloudflareZones()
        {
            return this.AllowedZones.Select(x => x.Name);
        }

    }
}
