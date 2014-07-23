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
    public class UpdateBuilderTests : BuilderTests
    {
        [TestMethod]
        public void UpdateBuilder_Should_Update_All_For_AnsiSql_Anonymous_Set()
        {
            //  arrange
            var b = Sql.Update<ComplexObject>().Set(new { YesNo = false });

            //  act
            var query = b.Query;

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "update [COMPLEX_OBJECT] set [YesNo] = 0";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(0, parameters.Count);
        }

        [TestMethod]
        public void UpdateBuilder_Should_Update_All_For_AnsiSql_Expression_Set()
        {
            //  arrange
            var b = Sql.Update<ComplexObject>().Set(x => x.ID, x => x.ID + 999);

            //  act
            var query = b.Query;

            var parameters = b.Parameters as IDictionary<string,object>;

            string sql = "update [COMPLEX_OBJECT] set [id] = ([id] + @p0)";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(999, parameters["p0"]);
        }

        [TestMethod]
        public void UpdateBuilder_Should_Update_Many_For_AnsiSql()
        {
            //  arrange
            DefaultComplexObject.YesNo = true;

            Expression<Func<ComplexObject, bool>> where = x => x.ID > 0;

            var b = Sql.Update<ComplexObject>();

            b.Set(new { DefaultComplexObject.YesNo }).Where(where);

            //  act
            var query = b.Query;

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "update [COMPLEX_OBJECT] set [YesNo] = 1 where (([id] > @p0))";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(0, parameters["p0"]);
        }

        [TestMethod]
        public void UpdateBuilder_Should_Update_Many_For_AnonymousObjects()
        {
            //  arrange
            var b = Sql.Update<IdentityObject>();

            b.Set(new { Name = "abc" }).Where(new { IdentityID = 123 });

            //  act
            var query = b.Query;

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "update [IDENTITY_OBJECT] set [Name] = @p0 where ([id] = @p1)";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("abc", parameters["p0"]);
            Assert.AreEqual(123, parameters["p1"]);
        }
    }
}
