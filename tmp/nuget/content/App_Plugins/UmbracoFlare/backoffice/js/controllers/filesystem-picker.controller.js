var fileSystemPickerTreeDialog = null;
var fileSytemPickerUploadDialog = null;


function fileSystemPickerController($scope, dialogService) {

    $scope.model =
        {
            value: "",
            config:
                {
                    folder: "",
                    filter: "",
                    thumbnailSize: 100,
                    coordinatePicker: null
                }
        };

    $scope.openPicker = function () {
        fileSystemPickerTreeDialog = dialogService.open({
            template: "/App_Plugins/UmbracoFlare/backoffice/directiveViews/filesystem-picker-dialog.html",
            callback: populate,
            dialogData: {
                filter: $scope.model.config.filter,
                folder: $scope.model.config.folder
            }
        });
    };


    $scope.remove = function () {
        $scope.model.value = "";
    };

    function populate(data) {
        $scope.model.value = data.toString();
        $scope.selectedFiles = data;
    };
    
};
angular.module("umbraco").controller("Umbraco.FileSystemPickerController", fileSystemPickerController);







//FILE SYSTEM PICKER DIALOG CONTROLLER
function fileSystemPickerDialogController($scope, $log, dialogService) {

    $scope.dialogEventHandler = $({});
    $scope.dialogEventHandler.bind("treeNodeSelect", nodeSelectHandler);
    //$scope.dialogEventHandler.bind("treeOptionsClick", optionsHandler);

    $scope.model = {
        selectedValues: []
    };


    function nodeSelectHandler(ev, args) {
        args.event.preventDefault();
        args.event.stopPropagation();
        
        var targetDiv = args.element.children("div").first();
        var path = BuildFullPath(args.node, "");

        var indexOfPath = $scope.model.selectedValues.indexOf(path);

        if (targetDiv.hasClass("umb-tree-node-checked")) {
            //it was already checked, uncheck it and remove it from the array
            targetDiv.removeClass("umb-tree-node-checked");

            //make sure we are in bounds
            if ($scope.model.selectedValues.length - 1 >= indexOfPath) {
                $scope.model.selectedValues.splice(indexOfPath, 1);
            }
        } else {
            targetDiv.addClass("umb-tree-node-checked");
            $scope.model.selectedValues.push(path);
        } 
    };

    function BuildFullPath(node, path){
        if (node.parentId == null) {
            //we have made it to the top, return 
            return path;
        }

        path = "/" + node.name + path;

        return BuildFullPath(node.parent(), path);
    }

    $scope.purgeStaticFiles = function () {
        alert("Gonna purge static files");
    }

    $scope.selectFiles = function () {
        dialogService.close(fileSystemPickerTreeDialog);

        //Crazy how the dialog service works. If you submit, the value is passed back to the callback function that was defined 
        //when you opened the modal.
        $scope.submit($scope.model.selectedValues);
    }

    $scope.dialogEventHandler.bind("treeNodeExpanded", nodeExpandedHandler);
    $scope.dialogEventHandler.bind("treeLoaded", treeLoadedHandler);

    function nodeExpandedHandler(ev, args) { };
    function treeLoadedHandler(ev, args) { };
};
angular.module("umbraco").controller("Umbraco.FileSystemPickerDialogController", fileSystemPickerDialogController);

