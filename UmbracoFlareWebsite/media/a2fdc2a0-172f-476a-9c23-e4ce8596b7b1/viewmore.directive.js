'use strict';
angular.module('umbraco.directives').directive('viewMore', function ($timeout, $window, $compile) {
    return {
        restrict: 'A',
        transclude:true,
        scope:{
            showMoreText:'='
        },
        templateUrl: '/App_Plugins/UmbracoFlare/backoffice/directiveViews/viewmore.html',

        link: function (scope, elm, attrs) {

            //Get the scroll height of the div that we have inserted the content into through the ng-transclude directive.
            scope.originalHeight;
            scope.contentShowed = false;


            //Compile the show more / show less buttons and tell them what scope they should be using which is the local isolate scope.
            var buttons = $compile(
                '<button ng-hide="contentShowed" ng-click="showMore()">Show {{showMoreText}}</button>' +
                '<button ng-show="contentShowed" ng-click="showLess()">Hide {{showMoreText}}</button>')(scope);
            //Now append them to the dom.
            elm.append(buttons);

            var w = angular.element($window);

            w.bind('resize', function () {
                updateView();
            });


            var updateView = function () {
                $("#viewMore" + id).show();
                $("#viewLess" + id).hide();

                $timeout(viewMore, 0);
            }

            /** showMore - Called on click of 'View more' link **/
            scope.showMore = function () {
                var $viewMoreDiv = $(".view-more");
                var scrollHeight = $viewMoreDiv[0].scrollHeight;
                scope.originalHeight = $viewMoreDiv.height(); //set the current height so we know where to go back to when "show less"
                $viewMoreDiv.animate({
                    'height': scrollHeight + 'px'
                });

                scope.contentShowed = true;
            }

            /** showLess - Called on click of 'View less' link **/
            scope.showLess = function () {
                $(".view-more").animate({
                    'height': scope.originalHeight + "px"
                });
                scope.contentShowed = false; //Make sure we flip the flag to indicate that the content is hidden again
            }

            scope.syncUiConfigApiKey = function ($event) { alert("hey");}

            var updateView = function () {
                var id = scope.customdata.id;
                var $content = $('#viewMoreDataDiv' + id);

                $content.height('100px');
                $("#viewMore" + id).show();
                $("#viewLess" + id).hide();

                $timeout(viewMore, 0);
            }
        }
    };
});