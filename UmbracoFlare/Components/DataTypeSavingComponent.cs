using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using UmbracoFlare.ImageCropperHelpers;

namespace UmbracoFlare.Components
{
    public class DataTypeSavingComponent : IComponent
    {
        private readonly IImageCropperManager imageCropperManager;

        public DataTypeSavingComponent(IImageCropperManager imageCropperManager)
        {
            this.imageCropperManager = imageCropperManager;
        }

        public void Initialize()
        {
            DataTypeService.Saved += RefreshImageCropsCache;
        }

        public void Terminate()
        {
            DataTypeService.Saved -= RefreshImageCropsCache;
        }

        private void RefreshImageCropsCache(IDataTypeService sender, SaveEventArgs<IDataType> e)
        {
            //A data type has saved, see if it was a 
            IEnumerable<IDataType> imageCroppers = imageCropperManager.GetImageCropperDataTypes(true);
            IEnumerable<IDataType> freshlySavedImageCropper = imageCroppers.Intersect(e.SavedEntities);

            if (imageCroppers.Intersect(e.SavedEntities).Any())
            {
                //There were some freshly saved Image cropper data types so refresh the image crop cache.
                //We can do that by simply getting the crops
                imageCropperManager.GetAllCrops(true); //true to bypass the cache & refresh it.
            }
        }
    }
}
