﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yapper.Tests.Data;
using Dapper;

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
                    Categories c = new Categories { CategoryName = "abc" };

                    ISqlQuery sqlc = Sql.Insert<Categories>(c);

                    c.CategoryID = db.Query<int>(sqlc).First();
                }
                using (IUnitOfWork uow = db.CreateUnitOfWork())
                {
                    ISqlQuery sqlc = Sql.Select<Categories>().Top(1).Where(x => x.CategoryID == 1);
                    ISqlQuery sqlp = Sql.Select<Products>().Top(1).Where(x => x.CategoryID == 1);

                    using (var r = db.Query(sqlc, sqlp))
                    {
                        Categories c = r.Read<Categories>().FirstOrDefault();

                        c.Products = r.Read<Products>().ToList();
                    }
                }
                //ISqlQuery sql = Sql.Select<Products>().Top(10);

                //IList<Products> products = db.Query<Products>(sql).ToList();
            }
        }

        private ISession GetDb() { return DB.Open("MsSql"); }
    }
}
