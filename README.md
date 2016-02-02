# UmbracoFlare
This project aims to provide a basic level of service integration between Cloudflare and the open-source ASP.NET CMS Umbraco.

#What does it do? 

###Content Published Event
Everytime content is published, UmbracoFlare will check to see if UmbracoFlare is on (in the config section). 
If UmbracoFlare is on, we will check for the property with the alias of "cloudflareDisabledOnPublish", if this property does NOT exist, 
it will be treated as if the checkbox is not checked, and the cache will be purged. If this property exists, and is checked, we will ignore this piece of content and will NOT purge cloudflare for this piece of content. The scheme used to generate the url for the piece of content is 'umbraco.lobrary.NiceUrlWithDomain(content.ID))'. [The relevant code to the Content Published event can be found here.](../blob/master/LICENSE]


##Setup

### Setup Permissions
The first thing you will want to do if you don't see the UmbracoFlare section on the left hand side of umbraco is make sure that your 
user has permission to see it. To do this, go to the Users section, select your user from the tree, and then tick the checkbox for 
"UmbracoFlare". Refresh your browser.

###cloudflare.config
The credentials to hookup 
