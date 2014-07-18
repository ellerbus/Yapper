using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yapper.Tests.Data;

namespace Yapper.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var db = DB.Open())
            {
                ISqlQuery sql = Sql.Select<Products>().Top(10);

                IList<Products> products = db.Query<Products>(sql).ToList();
            }
        }
    }
}
