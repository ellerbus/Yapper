using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dapper;
using Yapper.Tests.Data;

namespace Yapper.Tests
{
    [TestClass]
    public class MsSqlIntegrationTests
    {
        private IDatabaseSession CreateSession()
        {
            return Database.OpenSession("MSSQL");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void IDatabaseSession_Should_Crash_Too_Many_Connection_Strings()
        {
            Database.OpenSession();
        }

        [TestMethod]
        public void IDatabaseSession_Should_Dispose_Without_Error()
        {
            using (var sut = CreateSession())
            {
                var uow = sut.BeginTransaction();

                //  stub
                Assert.IsInstanceOfType(uow, typeof(IDatabaseTransaction));
            }
        }

        [TestMethod]
        public void IDatabaseSession_Should_Rollback_And_Execute_Properly()
        {
            using (var sut = CreateSession())
            {
                int newid = 0;

                using (var uow = sut.BeginTransaction())
                {
                    Category c = new Category { Name = "abc" };

                    sut.Insert<Category>(c);

                    c.ID = sut.Query<int>("select max(CategoryID) from Categories").First();

                    newid = c.ID;

                    sut.Update<Category>(new { Name = "def" }, new { c.ID });
                }

                using (var uow = sut.BeginTransaction())
                {
                    string sql = "select top 1 * from Categories where CategoryID = @id;" +
                        "select top 1 * from Products where CategoryID = @id";

                    using (var r = sut.QueryMultiple(sql, new { id = newid }))
                    {
                        Categories c = r.Read<Categories>().FirstOrDefault();

                        if (c != null)
                        {
                            c.Products = r.Read<Products>().ToList();
                        }

                        Assert.IsNull(c);
                    }
                }
            }
        }
    }
}