using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Umbraco.Web.Composing;

namespace UmbracoFlare.Helpers
{
    public class UrlHelper
    {

        /// <summary>
        /// Takes the given url and returns the domain with the scheme (no path and query)
        /// ex. http://www.example.com/blah/blah?blah=blah will return http://www.example.com(/)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withTrailingSlash"></param>
        /// <returns></returns>
        public static string GetDomainFromUrl(string url, bool withScheme = false, bool withTrailingSlash = false)
        {
            Uri uri;
            //first try parsing the Url
            try
            {

                uri = new UriBuilder(url).Uri;
            }
            catch(Exception e)
            {
                //try to add 
                return MakeFullUrlWithDomain(url, null, true, withScheme);
            }

            if(withScheme)
            {
                return AddSchemeToUrl(uri.ToString()) + (withTrailingSlash ? "/" : "");
            }
            else
            {
                return uri.DnsSafeHost + (withTrailingSlash ? "/" : "");
            }
        }

        public static Uri MakeUriFromUrl(string url)
        {
            try
            {
                return new Uri(url);
            }
            catch(Exception e)
            {
                return new UriBuilder(url).Uri;
            }
        }


        public static string GetCurrentDomainWithScheme(bool withTrailingSlash = false)
        {
            string currentDomain = MakeFullUrlWithDomain("", null, true, true);

            if(withTrailingSlash)
            {
                if(currentDomain[currentDomain.Length -1] != '/')
                {
                    currentDomain = currentDomain + "/";
                }
            }

            return currentDomain;
        }


        public static IEnumerable<string> MakeFullUrlWithDomain(IEnumerable<string> urls, string host, bool withScheme = false)
        {
            if (urls == null) return urls;

            List<string> urlsWithDomain = new List<string>();

            foreach(string url in urls)
            {
                urlsWithDomain.Add(MakeFullUrlWithDomain(url, host, String.IsNullOrEmpty(host) ? true : false, withScheme));
            }

            return urlsWithDomain;
        }


        public static IEnumerable<string> MakeFullUrlWithDomain(string url, IEnumerable<string> hosts, bool withScheme = false)
        {
            if (String.IsNullOrEmpty(url)) return new List<string>();

            if(hosts == null || !hosts.Any())
            {
                //there aren't any hosts passed in so use the current domain. 
                return new List<string>() { MakeFullUrlWithDomain(url, null, true, false) };
            }

            List<string> urlsWithDomain = new List<string>();

            foreach(string host in hosts)
            {
                urlsWithDomain.Add(MakeFullUrlWithDomain(url, host, withScheme: withScheme));
            }
            return urlsWithDomain;
        }

        public static IEnumerable<string> MakeFullUrlWithDomain(IEnumerable<string> urls, IEnumerable<string> hosts, bool withScheme = false)
        {
            if (urls == null || hosts == null) return new List<string>();

            List<string> urlsWithDomains = new List<string>();

            foreach(string url in urls)
            {
                foreach(string host in hosts)
                {
                    urlsWithDomains.Add(MakeFullUrlWithDomain(url, host, withScheme:withScheme));
                }
            }
            return urlsWithDomains;
        }


        /// <summary>
        /// Uses the HttpContext.Current to add on the scheme & host to the given url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string MakeFullUrlWithDomain(string url, string host, bool useCurrentDomain = false, bool withScheme = false)
        {
            if(String.IsNullOrEmpty(host) && !useCurrentDomain)
            {
                //there shoulda been a host name passed in.
                return url;
            }

            if(useCurrentDomain && !String.IsNullOrEmpty(host))
            {
                throw new Exception("If you are using the current domain, you CANNOT pass in a host as well.");
            }

            string returnUrl = "";

            Uri uriWithDomain;
            try
            {
                uriWithDomain = new Uri(url);

                if (!String.IsNullOrEmpty(uriWithDomain.Host))
                {
                    //The url already has a host, but we want it to have the host we passed in.

                    returnUrl = CombinePaths(host, uriWithDomain.PathAndQuery);
                }
            }
            catch
            { }

            //if we made it here we know that the host was not added to the url.
            try
            {
                if(useCurrentDomain)
                {
                    if (HttpContext.Current != null || HttpContext.Current.Request == null)
                    {
                        Current.Logger.Error(typeof(UrlHelper),String.Format("HttpContext.Current or HttpContext.Current.Request is null."));
                    }
                    Uri root = new Uri(String.Format("{0}{1}{2}", HttpContext.Current.Request.Url.Scheme, Uri.SchemeDelimiter, HttpContext.Current.Request.Url.Host));
                    uriWithDomain = new Uri(root, url);

                    returnUrl = uriWithDomain != null ? uriWithDomain.ToString() : url;
                }
                else
                {
                    returnUrl = CombinePaths(host, url);
                }
            }
            catch
            {
                Current.Logger.Error(typeof(UrlHelper), String.Format("Could not create root uri using http context request url {0}", HttpContext.Current.Request.Url));
                return String.Empty;
            }

            if(withScheme)
            {
                return AddSchemeToUrl(returnUrl);
            }
            else
            {
                return returnUrl;
            }
        }




        public static string AddSchemeToUrl(string url)
        {
            try
            {
                if (!String.IsNullOrEmpty(new Uri(url).Scheme))
                {
                    //It already has a scheme
                    return url;
                }
                else
                {
                    return new UriBuilder(url).Scheme + "://" + url;
                }
            }
            catch (Exception e)
            {
                return new UriBuilder(url).Scheme + "://" + url; ;
            }
        }


        public static string CombinePaths(string path1, string path2)
        {
            if(path1.EndsWith("/") && path2.StartsWith("/"))
            {
                //strip the first / so they aren't doubled up when we combine them.
                path1 = path1.TrimEnd('/');
            }
            else if(!path1.EndsWith("/") && !path2.StartsWith("/"))
            {
                //neight of them had a / so we have to add one. 
                path1 = path1 + "/";
            }

            return path1 + path2;
        }
    }
}