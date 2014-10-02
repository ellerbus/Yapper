using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniqueNamespace;
using UniqueNamespace.Dapper;
using Yapper.Core;
using Yapper.Tests.DataObjects;

namespace Yapper.Tests
{
    [TestClass]
    public class QueryBuilderTests
    {
        [TestMethod]
        public void QueryBuilder_Should_Insert_Ignore_Identity()
        {
            var expected = "insert into SIMPLE_OBJECTS (name) values (@Name)";

            var sql = QueryBuilder.Insert<SimpleObject>(new SimpleObject());

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Insert_Anonymous_Ignore_Identity()
        {
            var expected = "insert into SIMPLE_OBJECTS (name) values (@Name)";

            var sql = QueryBuilder.Insert<SimpleObject>(new { Name = "ABC" });

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Insert_Anonymous_Ignore_Not_Found()
        {
            var expected = "insert into SIMPLE_OBJECTS (name) values (@Name)";

            var sql = QueryBuilder.Insert<SimpleObject>(new { Name = "ABC", IsModified = false });

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Delete_All()
        {
            var expected = "delete from SIMPLE_OBJECTS";

            var sql = QueryBuilder.Delete<SimpleObject>();

            Assert.AreEqual(expected, sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Delete_Item()
        {
            var expected = "delete from SIMPLE_OBJECTS where id = @ID";

            var sql = QueryBuilder.Delete<SimpleObject>(new SimpleObject());

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Delete_Item_Composite_Key()
        {
            var expected = "delete from COMPLEX_OBJECT where parent_id = @ParentID and this_id = @ThisID";

            var sql = QueryBuilder.Delete<ComplexObject>(new ComplexObject());

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Delete_Anonymous()
        {
            var expected = "delete from SIMPLE_OBJECTS where name = @Name";

            var sql = QueryBuilder.Delete<SimpleObject>(new { Name = "ABC" });

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Delete_Anonymous_Ignore_Not_Found()
        {
            var expected = "delete from SIMPLE_OBJECTS where name = @Name";

            var sql = QueryBuilder.Delete<SimpleObject>(new { Name = "ABC", IsModified = false });

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Update_All()
        {
            var expected = "update SIMPLE_OBJECTS set name = @Name";

            var sql = QueryBuilder.Update<SimpleObject>(new { Name = "ABC" }, null);

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Update_Item()
        {
            var expected = "update SIMPLE_OBJECTS set name = @Name where id = @ID";

            var sql = QueryBuilder.Update<SimpleObject>(new SimpleObject());

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Update_Item_Composite_Key()
        {
            var expected = "update COMPLEX_OBJECT set Name = @Name where parent_id = @ParentID and this_id = @ThisID";

            var sql = QueryBuilder.Update<ComplexObject>(new ComplexObject());

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Update_Anonymous()
        {
            var expected = "update COMPLEX_OBJECT set Name = @Name where Computed = @Computed";

            var sql = QueryBuilder.Update<ComplexObject>(new { Name = "ABC" }, new { Computed = "DEF" });

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void QueryBuilder_Should_Update_Throws_Invalid_Operation()
        {
            var expected = "";

            var sql = QueryBuilder.Update<ComplexObject>(new { Name = "ABC" }, new { Name = "DEF" });

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Select_All()
        {
            var expected = "select * from SIMPLE_OBJECTS";

            var sql = QueryBuilder.Select<SimpleObject>();

            Assert.AreEqual(expected, sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Select_Item()
        {
            var expected = "select * from SIMPLE_OBJECTS where id = @ID";

            var sql = QueryBuilder.Select<SimpleObject>(new { ID = 0 });

            Assert.AreEqual(expected, sql.Sql);
        }

        [TestMethod]
        public void QueryBuilder_Should_Get_Use_From_Extension()
        {
            var expected = "SELECT Count(*) FROM SIMPLE_OBJECTS ";

            var b = new SqlBuilder().From<SimpleObject>();

            var t = b.AddTemplate(Templates.Count);

            Assert.AreEqual(expected, Regex.Replace(t.RawSql, @"\s+", " "));
        }
    }
}