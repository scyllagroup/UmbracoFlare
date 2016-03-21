angular.module("umbraco").controller("Cloudflare.Configuration.Controller",
	function ($scope, $timeout, cloudflareResource, notificationsService, navigationService, modals) {
	    $scope.showConfigButtonText = "Config";

	    //$scope.config = { PurgeCacheOn: true };
	    $scope.oldConfig = null; //A holder to save the config in before updates, that way if the updates fail, we have a copy to revert back to.
	    $scope.skipConfigUpdate = true; //We have a watch on $scope.config. Don't update it when we get the config from the server for the first time.
	    $scope.uiConfig = {};

        /*
	    //Keep track of the list of domains available to use on cloudflare
	    $scope.domains = [];
	    $scope.activeDomain = "";
	    $scope.staticFileActiveDomain = "";

	    //Add some watches to watch the domains so we can update the available domain list.
	    $scope.$watch('config.AdditionalUrls', function () {
	        buildDomainList();
	    }, true);

	    $scope.$watch('umbracoDomains', function () {
	        buildDomainList();
	    });

	    //Build a list of available domains for cloudflare dashboard
	    function buildDomainList() {
	        $scope.domains = []; //Clear it out.

	        if ($scope.config !== undefined) {
	            angular.forEach($scope.config.AdditionalUrls, function (value, key) {
	                //make sure it doesnt exist
	                if ($scope.domains.indexOf(value) < 0) {
	                    $scope.domains.push(value);
	                }
	            });
	        }

	        if ($scope.umbracoDomains !== undefined) {
	            angular.forEach($scope.umbracoDomains, function (value, key) {
	                if ($scope.domains.indexOf(value) < 0) {
	                    $scope.domains.push(value);
	                }
	            });
	        }
	    }
        */
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

	    //Get all of the domains registered with umbraco
	    cloudflareResource.getAllowedDomains().success(function (domains) {
	        //domains = JSON.parse(domains);
	        $scope.umbracoDomains = domains;
	    });

	    $scope.openModal = function (type) {
            var promise = modals.open(type)
	    }


	    $scope.UpdateCredentials = function () {
	        $scope.syncUiConfigApiKey();
	        $scope.syncUiConfigAccountEmail();

	        //The config changed, tell the server.
	        cloudflareResource.updateConfigurationStatus($scope.config).success(function (returnedConfig) {
	            if (returnedConfig === null || returnedConfig === undefined) {
	                notificationsService.error("We could not update the configuration.");
	            } else if (!returnedConfig.CredentialsAreValid) {
	                notificationsService.error("We could not validate your credentials.");
	                $scope.config.CredentialsAreValid = false;
	                $scope.uiConfig = $scope.config;
	            } else {
	                notificationsService.success("Successfully updated your configuration!");
	                $scope.config = returnedConfig;
	                $scope.uiConfig = returnedConfig;
	                $scope.oldConfig = angular.copy(returnedConfig);
	            }
	        });
	    };

	    $scope.showModal = function () {
            $modal
	    }


	    $scope.togglePurgeCacheOn = function () {
	        $scope.config.PurgeCacheOn = !$scope.config.PurgeCacheOn;
	        $scope.UpdateCredentials();
	    };

	    //Takes the value from the UI and syncs it to the config which will trigger a server call to update it.
	    $scope.syncUiConfigApiKey = function () {
	        //we only care about changing it if it has been changed
	        if ($scope.uiConfig.ApiKey != $scope.config.ApiKey) {
	            $scope.config.ApiKey = $scope.uiConfig.ApiKey;
	        }
	    }

	    //Takes the value from the UI and syncs it to the config which will trigger a server call to update it.
	    $scope.syncUiConfigAccountEmail = function () {
	        if ($scope.config.AccountEmail != $scope.uiConfig.AccountEmail) {
	            $scope.config.AccountEmail = $scope.uiConfig.AccountEmail;
	        }
	    }
	});