using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core.IO;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Umbraco.FileSystemPicker.Controllers
{
    [Tree("dummy", "fileSystemTree", "File System")]
    [PluginController("FileSystemPicker")]
    public class FolderSystemTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            //if (!string.IsNullOrWhiteSpace(queryStrings.Get("startfolder")))
            //{
                string folder = id == "-1" ? queryStrings.Get("startfolder") : id;
                folder = folder.EnsureStartsWith("/");
                TreeNodeCollection tempTree = AddFolders(folder, queryStrings);
                tempTree.AddRange(AddFiles(folder, queryStrings));
                return tempTree;
            //}
            //return AddFiles()
            //return AddFolders(id == "-1" ? "" : id, queryStrings);
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            menu.Items.Add(new MenuItem("create", "Create"));

            return menu;
        }

        private TreeNodeCollection AddFiles(string folder, FormDataCollection queryStrings)
        {
            var pickerApiController = new FileSystemPickerApiController();
            //var str = queryStrings.Get("startfolder");

            if (string.IsNullOrWhiteSpace(folder))
                return null;

            var filter = queryStrings.Get("filter").Split(',').Select(a => a.Trim().EnsureStartsWith(".")).ToArray();


            var path = IOHelper.MapPath(folder);
            var rootPath = IOHelper.MapPath(queryStrings.Get("startfolder"));
            var treeNodeCollection = new TreeNodeCollection();

            foreach (FileInfo file in pickerApiController.GetFiles(folder, filter))
            {
                string nodeTitle = file.Name;
                string filePath = file.FullName.Replace(rootPath, "").Replace("\\", "/");

                //if (file.Extension.ToLower() == ".gif" || file.Extension.ToLower() == ".jpg" || file.Extension.ToLower() == ".png")
                //{
                //    nodeTitle += "<div><img src=\"/umbraco/backoffice/FileSystemPicker/FileSystemThumbnailApi/GetThumbnail?width=150&imagePath="+ HttpUtility.UrlPathEncode(filePath) +"\" /></div>";
                //}

                TreeNode treeNode = CreateTreeNode(filePath, path, queryStrings, nodeTitle, "icon-document", false);
                treeNodeCollection.Add(treeNode);
            }

            return treeNodeCollection;
        }

        private TreeNodeCollection AddFolders(string parent, FormDataCollection queryStrings)
        {
            var pickerApiController = new FileSystemPickerApiController();

            var filter = queryStrings.Get("filter").Split(',').Select(a => a.Trim().EnsureStartsWith(".")).ToArray();

            var treeNodeCollection = new TreeNodeCollection();
            treeNodeCollection.AddRange(pickerApiController.GetFolders(parent, filter)
                .Select(dir => CreateTreeNode(dir.FullName.Replace(IOHelper.MapPath("~"), "").Replace("\\", "/"),
                    parent, queryStrings, dir.Name,
                    "icon-folder", filter[0] == "." ? dir.EnumerateDirectories().Any() || pickerApiController.GetFiles(dir.FullName.Replace(IOHelper.MapPath("~"), "").Replace("\\", "/"), filter).Any() : pickerApiController.GetFiles(dir.FullName.Replace(IOHelper.MapPath("~"), "").Replace("\\", "/"), filter).Any())));

            return treeNodeCollection;
        }
    }
}