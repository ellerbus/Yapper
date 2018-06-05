using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yapper.Tests.DataObjects;

namespace Yapper.Tests
{
    [TestClass]
    public class StatementBuilder_SimpleObject_SimpleObjectTests : BaseStatementBuilder
    {
        [TestMethod]
        public void StatementBuilder_SimpleObject_Should_Save_AsUpsert()
        {
            var update = "update SIMPLE_OBJECTS with (serializable) set name = @Name where id = @ID";
            var insert = "insert into SIMPLE_OBJECTS ( name ) values ( @Name ) set @ID = ident_current('dbo.SIMPLE_OBJECTS')";

            var expected = $"begin tran {update} if @@rowcount = 0 begin {insert} end select @ID commit tran";

            var sql = StatementBuilder.SaveOne<SimpleObject>();

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_SimpleObject_Should_Insert_IgnoreIdentity()
        {
            var expected = "insert into SIMPLE_OBJECTS ( name ) values ( @Name ) select ident_current('dbo.SIMPLE_OBJECTS')";

            var sql = StatementBuilder.InsertOne<SimpleObject>();

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_SimpleObject_Should_Delete_ItemByPrimaryKey()
        {
            var expected = "delete from SIMPLE_OBJECTS where id = @ID";

            var sql = StatementBuilder.DeleteOne<SimpleObject>();

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_SimpleObject_Should_Update_ItemByPrimaryKey()
        {
            var expected = "update SIMPLE_OBJECTS set name = @Name where id = @ID";

            var sql = StatementBuilder.UpdateOne<SimpleObject>();

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_SimpleObject_Should_Select_ItemByPrimaryKey()
        {
            var expected = "select id ID, name Name from SIMPLE_OBJECTS where id = @ID";

            var sql = StatementBuilder.SelectOne<SimpleObject>();

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_SimpleObject_Should_Select_ItemByCustomWhere()
        {
            var expected = "select id ID, name Name from SIMPLE_OBJECTS where name = @Name";

            var sql = StatementBuilder.SelectOne<SimpleObject>(new { Name = "abc" });

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_SimpleObject_Should_Select_All()
        {
            var expected = "select id ID, name Name from SIMPLE_OBJECTS";

            var sql = StatementBuilder.SelectMany<SimpleObject>();

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_SimpleObject_Should_Select_AllUsingWhere()
        {
            var expected = "select id ID, name Name from SIMPLE_OBJECTS where name = @Name";

            var sql = StatementBuilder.SelectMany<SimpleObject>(new { Name = "abc" });

            AssertSql(sql, expected);
        }
    }
}