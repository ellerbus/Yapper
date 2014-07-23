﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yapper.Tests.Data;

namespace Yapper.Tests
{
    [TestClass]
    public class MsSqlIntegrationTests
    {
        [TestMethod]
        public void MsSql_Should_Rollback()
        {
            using (ISession db = GetDb())
            {
                using (IUnitOfWork uow = db.CreateUnitOfWork())
                {
                }
                //ISqlQuery sql = Sql.Select<Products>().Top(10);

                //IList<Products> products = db.Query<Products>(sql).ToList();
            }
        }

        private ISession GetDb() { return DB.Open("MsSql"); }
    }
}
