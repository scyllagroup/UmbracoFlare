using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Xml.Linq;

namespace UmbracoFlare.Configuration
{
    public class XmlCloudflareConfiguration : ICloudflareConfiguration
    {
        public static string CONFIG_PATH;
        private XDocument _doc = null;

        public XmlCloudflareConfiguration()
        {
            try
            {
                CONFIG_PATH = HttpContext.Current.Server.MapPath("~/Config/cloudflare.config");
                this._doc = XDocument.Load(CONFIG_PATH);
            }
            catch(Exception e)
            {

            }
        }

        public string ApiKey
        {
            get
            {
                return this._doc==null ? String.Empty: this._doc.Root.Element("apiKey").Value;
            }
            set
            {
                this._doc.Root.Element("apiKey").SetValue(value);
                this._doc.Save(CONFIG_PATH);
            }
        }
        
        
        public string AccountEmail
        {
            get
            {
                return this._doc == null ? String.Empty : this._doc.Root.Element("accountEmail").Value;
            }
            set
            {
                this._doc.Root.Element("accountEmail").SetValue(value);
                this._doc.Save(CONFIG_PATH);
            }
        }

        
        public bool PurgeCacheOn 
        {
            get
            {
                bool purgeCacheOn = false;
                if(this._doc!=null)
                {
                    bool.TryParse(this._doc.Root.Element("purgeCacheOn").Value, out purgeCacheOn);
                }
                return purgeCacheOn;
            }

            set
            {
                this._doc.Root.Element("purgeCacheOn").SetValue(value.ToString());
                this._doc.Save(CONFIG_PATH);
            }
        }

        /*
        public List<string> AdditionalZoneUrls
        {
            get
            {
                string additionalZoneUrlsCommaSep = this._doc.Root.Element("additionalZoneUrls").Value;

                if(!String.IsNullOrEmpty(additionalZoneUrlsCommaSep))
                {
                    return additionalZoneUrlsCommaSep.Split(',').ToList();
                }
                else
                {
                    return new List<string>();
                }
            }
        }*/
    }
}
