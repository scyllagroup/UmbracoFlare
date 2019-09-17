angular.module('umbraco.directives').directive('ngBlur', function () {
    return {
        restrict: 'A',
        link: function postLink(scope, element, attrs) {
            element.bind('blur', function () {
                scope.$apply(attrs.ngBlur);
            });
        }
    };
});