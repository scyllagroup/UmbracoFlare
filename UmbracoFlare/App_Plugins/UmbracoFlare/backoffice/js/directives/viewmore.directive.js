'use strict';
angular.module('umbraco.directives').directive('viewMore', function ($timeout, $window, $compile) {
    return {
        restrict: 'A',
        transclude:true,
        scope:{
        },
        templateUrl: '/App_Plugins/UmbracoFlare/backoffice/directiveViews/viewmore.html',

        link: function (scope, elm, attrs) {

            //Get the scroll height of the div that we have inserted the content into through the ng-transclude directive.
            scope.originalHeight;

            scope.clickElement = $("#"+attrs.clickElementId);
            scope.belowTheFoldElement = $("#"+attrs.belowTheFoldElementId);
            scope.belowTheFoldElement.css("overflow", "hidden");
            scope.clickElement.attr("ng-click", "toggleShowMore()");

            scope.contentHidden = false;
            
            $compile(scope.clickElement)(scope);

            //scope.belowTheFoldElement.height(0);

            var w = angular.element($window);

          

            /** showMore - Called on click of 'View more' link **/
            scope.toggleShowMore = function () {
               
                if (!scope.contentHidden) {
                    scope.belowTheFoldElement.animate({
                        'height': 0 + "px"
                    });
                    scope.contentHidden = true; //Make sure we flip the flag to indicate that the content is hidden again
                } else {
                    
                    scope.belowTheFoldElement.animate({
                        'height': scope.belowTheFoldElement[0].scrollHeight + 'px'
                    });
                    scope.contentHidden = false;
                }

            };

          
        }
    };
});