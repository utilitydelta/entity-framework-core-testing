using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace UtilityDelta.EFCore.Entities
{
    public class Kid
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsCool { get; set; }
        public Parent Parent { get; set; }
        public int ParentId { get; set; }
    }
}
