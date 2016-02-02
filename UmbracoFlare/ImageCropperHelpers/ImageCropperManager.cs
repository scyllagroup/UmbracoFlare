using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using UmbracoFlare.Models.CropModels;

namespace UmbracoFlare.ImageCropperHelpers
{
    public class ImageCropperManager
    {
        private static ImageCropperManager _instance;

        private ImageCropperManager()
        {

        }

        public static ImageCropperManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new ImageCropperManager();
                }
                return _instance;
            }
        }


        public List<Crop> GetAllCrops(bool bypassCache = false)
        {
            List<Crop> allCrops = new List<Crop>();


            if (bypassCache)
            {
                allCrops = GetAllCropsFromDataTypeServiceAndUpdateCache();   
            }
            else
            {
                allCrops = ImageCropperCacheManager.Instance.GetFromCache<List<Crop>>(ImageCropperCacheKeys.AllCrops);

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
            IEnumerable<IDataTypeDefinition> imageCropperDataTypes = GetImageCropperDataTypes(true);

            foreach (IDataTypeDefinition dataType in imageCropperDataTypes)
            {
                IEnumerable<string> cropsStr = ApplicationContext.Current.Services.DataTypeService.GetPreValuesByDataTypeId(dataType.Id);

                if (cropsStr == null || !cropsStr.Any())
                {
                    continue;
                }

                IEnumerable<Crop> cropsFromDb = JsonConvert.DeserializeObject<IEnumerable<Crop>>(cropsStr.First());

                allCrops.AddRange(cropsFromDb);
            }

            ImageCropperCacheManager.Instance.UpdateCache(ImageCropperCacheKeys.AllCrops, allCrops);

            return allCrops;
        }



        public IEnumerable<IDataTypeDefinition> GetImageCropperDataTypes(bool bypassCache = false)
        {
            IEnumerable<IDataTypeDefinition> imageCropperDataTypes;
            if(bypassCache)
            {
                imageCropperDataTypes = ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionByPropertyEditorAlias("Umbraco.ImageCropper");
                ImageCropperCacheManager.Instance.UpdateCache(ImageCropperCacheKeys.ImageCropperDataTypes, imageCropperDataTypes);

                return imageCropperDataTypes;
            }
            else
            {
                imageCropperDataTypes = ImageCropperCacheManager.Instance.GetFromCache<IEnumerable<IDataTypeDefinition>>(ImageCropperCacheKeys.ImageCropperDataTypes);

                if(imageCropperDataTypes == null)
                {
                    imageCropperDataTypes = ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionByPropertyEditorAlias("Umbraco.ImageCropper");
                    ImageCropperCacheManager.Instance.UpdateCache(ImageCropperCacheKeys.ImageCropperDataTypes, imageCropperDataTypes);
                }
                return imageCropperDataTypes;
            }

        }
    }
}
