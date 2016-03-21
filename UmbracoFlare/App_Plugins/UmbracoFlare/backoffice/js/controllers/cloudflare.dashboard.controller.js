angular.module("umbraco").controller("Cloudflare.Dashboard.Controller",
	function ($scope, $timeout, cloudflareResource, notificationsService, navigationService, appState, eventsService, dialogService, modals) {

	    $scope.oldConfig = null; //A holder to save the config in before updates, that way if the updates fail, we have a copy to revert back to.
	    $scope.skipConfigUpdate = true; //We have a watch on $scope.config. Don't update it when we get the config from the server for the first time.
	    $scope.uiConfig = {};

	    

	    $scope.state = '';

	    //Get the configuration status of cloudflare
	    cloudflareResource.getConfigurationStatus().success(function (config) {
	        $scope.config = config;
	        $scope.oldConfig = angular.copy(config); //Save a copy so we have somethint to revert back to if the configuration update fails.

	        $scope.uiConfig.ApiKey = $scope.config.ApiKey;
	        $scope.uiConfig.AccountEmail = $scope.config.AccountEmail;


	        //Put this in a timout so the watch cycle
	        $timeout(function () {
	            $scope.skipConfigUpdate = false; //Any changes to $scope.config from here should be updated on the server.
	        }, 0);
	    });

	    $scope.openModal = function (type) {
	        return modals.open(type)
	    }


	    $scope.GoToCredentials = function () {
	        var somethinhg = 1;
	    };

	    $scope.urls = [];
	    $scope.purgeUrlsButtonText = "Purge Urls";
	    
	    //Button Click - ToggleUmbracoNavigation
	    $scope.toggleUmbracoNavigation = function () {

	        //Get the current state of showNavigation
	        var currentNavigationState = appState.getGlobalState('showNavigation');

	        //console.log("currentNavigationState", currentNavigationState);
	        //console.log("Inverse of currentNavigationState", !currentNavigationState);

	        //Toggle the tree visibility
	        appState.setGlobalState("showNavigation", !currentNavigationState);
	    }


	    //The eventService allows us to easily listen for any events that the Umbraco applciation fires
	    //Let's listen for globalState changes...
	    eventsService.on("appState.globalState.changed", function (e, args) {
	        //console.log("appState.globalState.changed (args)", args);

	        if (args.key === "showNavigation") {
	            //console.log("showNavigation value", args.key, args.value);

	            //If false (So hiding navigation)
	            if (!args.value) {
	                //Set css left position to 80px (width of appBar)
	                document.getElementById("contentwrapper").style.left = "80px";
	            }
	            else {
	                //Remove the CSS we set so default CSS of Umbraco kicks in
	                document.getElementById("contentwrapper").style.left = "";
	            }
	        }
	    });

	  
	    $scope.selectedFiles = [];

	    $scope.purgeStaticFiles = function (selectedFiles) {
	        openDomainDialog(function (domains) {
	            $scope.state = 'purge-static-busy';
	            cloudflareResource.purgeStaticFiles(selectedFiles, domains).success(function (statusWithMessage) {
	                if (statusWithMessage.Success) {
	                    $scope.state = "purge-static-success";
	                    notificationsService.success(statusWithMessage.Message);
	                } else {
	                    $scope.state = "purge-static-error";
	                    notificationsService.error(statusWithMessage.Message, "");
	                }

	                refreshStateAfterTime();

	            }).error(function (e) {
	                notificationsService.error("Sorry, we could not purge the cache for the selected static files.", "");

	                $scope.state = "purge-static-error";

	                refreshStateAfterTime();
	            });
	        })
	    };

	    var refreshStateAfterTime = function() {
	        $timeout(function () {
	            $scope.state = "";
	        }, 5000);
	    }


	    $scope.purgeUrls = function (urls, domains) {

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

	        openDomainDialog(function (domains) {
	            $scope.state = 'purge-urls-busy';
	            cloudflareResource.purgeCacheForUrls(urls, domains).success(function (statusWithMessage) {

	                if (statusWithMessage.Success) {
	                    notificationsService.success(statusWithMessage.Message, "");
	                    $scope.state = 'purge-urls-success';
	                } else {
	                    //Build the error
	                    notificationsService.error(statusWithMessage.Message, "");
	                    $scope.state = 'purge-urls-error';
	                }
	                refreshStateAfterTime();
	            }).error(function (e) {
	                notificationsService.error("Sorry, we could not purge the cache for the given urls.", "");
	                $scope.state = 'purge-urls-error';
	                refreshStateAfterTime();
	            });
	        })
	        
	    };

	    $scope.purgeEverything = function () {

	        var confirmPromise = $scope.openModal("confirmModal");

	        confirmPromise.then(function () {
	            cloudflareResource.purgeAll().success(function (statusWithMessage) {
	                if (statusWithMessage.Success) {
	                    notificationsService.success("Purged Cache Successfully!", "");
	                } else {
	                    notificationsService.error(statusWithMessage.Message, "");
	                }
	            }).error(function (e) {
	                notificationsService.error("Sorry, we could not purge the cache, please check the error logs for details.", "");
	            });
	        });
	   }


	    $scope.toggleUmbracoNavigation();
	    

	    var openDomainDialog = function (callback) {
	        domainDialog = dialogService.open(
                {
                    template: "/App_Plugins/UmbracoFlare/backoffice/dashboardViews/domainDialog.html",
                    callback: callback
                });
	    }
	});

