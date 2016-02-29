
angular.module('umbraco.directives').directive("inputAdder", function () {

	function link(scope, element, attrs) {

		scope.activeInput = "";

		scope.add = function (item) {
		    if (scope.collection.indexOf(item)== -1) {
		        scope.collection.push(item);
		        scope.activeInput = "";
		    } else {
		        alert("This url is already in the list.");
		    }
		}

		scope.remove = function(item) {
			var index = scope.collection.indexOf(item);
			if (index > -1) {
				scope.collection.splice(index, 1);
			}
		}

		
	}

	return {
		restrict: "E",
		scope:{
			collection: '=ngModel',
			submit: '&onSubmit',
			submitText: '=submitText'
		},
		templateUrl: "/App_Plugins/UmbracoFlare/backoffice/directiveViews/inputAdder.html",
		link : link
	}
});



   