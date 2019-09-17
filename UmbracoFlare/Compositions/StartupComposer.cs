using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;
using UmbracoFlare.Components;
using UmbracoFlare.Configuration;
using UmbracoFlare.ImageCropperHelpers;
using UmbracoFlare.Manager;
using UmbracoFlare.Services;

namespace UmbracoFlare.Compositions
{
    public class StartupComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            //upgrade to tls1.2, this is not optional as cloudflare rejects calls made with deprecated tls versions
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //all the DI bits
            composition.Register<ICloudflareConfiguration, XmlCloudflareConfiguration>();
            composition.Register<IUmbracoFlareDomainManager, UmbracoFlareDomainManager>();
            composition.Register<ICloudflareManager, CloudflareManager>();
            composition.Register<IUrlWildCardManager, RuntimeCacheUrlWildCardManager>();
            composition.Register<ICloudflareService, CloudflareService>();
            composition.Register<IImageCropperCacheManager, ImageCropperCacheManager>();
            composition.Register<IImageCropperManager, ImageCropperManager>();

            //add our new components
           composition.Components()
                .Append<ContentPublishingComponent>() 
                .Append<FileSavingComponent>()
                .Append<MediaSavingComponent>()
                .Append<DataTypeSavingComponent>()
                .Append<TreeMenuComponent>();

        }
    }
}
