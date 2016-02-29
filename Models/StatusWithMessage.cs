using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoFlare.Models
{
    public class StatusWithMessage
    {
        public StatusWithMessage(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }

        public StatusWithMessage() { }

        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
