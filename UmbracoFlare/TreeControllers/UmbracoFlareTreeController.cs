using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using System.Net.Http.Formatting;

namespace UmbracoFlare.Cloudflare.TreeControllers
{
    [Umbraco.Web.Trees.Tree("UmbracoFlare", "UmbracoFlare", "UmbracoFlare")]
    public class UmbracoFlareTreeController : TreeController
    {

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            TreeNodeCollection nodes = new TreeNodeCollection();
            TreeNode item = this.CreateTreeNode("-1", "-1", queryStrings, "");
            nodes.Add(item);
            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            throw new NotImplementedException();
        }
    }
}
