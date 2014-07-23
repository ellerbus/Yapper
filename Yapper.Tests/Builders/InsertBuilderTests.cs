﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Yapper.Builders;
using Yapper.Dialects;
using Yapper.Tests.DataObjects;

namespace Yapper.Tests.Builders
{
    [TestClass]
    public class InsertBuilderTests : BuilderTests
    {
        [TestMethod]
        public void InsertBuilder_Should_Insert_Identity_Correctly_For_MsSql()
        {
            //  arrange
            SetDialect(new SqlServerDialect());

            var b = Sql.Insert<IdentityObject>(DefaultIdentityObject);

            //  act
            var query = b.Query;

            var parameters = b.Parameters as IDictionary<string, object>;

            var sql = "insert into [IDENTITY_OBJECT] ([Name]) values (@p0);select SCOPE_IDENTITY()";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(DefaultIdentityObject.Name, parameters["p0"]);
        }

        [TestMethod]
        public void InsertBuilder_Should_Insert_Identity_Correctly_For_SqlCe()
        {
            //  arrange
            SetDialect(new SqlCeDialect());

            var b = Sql.Insert<IdentityObject>(DefaultIdentityObject);

            //  act
            var query = b.Query;

            var parameters = b.Parameters as IDictionary<string, object>;

            var sql = "insert into \"IDENTITY_OBJECT\" (\"Name\") values (@p0);select @@IDENTITY";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(DefaultIdentityObject.Name, parameters["p0"]);
        }

        [TestMethod]
        public void InsertBuilder_Should_Insert_Identity_Correctly_For_SQLite()
        {
            //  arrange
            SetDialect(new SQLiteDialect());

            var b = Sql.Insert<IdentityObject>(DefaultIdentityObject);

            //  act
            var query = b.Query;

            var parameters = b.Parameters as IDictionary<string, object>;

            var sql = "insert into \"IDENTITY_OBJECT\" (\"Name\") values (@p0);select last_insert_rowid()";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(DefaultIdentityObject.Name, parameters["p0"]);
        }

        [TestMethod]
        public void InsertBuilder_Should_Insert_Composite_Correctly_For_AnsiSql()
        {
            //  arrange
            var b = Sql.Insert<CompositeKeyObject>(DefaultCompositeKeyObject);

            //  act
            var query = b.Query;

            var parameters = b.Parameters as IDictionary<string, object>;

            var sql = "insert into [COMPOSITE_KEY_OBJECT] ([parent_id], [this_id], [Name]) values (@p0, @p1, @p2)"                ;

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(3, parameters.Count);
            Assert.AreEqual(DefaultCompositeKeyObject.ParentID, parameters["p0"]);
            Assert.AreEqual(DefaultCompositeKeyObject.ThisID, parameters["p1"]);
            Assert.AreEqual(DefaultCompositeKeyObject.Name, parameters["p2"]);
        }
    }
}