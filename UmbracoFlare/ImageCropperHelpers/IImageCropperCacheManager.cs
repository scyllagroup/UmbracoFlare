using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoFlare.ImageCropperHelpers
{
    public interface IImageCropperCacheManager
    {
        T GetFromCache<T>(string cacheKey);
        bool UpdateCache(string key, object value);
    }
}
