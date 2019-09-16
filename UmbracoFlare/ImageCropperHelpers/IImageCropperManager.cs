using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using UmbracoFlare.Models.CropModels;

namespace UmbracoFlare.ImageCropperHelpers
{
    public interface IImageCropperManager
    {
        IEnumerable<Crop> GetAllCrops(bool bypassCache = false);
        IEnumerable<IDataType> GetImageCropperDataTypes(bool bypassCache = false);
    }
}
