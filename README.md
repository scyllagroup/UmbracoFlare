# UmbracoFlare
This project aims to provide a basic level of service integration between Cloudflare and the open-source ASP.NET CMS Umbraco.

###Please note: This is still an APLHA so use at your own risk on production/live sites! 

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
If UmbracoFlare is on, we will check for the property with the alias of "cloudflareDisabledOnPublish", if this property does NOT exist, it will be treated as if the checkbox is not checked, and the cache will be purged. If this property exists, and is checked, we will ignore this piece of content and will NOT purge cloudflare for this piece of content. The scheme used to generate the url for the piece of content is `umbraco.lobrary.NiceUrlWithDomain(content.ID))`. [The relevant code to the Content Published event can be found here.](../blob/master/LICENSE]

###Media Saved
Everytime media is saved, UmbracoFlare will check to see if UmbracoFlare is on (in the config section). 
If UmbracoFlare is on, we will check for the property with the alias of "cloudflareDisabledOnPublish", if this property does NOT exist, it will be treated as if the checkbox is not checked, and the cache will be purged. If this property exists, and is checked, we will ignore this media and will NOT purge cloudflare for it. It is important to note that UmbracoFlare will get every crop (if you use Image Cropper) and purge the media url for each of those crops (using `(IPublishedContent)publishedMedia.GetCropUrl(crop.alias)`). The scheme used to generate the url for the piece of content is `(IPublishedContent)publishedMedia.Url` as well as the crop urls described in the previous sentence. [The relevant code to the Media Saved event can be found here.](../blob/master/LICENSE]



##Setup

### Setup Permissions
The first thing you will want to do if you don't see the UmbracoFlare section on the left hand side of umbraco is make sure that your 
user has permission to see it. To do this, go to the Users section, select your user from the tree, and then tick the checkbox for 
"UmbracoFlare". Refresh your browser.

###cloudflare.config
The credentials to hookup 
