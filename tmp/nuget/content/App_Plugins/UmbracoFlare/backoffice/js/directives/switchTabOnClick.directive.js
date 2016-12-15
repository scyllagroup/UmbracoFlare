
angular.module('umbraco.directives').directive("switchTabOnClick", function () {

    function link(scope, element, attrs) {

        element.on('click', {tabId: attrs.tabIndex}, function (e) {
            //get the target tab
            $($(".nav-tabs li")[e.data.tabId]).find('a').click();
            }); 
    }
    return {
        restrict: "A",
        link: link
    }
});



