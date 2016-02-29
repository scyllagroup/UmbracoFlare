# UmbracoFlare
This project aims to provide a basic level of service integration between Cloudflare and the open-source ASP.NET CMS Umbraco.

Here is the package on our umbraco. https://our.umbraco.org/projects/backoffice-extensions/umbracoflare/

###Please note: This is still in Beta so use at your own risk on production/live sites! 

#What does it do? 

##Abilities of UmbracoFlare
  - Purge Cloudflare on content published event.
  - Purge Cloudflare on media saved event.
  - Purge Cloudflare from content context menu.
  - Purge everything on Cloudflare.
  - Purge certain urls on Cloudflare.
  - Purge static files on Cloudflare.

###Content Published Event
Everytime content is published, UmbracoFlare will check to see if UmbracoFlare is on (in the config section). 
If UmbracoFlare is on, we will check for the property with the alias of "cloudflareDisabledOnPublish", if this property does NOT exist, it will be treated as if the checkbox is not checked, and the cache will be purged. If this property exists, and is checked, we will ignore this piece of content and will NOT purge cloudflare for this piece of content. The scheme used to generate the url for the piece of content is `umbraco.library.NiceUrlWithDomain(content.ID))`. [The relevant code to the Content Published event can be found here.](../master/UmbracoFlare/App_Start/SetCloudflareHooks.cs)

###Media Saved
Everytime media is saved, UmbracoFlare will check to see if UmbracoFlare is on (in the config section). 
If UmbracoFlare is on, we will check for the property with the alias of "cloudflareDisabledOnPublish", if this property does NOT exist, it will be treated as if the checkbox is not checked, and the cache will be purged. If this property exists, and is checked, we will ignore this media and will NOT purge cloudflare for it. It is important to note that UmbracoFlare will get every crop (if you use Image Cropper) and purge the media url for each of those crops (using `(IPublishedContent)publishedMedia.GetCropUrl(crop.alias)`). The scheme used to generate the url for the piece of content is `(IPublishedContent)publishedMedia.Url` as well as the crop urls described in the previous sentence. [The relevant code to the Media Saved event can be found here.](../master/UmbracoFlare/App_Start/SetCloudflareHooks.cs)

###Purge from content context menu
If you right click a piece of content, you can purge the Cloudflare cache for that single piece of content (child nodes are supported). Like the content published event, we will check to make sure UmbracoFlare is on and will check for the property "cloudflareDisabledOnPublish".

###Purge everything on cloudflare
In the UmbracoFlare section, you can purge everything for the current domain you are on. 

###Purge by url 
In the UmbracoFlare section, you can purge individual urls. You may also include wildcards.

###Purge static files
In the UmbracoFlare section, you can select static files to purge.

##Setup

### Setup Permissions
The first thing you will want to do if you don't see the UmbracoFlare section on the left hand side of umbraco is make sure that your 
user has permission to see it. To do this, go to the Users section, select your user from the tree, and then tick the checkbox for 
"UmbracoFlare". Refresh your browser.

###cloudflare.config
The credentials to hookup to CloudFlare can either be changed in the /config/cloudflare.config or through the backoffice under the UmbracoFlare section.

###Cloudflare rules
If you find that cloudflare caching is not working with your site. Use redbot.org (or a similar tool) to determine if your server is sending your cache control header as "private". If so, you will want to update your page rules in the Cloudflare dashboard (not UmbracoFlare) to "purge everything" in the dropdown. 
