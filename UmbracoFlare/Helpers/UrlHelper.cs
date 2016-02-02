using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace UmbracoFlare.Helpers
{
    public class UrlHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Takes the given url and returns the domain with the scheme (no path and query)
        /// ex. http://www.example.com/blah/blah?blah=blah will return http://www.example.com(/)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withTrailingSlash"></param>
        /// <returns></returns>
        public static string GetDomainWithScheme(string url, bool withTrailingSlash = false)
        {
            Uri uri;
            //first try parsing the Url
            try
            {
                uri = new Uri(url);
            }
            catch(Exception e)
            {
                return MakeFullUrlWithDomain(url);
            }

            return uri.Scheme + "://" + uri.Authority + (withTrailingSlash ? "/" : "");
        }


        public static string GetCurrentDomainWithScheme(bool withTrailingSlash = false)
        {
            string currentDomain = MakeFullUrlWithDomain("");

            if(withTrailingSlash)
            {
                if(currentDomain[currentDomain.Length -1] != '/')
                {
                    currentDomain = currentDomain + "/";
                }
            }

            return currentDomain;
        }

        /// <summary>
        /// Uses the HttpContext.Current to add on the scheme & host to the given url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string MakeFullUrlWithDomain(string url)
        {

            Uri uriWithDomain;
            try
            {
                uriWithDomain = new Uri(url);

                if (!String.IsNullOrEmpty(uriWithDomain.Host))
                {
                    //The host is not null, so its already a full url with the domain.
                    return url;
                }
            }
            catch
            { }

            //if we made it here we know that the host was not added to the url.
            try
            {
                if(HttpContext.Current == null || HttpContext.Current.Request == null)
                {
                    Log.Error(String.Format("HttpContext.Current or HttpContext.Current.Request is null."));
                }
                Uri root = new Uri(String.Format("{0}{1}{2}", HttpContext.Current.Request.Url.Scheme, Uri.SchemeDelimiter, HttpContext.Current.Request.Url.Host));
                uriWithDomain = new Uri(root, url);

                return uriWithDomain != null ? uriWithDomain.ToString() : url;
            }
            catch
            {
                Log.Error(String.Format("Could not create root uri using http context request url {0}", HttpContext.Current.Request.Url));
                return String.Empty;
            }
        }
    }
}