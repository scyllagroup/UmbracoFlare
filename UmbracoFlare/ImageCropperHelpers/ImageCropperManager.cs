using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using UmbracoFlare.Models.CropModels;

namespace UmbracoFlare.ImageCropperHelpers
{
    public class ImageCropperManager : IImageCropperManager
    {
        private readonly IDataTypeService dataTypeService;
        private readonly IImageCropperCacheManager cacheManager;

        public ImageCropperManager(IDataTypeService dataTypeService, IImageCropperCacheManager cacheManager)
        {
            this.dataTypeService = dataTypeService;
            this.cacheManager = cacheManager;
        }
        public IEnumerable<Crop> GetAllCrops(bool bypassCache = false)
        {
            List<Crop> allCrops = new List<Crop>();


            if (bypassCache)
            {
                allCrops = GetAllCropsFromDataTypeServiceAndUpdateCache();   
            }
            else
            {
                allCrops = cacheManager.GetFromCache<List<Crop>>(ImageCropperCacheKeys.AllCrops);

                if(allCrops == null)
                {
                    allCrops = GetAllCropsFromDataTypeServiceAndUpdateCache();
                }
            }
            return allCrops;
        }


        private List<Crop> GetAllCropsFromDataTypeServiceAndUpdateCache()
        {
            List<Crop> allCrops = new List<Crop>();

            //We are bypassing the cache so we have to get everything again from the db. 
            IEnumerable<IDataType> imageCropperDataTypes = GetImageCropperDataTypes(true);

            foreach (IDataType dataType in imageCropperDataTypes)
            {
                ImageCropperConfiguration valueList = (ImageCropperConfiguration)dataType.Configuration;
                var crops = valueList.Crops ?? Enumerable.Empty<ImageCropperConfiguration.Crop>();

                if (crops.Any())
                {
                    IEnumerable<Crop> cropsFromDb = crops.Select(x => new Crop()
                    {
                        alias = x.Alias,
                        width = x.Width,
                        height = x.Height
                    });

                    allCrops.AddRange(cropsFromDb);
                }

            }

            cacheManager.UpdateCache(ImageCropperCacheKeys.AllCrops, allCrops);

            return allCrops;
        }



        public IEnumerable<IDataType> GetImageCropperDataTypes(bool bypassCache = false)
        {
            IEnumerable<IDataType> imageCropperDataTypes;
            if(bypassCache)
            {
                imageCropperDataTypes = dataTypeService.GetByEditorAlias("Umbraco.ImageCropper");
                cacheManager.UpdateCache(ImageCropperCacheKeys.ImageCropperDataTypes, imageCropperDataTypes);

                return imageCropperDataTypes;
            }
            else
            {
                imageCropperDataTypes = cacheManager.GetFromCache<IEnumerable<IDataType>>(ImageCropperCacheKeys.ImageCropperDataTypes);

                if(imageCropperDataTypes == null)
                {
                    imageCropperDataTypes = dataTypeService.GetByEditorAlias("Umbraco.ImageCropper");
                    cacheManager.UpdateCache(ImageCropperCacheKeys.ImageCropperDataTypes, imageCropperDataTypes);
                }
                return imageCropperDataTypes;
            }

        }
    }
}
