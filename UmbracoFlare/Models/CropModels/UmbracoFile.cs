using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoFlare.Models.CropModels
{
    class UmbracoFile
    {
        public string src { get; set; }
        public IEnumerable<Crop> crops { get; set; }
        public FocalPoint focalPoint { get; set; }
    }
}
