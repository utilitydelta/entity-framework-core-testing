using System;
using System.Linq;
using System.Linq.Expressions;
using UtilityDelta.EFCore.Entities;

namespace UtilityDelta.EFCore.Database.QueryExtensions
{
    public static class ParentExtensions
    {
        public static IQueryable<Parent> AtLeastOneKidHas(this IQueryable<Parent> query,
            Expression<Func<Kid, bool>> expression)
        {
            //Here we use LinqKit Compile() to meet compile time requirements
            //This is because Any() takes a Func<> instead of Expression<Func<>>
            //Compile will turn the Expression into a Func. If we didn't do this, we
            //will get a compile time error (bad). If we just passed around Func<>, this wouldn't 
            //be executed as part of the EntityFramework expression tree (also bad)
            return query.Where(q => q.Kids.Any(expression.Compile()));
        }
    }
}
