using System;
using System.Linq.Expressions;
using LinqKit;
using UtilityDelta.EFCore.Entities;

namespace UtilityDelta.EFCore.Database.QueryExtensions
{
    public static class KidExtensions
    {
        public static Expression<Func<Kid, bool>> OnlyCoolKids(bool checkName)
        {
            //Use LinqKit to dynamically build up the where expression based on input
            var builder = PredicateBuilder.New<Kid>(x => false);
            builder.Or(x => x.IsCool);
            if (checkName)
            {
                builder.Or(x => x.Name == "Mr. Cool");
            }
            return builder;
        }

        public static Expression<Func<Kid, bool>> ImportantKids() => 
            x => x.Id > 1;
    }
}
