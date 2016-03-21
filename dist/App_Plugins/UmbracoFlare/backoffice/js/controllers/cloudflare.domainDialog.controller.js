angular.module("umbraco").controller("Cloudflare.DomainDialog.Controller",
	function ($scope, $timeout, cloudflareResource, notificationsService, dialogService) {

	    //Get a list of the umbracodomains
	    cloudflareResource.getAllowedDomains().success(function (domains) {
	        $scope.allowedDomains = domains;
	    }).error(function (e) {
	        notificationsService.error("There was an error getting the umbraco domains.", "");
	    });

	    $scope.selectDeselectAllText = "Select All";
	    $scope.checkboxWrapper = {allSelected:false};
	    $scope.selectedDomains = [];
	    $scope.ignoreSelectedDomainsWatch = true;
	    $scope.ignoreAllSelectedWatch = true;
	    $scope.$watch(
            'selectedDomains',
            function selectedDomainsChanged(newValue, oldValue) {
                if ($scope.ignoreSelectedDomainsWatch) {
                    $scope.ignoreSelectedDomainsWatch = false;
                    return;
                }
                //See if all of them are selected.
                if ($scope.allowedDomains != undefined && $scope.allowedDomains.length == $scope.selectedDomains.length) {
                    if (!$scope.checkboxWrapper.allSelected) {
                        $scope.ignoreAllSelectedWatch = true;
                        $scope.checkboxWrapper.allSelected = true;
                    }
                } else {
                    if ($scope.checkboxWrapper.allSelected) {
                        $scope.ignoreAllSelectedWatch = true;
                        $scope.checkboxWrapper.allSelected = false;
                    }
                }
            }, true);

	    $scope.$watch(
            'checkboxWrapper.allSelected',
            function allSelectedChanged(newValue, oldValue) {
                if ($scope.ignoreAllSelectedWatch) {
                    $scope.ignoreAllSelectedWatch = false;
                    return;
                }

                var shouldBeSelected = true;
                if (!newValue) {
                    shouldBeSelected = false;
                }

                angular.forEach($scope.allowedDomains, function (domain, index) {
                    var index = $scope.selectedDomains.indexOf(domain);
                    if (index >= 0 && !shouldBeSelected) { //It is in the array and should NOT be selected, remove it.
                        $scope.selectedDomains.splice(index, 1);
                    } else if(index == -1 && shouldBeSelected) { //It is not in the array and it should be.
                        $scope.selectedDomains.push(domain);
                    }
                });
                $scope.ignoreSelectedDomainsWatch = true;
            });

	    $scope.addDomains = function () {
	        if ($scope.selectedDomains.length > 0) {
	            $scope.submit($scope.selectedDomains);

	            $scope.close();
	        }
	    };

	    $scope.close = function () {
	        dialogService.close(domainDialog);
	    }

	    $scope.toggleSelected = function (domain) {
	        var index = $scope.selectedDomains.indexOf(domain);
	        if (index >= 0) {
	            $scope.selectedDomains.splice(index,1);
	        } else {
	            $scope.selectedDomains.push(domain);
	        }
	    }

	    $scope.isChecked = function(domain)
	    {
	        return $scope.selectedDomains.indexOf(domain) > -1;
	    }
	});