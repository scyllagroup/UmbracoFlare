# UmbracoFlare
This project aims to provide a basic level of service integration between Cloudflare and the open-source ASP.NET CMS Umbraco.

#What does it do? 
  ##Umbraco Save and Publish Events
  UmbracoFlare hooks into the Content Published, & Media Saved events. 

    ###Content Published Event
    Everytime you publish a piece of content, UmbracoFlare will check to see if UmbracoFlare is switched to on (in the config section). 
    If UmbracoFlare is on,


##Setup

### Setup Permissions
The first thing you will want to do if you don't see the UmbracoFlare section on the left hand side of umbraco is make sure that your 
user has permission to see it. To do this, go to the Users section, select your user from the tree, and then tick the checkbox for 
"UmbracoFlare". Refresh your browser.

###cloudflare.config
The credentials to hookup 
