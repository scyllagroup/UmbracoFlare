
angular.module('umbraco.directives').directive("fsPicker", function () {

	function link(scope, element, attrs) {
	}

	return {
	    controller: "Umbraco.FileSystemPickerController",
		restrict: "E",
		scope: {
            selectedFiles: '='
		},
		templateUrl: "/App_Plugins/UmbracoFlare/backoffice/directiveViews/filesystem-picker.html",
		link: link
	}
});