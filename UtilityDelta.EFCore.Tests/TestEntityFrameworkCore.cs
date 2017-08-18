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
                    },
                    new Parent()
                    {
                        Name = "Frank",
                        IsMale = true
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

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task TestProjection(bool checkName)
        {
            //Mark as expandable for linqkit operations on query tree
            var query = m_context.Parents.AsExpandable();

            //Flatten data with projection
            var coolKids = KidExtensions.OnlyCoolKids(checkName);
            var projection = query.Select(parent => new
            {
                Grandparent = parent.Grandparent,

                //Generates outer apply (TOP 1) or sub select in SELECT clause
                //Project this early so we can use it later in our Select of columns
                BestKid = parent.Kids.FirstOrDefault(kid =>
                    //combine func with Invoke() with other clause
                    coolKids.Invoke(kid) || kid.Name == "boo"),

                //Not generated in SQL as not referenced in query (no Where / Select)
                parent.Kids
            });

            projection = projection
                .Where(parent => parent.BestKid.Name != "random name")
                .Where(parent => parent.Grandparent.Name == "Geoff");

            Assert.AreEqual(await projection.CountAsync(), 1);

            var sqlText = m_sql.Last();

            //Uses the lambda variable name as alias
            Assert.IsTrue(sqlText.Contains("FROM \"Parents\" AS \"parent\""));

            //INNER JOIN for grandparent
            Assert.IsTrue(sqlText.Contains("INNER JOIN \"Grandparents\" AS \"parent.Grandparent\" ON \"parent\".\"GrandparentId\" = \"parent.Grandparent\".\"Id\""));

            if (checkName)
            {
                //Also check if the kid is called "Mr. Cool"
                Assert.IsTrue(sqlText.Contains("WHERE (((\"kid\".\"IsCool\" = 1) OR (\"kid\".\"Name\" = 'Mr. Cool'))"));
            }
            else
            {
                //Check if kid is cool
                Assert.IsTrue(sqlText.Contains("WHERE ((\"kid\".\"IsCool\" = 1) OR (\"kid\".\"Name\" = 'boo'))"));
            }

            //Apply limit for kids as we are doing sub select
            Assert.IsTrue(sqlText.Contains($"LIMIT 1{Environment.NewLine}) <> 'random name')"));

            //Grandparent must be Geoff
            Assert.IsTrue(sqlText.Contains("AND (\"parent.Grandparent\".\"Name\" = 'Geoff')"));
        }

        [TestMethod]
        public async Task TestColumnRestriction()
        {
            var query = m_context.Kids
                .Select(kid => new
                {
                    kid.Name,
                    kid.IsCool
                });

            var data = await query.ToListAsync();

            Assert.IsFalse(data[1].IsCool);

            var sqlText = m_sql.Last();
            Assert.AreEqual(sqlText, "SELECT \"kid\".\"Name\", \"kid\".\"IsCool\"\r\nFROM \"Kids\" AS \"kid\"");
        }

        
        [TestMethod]
        public async Task TestMultipleRefsToSubQuery()
        {
            var query = m_context.Parents
                .Select(parent => new
                {
                    Name = parent.Name,
                    //MUST always use FirstOrDefault otherwise the query will be split into two queries
                    BestKid = parent.Kids.FirstOrDefault(kid => kid.IsCool)
                });

            var query2 = query
                .Select(x => new
            {
                KidName = x.BestKid.Name,
                x.Name
                });
            
            var data = await query2.ToListAsync();

            Assert.AreEqual(data[0].KidName, "Kid one");
            Assert.AreEqual(data[0].Name, "Joe");

            Assert.AreEqual(m_sql.Count, 2);

            // BestKid becomes a sub query in the select statement
            var sqlText = m_sql.Last();
            Assert.IsTrue(sqlText.Contains("SELECT (\r\n    SELECT \"kid\".\"Name\"\r\n    FROM \"Kids\" AS \"kid\"\r\n    WHERE (\"kid\".\"IsCool\" = 1) AND (\"parent\".\"Id\" = \"kid\".\"ParentId\")\r\n    LIMIT 1\r\n) AS \"KidName\","));
        }

        [TestMethod]
        public async Task TestJoinParentMultipleColumns()
        {
            var query = m_context.Parents
                .Select(parent => new
                {
                    //Single join for two parent columns
                    GrandparentName = parent.Grandparent.Name,
                    GrandparentId = parent.Grandparent.Id,
                });
            var result = await query.FirstOrDefaultAsync();

            Assert.AreEqual(result.GrandparentName, "Geoff");
            Assert.AreEqual(result.GrandparentId, 1);

            //Only one INNER JOIN
            var sqlText = m_sql.Last();
            Assert.IsTrue(sqlText.Split("INNER JOIN").Length == 2);

        }
    }
}
