using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoFlare.Models.CropModels
{
    public class Crop
    {
        public int height { get; set; }
        public int width { get; set; }
        public string alias { get; set; }
        public Coordinates coordinates { get; set; }
    }
}
