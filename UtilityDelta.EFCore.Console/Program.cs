using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UtilityDelta.EFCore.Database;
using UtilityDelta.EFCore.Entities;

namespace UtilityDelta.EFCore.Console
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var dbContext = new FamilyContext())
            {
                dbContext.Database.Migrate();

                dbContext.Grandparents.Add(new Grandparent()
                {
                    Name = "Geoff" + DateTime.Now.ToLongTimeString()
                });

                dbContext.SaveChanges();

                System.Console.WriteLine(dbContext.Grandparents.OrderByDescending(x => x.Id).FirstOrDefault().Name);
                System.Console.WriteLine(dbContext.Grandparents.Count());
            }
        }
    }
}
