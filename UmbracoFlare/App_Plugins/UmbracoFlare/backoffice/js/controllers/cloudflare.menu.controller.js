angular.module("umbraco").controller("Cloudflare.Menu.Controller",
    function ($scope, eventsService, cloudflareResource, navigationService, appState, treeService, localizationService) {


        $scope.busy = false;
        $scope.success = false;
        $scope.error = false;

        var nodeId = $scope.$id;

        $scope.purge = function () {
            $scope.busy = true;
            cloudflareResource.purgeCacheForNodeId(nodeId, $scope.purgeChildren).then(function (statusWithMessage) {
                    //statusWithMessage = JSON.parse(statusWithMessage);
                    $scope.busy = false;
                    if (statusWithMessage.Success) {
                        $scope.error = false;
                        $scope.success = true;
                    } else {
                        $scope.error = true;
                        $scope.success = false;
                        $scope.errorMsg = statusWithMessage.Message === undefined ? "We are sorry, we could not clear the cache at this time." : statusWithMessage.Message;
                    }
                }
                , function (e) {
                    $scope.busy = false;
                    $scope.success = false;
                    $scope.error = true;
                    $scope.errorMessage = "We are sorry, we could not clear the cache at this time.";
                });
        };
    });