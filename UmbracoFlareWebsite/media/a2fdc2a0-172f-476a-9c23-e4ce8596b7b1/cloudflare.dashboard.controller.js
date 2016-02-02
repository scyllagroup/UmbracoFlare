angular.module("umbraco").controller("Cloudflare.Dashboard.Controller",
	function ($scope, $timeout, cloudflareResource, notificationsService, navigationService) {

	    $scope.urls = [];
	    $scope.purgeUrlsButtonText = "Purge Urls";
	    

	  
	    $scope.selectedFiles = [];

	    $scope.purgeStaticFiles = function (selectedFiles) {
	        cloudflareResource.purgeStaticFiles(selectedFiles).success(function (statusWithMessage) {
	            if(statusWithMessage.Success){
	                notificationsService.success(statusWithMessage.Message);
	            } else {
	                notificationsService.error(statusWithMessage.Message, "");
	            }
	        }).error(function (e) {
	            notificationsService.error("Sorry, we could not purge the cache for the selected static files.", "");
	        });
	    };

	    $scope.purgeUrls = function (urls) {

	        var noBeginningSlash = false;

	        //Make sure they have http:// or https:// included in their urls
	        angular.forEach(urls, function (value, key) {
	            //if (value.indexOf("http://") == -1 && value.indexOf("https://") == -1) {
	            //    noHttp = true;
	            //}
	            if (value.indexOf("/") !== 0) {
	                noBeginningSlash = true;
	            }
	        });

	        if (noBeginningSlash) {
	            alert("Your urls must begin with /");
	            //alert("Your urls must contain http:// or https://");
	            return;
	        }

	        cloudflareResource.purgeCacheForUrls(urls).success(function (statusWithMessage) {
	            
	            if (statusWithMessage.Success) {
	                notificationsService.success("Purged Cache for urls Successfully!", "");
	            } else {
                    //Build the error
	                notificationsService.error(statusWithMessage.Message, "");
	            }
	        }).error(function (e) {
	            notificationsService.error("Sorry, we could not purge the cache for the given urls.", "");
	        });
	    };

	    $scope.purgeEverything = function () {
	        var theyAreSure = window.confirm("Are you sure you want to purge the entire site cache? The website may take a performance hit while the cache is rebuilt.");

	        if (theyAreSure) {
	            cloudflareResource.purgeAll().success(function (statusWithMessage) {
	                //statusWithMessage = JSON.parse(statusWithMessage);
	                if (statusWithMessage.Success) {
	                    notificationsService.success("Purged Cache Successfully!", "");
	                } else {
	                    notificationsService.error(statusWithMessage.Message, "");
	                }
	            }).error(function (e) {
	                notificationsService.error("Sorry, we could not purge the cache, please check the error logs for details.", "");
	            });
	        }
	    };

	    
	});

