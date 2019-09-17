using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models.Entities;

namespace UmbracoFlare.Extensions
{
    public static class IEntityExtensions
    {
        public static bool IsNew(this IEntity entity)
        {
            return entity.CreateDate == entity.UpdateDate;
        }
    }
}
