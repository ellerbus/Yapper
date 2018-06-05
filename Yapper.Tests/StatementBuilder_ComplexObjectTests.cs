using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yapper.Tests.DataObjects;

namespace Yapper.Tests
{
    [TestClass]
    public class StatementBuilder_ComplexObject_ComplexObjectTests : BaseStatementBuilder
    {
        [TestMethod]
        public void StatementBuilder_ComplexObject_Should_Save_AsUpsert()
        {
            var update = "update COMPLEX_OBJECT with (serializable) set Name = @Name where parent_id = @ParentID and this_id = @ThisID";
            var insert = "insert into COMPLEX_OBJECT ( parent_id, this_id, Name ) values ( @ParentID, @ThisID, @Name )";

            var expected = $"begin tran {update} if @@rowcount = 0 begin {insert} end commit tran";

            var sql = StatementBuilder.SaveOne<ComplexObject>();

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_ComplexObject_Should_Insert_IncludePrimaryKey()
        {
            var expected = "insert into COMPLEX_OBJECT ( parent_id, this_id, Name ) values ( @ParentID, @ThisID, @Name )";

            var sql = StatementBuilder.InsertOne<ComplexObject>();

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_ComplexObject_Should_Delete_ItemByPrimaryKey()
        {
            var expected = "delete from COMPLEX_OBJECT where parent_id = @ParentID and this_id = @ThisID";

            var sql = StatementBuilder.DeleteOne<ComplexObject>();

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_ComplexObject_Should_Update_ItemByPrimaryKey()
        {
            var expected = "update COMPLEX_OBJECT set Name = @Name where parent_id = @ParentID and this_id = @ThisID";

            var sql = StatementBuilder.UpdateOne<ComplexObject>();

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_ComplexObject_Should_Select_ItemByPrimaryKey()
        {
            var expected = "select parent_id ParentID, this_id ThisID, Name Name, Computed Computed from COMPLEX_OBJECT where parent_id = @ParentID and this_id = @ThisID";

            var sql = StatementBuilder.SelectOne<ComplexObject>();

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_ComplexObject_Should_Select_ItemByCustomWhere()
        {
            var expected = "select parent_id ParentID, this_id ThisID, Name Name, Computed Computed from COMPLEX_OBJECT where Name = @Name";

            var sql = StatementBuilder.SelectOne<ComplexObject>(new { Name = "abc" });

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_ComplexObject_Should_Select_All()
        {
            var expected = "select parent_id ParentID, this_id ThisID, Name Name, Computed Computed from COMPLEX_OBJECT";

            var sql = StatementBuilder.SelectMany<ComplexObject>();

            AssertSql(sql, expected);
        }

        [TestMethod]
        public void StatementBuilder_ComplexObject_Should_Select_AllUsingWhere()
        {
            var expected = "select parent_id ParentID, this_id ThisID, Name Name, Computed Computed from COMPLEX_OBJECT where Name = @Name";

            var sql = StatementBuilder.SelectMany<ComplexObject>(new { Name = "abc" });

            AssertSql(sql, expected);
        }
    }
}