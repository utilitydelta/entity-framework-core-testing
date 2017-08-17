using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace UtilityDelta.EFCore.Entities
{
    public class Parent
    {
        public Parent()
        {
            Kids = new List<Kid>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsMale { get; set; }
        public Grandparent Grandparent { get; set; }
        public int GrandparentId { get; set; }
        public ICollection<Kid> Kids { get; set; }
    }
}
