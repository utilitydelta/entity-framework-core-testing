using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilityDelta.EFCore.Database;
using UtilityDelta.EFCore.Entities;
using UtilityDelta.EFCore.Database.QueryExtensions;

namespace UtilityDelta.EFCore.Tests
{
    [TestClass]
    public class TestEntityFrameworkCore
    {
        private FamilyContext m_context;
        private List<string> m_sql = new List<string>();

        [TestInitializeAttribute]
        public async Task Init()
        {
            File.Delete(FamilyContext.DatabasePath);
            m_context = new FamilyContext();
            await m_context.Database.MigrateAsync();
            InsertData(m_context);

            var serviceProvider = m_context.GetInfrastructure();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddProvider(new SqlLoggerProvider(m_sql));
        }

        private static void InsertData(FamilyContext context)
        {
            context.Grandparents.Add(new Grandparent
            {
                Name = "Geoff",
                Parents = new List<Parent>
                {
                    new Parent
                    {
                        Name = "Joe",
                        IsMale = true,
                        Kids = new List<Kid>()
                        {
                            new Kid()
                            {
                                IsCool = true,
                                Name = "Kid one"
                            },
                            new Kid()
                            {
                                IsCool = false,
                                Name = "Mr. Cool"
                            },
                        }
                    }
                }
            });

            context.Grandparents.Add(new Grandparent()
            {
                Name = "Alexander"
            });

            context.SaveChanges();
        }
        
        [TestMethod]
        public async Task TestChildExists()
        {
            //Mark as expandable for linqkit operations on query tree
            var query = m_context.Parents.AsExpandable();

            //Where clause on child collection generates sub query WHERE EXISTS in FROM clause
            query = query.AtLeastOneKidHas(KidExtensions.ImportantKids());

            var result = await query.CountAsync();
            Assert.AreEqual(result, 1);

            var sqlText = m_sql.Last();
            Assert.IsTrue(sqlText.Contains("WHERE EXISTS"));
        }
    }
}
