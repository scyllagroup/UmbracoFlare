angular.module("umbraco").controller(
            "ConfirmModalController",
            function ($scope, modals) {

                // ---
                // PUBLIC METHODS.
                // ---
                // Wire the modal buttons into modal resolution actions.
                $scope.close = modals.reject;

                $scope.yes = modals.resolve;
            }
        );