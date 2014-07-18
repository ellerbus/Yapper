using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Yapper.Builders;
using Yapper.Tests.DataObjects;

namespace Yapper.Tests.Builders
{
    [TestClass]
    public class DeleteBuilderTests : BuilderTests
    {
        [TestMethod]
        public void DeleteBuilder_Should_Delete_All_For_AnsiSql()
        {
            //  arrange
            var b = Sql.Delete<CompositeKeyObject>();

            //  act
            var query = b.Query;

            var sql = "delete from [COMPOSITE_KEY_OBJECT]";

            //  assert
            Assert.AreEqual(sql, query);
        }

        [TestMethod]
        public void DeleteBuilder_Should_Delete_Many_For_AnsiSql()
        {
            //  arrange
            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            var b = Sql.Delete<IdentityObject>();

            b.Where(where);

            //  act
            var query = b.Query;

            var parameters = b.Parameters as IDictionary<string, object>;

            var sql = "delete from [IDENTITY_OBJECT] where ([id] > @p0)";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(0, parameters["p0"]);
        }

        [TestMethod]
        public void DeleteBuilder_Should_Delete_Many_For_AnonymousObject()
        {
            //  arrange
            var b = Sql.Delete<IdentityObject>();

            b.Where(new { IdentityID = 123 });

            //  act
            var query = b.Query;

            var parameters = b.Parameters as IDictionary<string, object>;

            var sql = "delete from [IDENTITY_OBJECT] where [id] = @p0";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [TestMethod]
        public void DeleteBuilder_Should_Delete_Many_For_TypedObject()
        {
            //  arrange
            var b = Sql.Delete<CompositeKeyObject>();

            b.Where(DefaultCompositeKeyObject);

            //  act
            var query = b.Query;

            var parameters = b.Parameters as IDictionary<string, object>;

            var sql = "delete from [COMPOSITE_KEY_OBJECT] where [parent_id] = @p0 and [this_id] = @p1";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual(DefaultCompositeKeyObject.ParentID, parameters["p0"]);
            Assert.AreEqual(DefaultCompositeKeyObject.ThisID, parameters["p1"]);
        }
    }
}