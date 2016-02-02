var fileSystemPickerTreeDialog = null;
var fileSytemPickerUploadDialog = null;

function folderSystemPickerController($scope, dialogService) {
    $scope.openPicker = function () {
        dialogService.open({
            template: "/App_Plugins/FileSystemPicker/foldersystem-picker-dialog.html",
            callback: populate
        });
    };
    function populate(data) {
        $scope.model.value = "/" + data;
    };

};
angular.module("umbraco").controller("Umbraco.FolderSystemPickerController", folderSystemPickerController);

function folderSystemPickerDialogController($scope, dialogService) {

    $scope.dialogEventHandler = $({});
    $scope.dialogEventHandler.bind("treeNodeSelect", nodeSelectHandler);

    function nodeSelectHandler(ev, args) {
        args.event.preventDefault();
        args.event.stopPropagation();
        $scope.submit(args.node.id);
    };
};
angular.module("umbraco").controller("Umbraco.FolderSystemPickerDialogController", folderSystemPickerDialogController);



function fileSystemPickerController($scope, dialogService) {

    $scope.openPicker = function () {
        fileSystemPickerTreeDialog = dialogService.open({
            template: "/App_Plugins/FileSystemPicker/filesystem-picker-dialog.html",
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
        $scope.model.value = $scope.model.config.folder + data;
    };
};
angular.module("umbraco").controller("Umbraco.FileSystemPickerController", fileSystemPickerController);

function fileSystemPickerDialogController($scope, $log, dialogService) {

    $scope.dialogEventHandler = $({});
    $scope.dialogEventHandler.bind("treeNodeSelect", nodeSelectHandler);
    $scope.dialogEventHandler.bind("treeOptionsClick", optionsHandler);

    $scope.onUploadComplete = function (data) {
        //$log.debug('Upload Complete: ', data);
        fileSytemPickerUploadDialog.close(data);
    };

    function uploadClosed(data) {
        //$log.debug('Upload Closed: ', data.node);
        if (data) {
            data.node.refresh();

        }
    };

    function optionsHandler(node, ev) {
        fileSytemPickerUploadDialog = dialogService.open({
            template: "/App_Plugins/FileSystemPicker/foldersystem-picker-upload-dialog.html",
            closeCallback: uploadClosed,
            dialogData: {
                node: ev.node
            }
        });
    };

    function nodeSelectHandler(ev, args) {
        args.event.preventDefault();
        args.event.stopPropagation();
        if (args.node.icon !== "icon-folder") {
            $scope.submit(args.node.id);
        }
    };


    $scope.dialogEventHandler.bind("treeNodeExpanded", nodeExpandedHandler);
    $scope.dialogEventHandler.bind("treeLoaded", treeLoadedHandler);

    function nodeExpandedHandler(ev, args) { };
    function treeLoadedHandler(ev, args) { };
};
angular.module("umbraco").controller("Umbraco.FileSystemPickerDialogController", fileSystemPickerDialogController);

