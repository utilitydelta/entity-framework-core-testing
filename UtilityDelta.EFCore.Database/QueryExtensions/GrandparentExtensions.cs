using System;
using System.Linq.Expressions;
using UtilityDelta.EFCore.Entities;

namespace UtilityDelta.EFCore.Database.QueryExtensions
{
    public static class GrandparentExtensions
    {
        public static Expression<Func<Grandparent, bool>> PopularName(this Grandparent entity) => 
            x => x.Name == "Geoff";
    }
}
