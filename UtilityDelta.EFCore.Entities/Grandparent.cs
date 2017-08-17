using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UtilityDelta.EFCore.Entities
{
    public class Grandparent
    {
        public Grandparent()
        {
            Parents = new List<Parent>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Parent> Parents { get; set; }
    }
}
