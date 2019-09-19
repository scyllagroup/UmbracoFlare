using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace UmbracoFlare.Components
{
    public class TreeMenuComponent : IComponent
    {
        public TreeMenuComponent()
        {
        }

        public void Initialize()
        {
            TreeControllerBase.MenuRendering += AddPurgeCacheForContentMenu;
        }

        public void Terminate()
        {
            TreeControllerBase.MenuRendering -= AddPurgeCacheForContentMenu;
        }
        private void AddPurgeCacheForContentMenu(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            if (sender.TreeAlias != "content")
            {
                //We aren't dealing with the content menu
                return;
            }

            MenuItem menuItem = new MenuItem("purgeCache", "Purge Cloudflare Cache");

            menuItem.Icon = "umbracoflare-tiny";
            menuItem.OpensDialog = true;
            menuItem.LaunchDialogView("/App_Plugins/UmbracoFlare/backoffice/treeViews/PurgeCacheDialog.html", "Purge Cloudflare Cache");

            e.Menu.Items.Insert(e.Menu.Items.Count - 1, menuItem);
        }
    }
}
