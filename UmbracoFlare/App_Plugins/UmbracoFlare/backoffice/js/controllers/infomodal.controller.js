angular.module("umbraco").controller(
            "InfoModalController",
            function ($scope, modals) {

                // ---
                // PUBLIC METHODS.
                // ---
                // Wire the modal buttons into modal resolution actions.
                $scope.close = modals.resolve;
            }
        );