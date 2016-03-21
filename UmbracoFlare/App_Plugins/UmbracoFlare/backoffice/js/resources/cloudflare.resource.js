angular.module('umbraco.resources').factory('cloudflareResource', function ($q, $http) {
    //the factory object returned
    return {
        //call the Cloudflare Umbraco APi Controller that we created on the backend under the controllers folder
        purgeCacheForUrls: function (urls, domains) {
            return $http({
                url: "backoffice/UmbracoFlare/CloudflareUmbracoApi/PurgeCacheForUrls",
                method: "POST",
                data: { Urls: urls, Domains: domains }
            });
        },

        purgeAll: function () {
            /*
            return $http({
                url: "backoffice/ScyllaPlugins/CloudflareUmbracoApi/PurgeAll",
                method: "POST",
                data: { '': domain }
            });*/
            return $http.post(
                "backoffice/UmbracoFlare/CloudflareUmbracoApi/PurgeAll"
                );
        },


        purgeStaticFiles: function(staticFiles, domains){
            return $http.post(
                "backoffice/UmbracoFlare/CloudflareUmbracoApi/PurgeStaticFiles",
                { StaticFiles: staticFiles, Hosts: domains }
            );
        },

        getConfigurationStatus: function () {
            return $http({
                url: "backoffice/UmbracoFlare/CloudflareUmbracoApi/GetConfig",
                method: "GET"
            });
        },

        purgeCacheForNodeId: function (nodeId, purgeChildren) {
            return $http({
                url: "backoffice/UmbracoFlare/CloudflareUmbracoApi/PurgeCacheForContentNode",
                method: "POST",
                data: { nodeId: nodeId, purgeChildren: purgeChildren }
            });
        },

        updateConfigurationStatus: function(on){
            return $http({
                url: "backoffice/UmbracoFlare/CloudflareUmbracoApi/UpdateConfigStatus",
                method: "POST",
                data: on
            });
        },

        getAllowedDomains: function () {
            return $http({
                url: "backoffice/UmbracoFlare/CloudflareUmbracoApi/GetAllowedDomains",
                method: "GET"
            });
        }
    }
});