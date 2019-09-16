using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace UmbracoFlare.ImageCropperHelpers
{
    public class ImageCropperCacheManager: IImageCropperCacheManager
    {

        public ImageCropperCacheManager()
        {

        }

        public T GetFromCache<T>(string cacheKey)
        {
            
            if(HttpRuntime.Cache[cacheKey] == null)
            {
                return default(T);
            }
            else
            {
                return (T)HttpRuntime.Cache[cacheKey];
            }
        }

        public bool UpdateCache(string key, object value)
        {
            try
            {
                if(HttpRuntime.Cache[key] == null)
                {
                    HttpRuntime.Cache.Add(key, value, null, DateTime.Now.AddDays(1), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
                }
                else
                {
                    HttpRuntime.Cache[key] = value;
                }
                
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

    }

    public static class ImageCropperCacheKeys
    {
        public static string AllCrops = "allCrops";
        public static string ImageCropperDataTypes = "imageCropperDataTypes";
    }
}
