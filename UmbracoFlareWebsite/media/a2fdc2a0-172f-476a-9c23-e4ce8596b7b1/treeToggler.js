angular.module('umbraco').controller('demo.toggler', function ($scope, appState, eventsService) {
    //Create an Angular JS controller and inject Umbraco services appState & eventsService

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

});