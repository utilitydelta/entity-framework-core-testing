using System;
using System.Linq.Expressions;

namespace UtilityDelta.EFCore.Entities
{
    public static class Navigation
    {
        public static Expression<Func<Parent, Grandparent>> ParentToGrandparent =>
            x => x.Grandparent;
        public static Expression<Func<Kid, Parent>> KidToParent =>
            x => x.Parent;
    }
}
