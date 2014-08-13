using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
    public class SelectBuilderTests : BuilderTests
    {
        [TestMethod]
        public void SelectBuilder_Should_Select_Enum_For_AnsiSql()
        {
            //  arrange
            var b = Sql.Select<ComplexObject>().Column(x => x.Status).Column(x => x.Order);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select case when [Status] = 'A' then 0 when [Status] = 'I' then 1 else 0 end as [Status], [Order] as [Order] from [COMPLEX_OBJECT]";

            //  assert
            Assert.AreEqual(sql, query);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_All_For_AnsiSql()
        {
            //  arrange
            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            var b = Sql.Select<IdentityObject>().Where(where);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select [id] as [IdentityID], [Name] as [Name] from [IDENTITY_OBJECT] where (([id] > @p0))";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(0, parameters["p0"]);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Count_All_For_AnsiSql()
        {
            //  arrange
            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            var b = Sql.Select<IdentityObject>().Count().Where(where);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select COUNT(1) from [IDENTITY_OBJECT] where (([id] > @p0))";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(0, parameters["p0"]);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Many_For_AnsiSql()
        {
            //  arrange
            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            var b = Sql.Select<IdentityObject>().Where(where);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select [id] as [IdentityID], [Name] as [Name] from [IDENTITY_OBJECT] where (([id] > @p0))";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(0, parameters["p0"]);
        }

        #region Paging

        [TestMethod]
        public void SelectBuilder_Should_Select_Page_For_AnsiSql()
        {
            //  **  not necessarily ANSI but SQLite, Postgres & MySql that I know of
            //  arrange
            SetDialect(new SQLiteDialect());

            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            var b = Sql.Select<IdentityObject>().Where(where).Page(1, 15);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select \"id\" as \"IdentityID\", \"Name\" as \"Name\"" +
                " from \"IDENTITY_OBJECT\" where ((\"id\" > @p0))" +
                " order by \"id\"" +
                " limit 0, 15"
                ;

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(0, parameters["p0"]);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Page_1_For_MsSql2008()
        {
            //  arrange
            SetDialect(new SqlServer2008Dialect());

            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            var b = Sql.Select<IdentityObject>().Where(where).Page(1, 15);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select * from" +
                " (select [id] as [IdentityID], [Name] as [Name], row_number() over (order by [id]) as rownbr" +
                " from [IDENTITY_OBJECT] where (([id] > @p0))) x" +
                " where x.rownbr between 1 and 15"
                ;

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(0, parameters["p0"]);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Page_2_For_MsSql2008()
        {
            //  arrange
            SetDialect(new SqlServer2008Dialect());

            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            var b = Sql.Select<IdentityObject>().Where(where).Page(2, 15);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select * from" +
                " (select [id] as [IdentityID], [Name] as [Name], row_number() over (order by [id]) as rownbr" +
                " from [IDENTITY_OBJECT] where (([id] > @p0))) x" +
                " where x.rownbr between 16 and 30"
                ;

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(0, parameters["p0"]);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Page_1_For_MsSql2012()
        {
            //  arrange
            SetDialect(new SqlServer2012Dialect());

            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            var b = Sql.Select<IdentityObject>().Where(where).Page(1, 15);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select [id] as [IdentityID], [Name] as [Name]" +
                " from [IDENTITY_OBJECT] where (([id] > @p0))" +
                " order by [id]" +
                " offset 0 rows fetch next 15 rows only"
                ;

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(0, parameters["p0"]);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Page_2_For_MsSql2012()
        {
            //  arrange
            SetDialect(new SqlServer2012Dialect());

            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            var b = Sql.Select<IdentityObject>().Where(where).Page(2, 15);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select [id] as [IdentityID], [Name] as [Name]" +
                " from [IDENTITY_OBJECT] where (([id] > @p0))" +
                " order by [id]" +
                " offset 15 rows fetch next 15 rows only"
                ;

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(0, parameters["p0"]);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Page_1_For_SqlCe()
        {
            //  arrange
            SetDialect(new SqlCeDialect());

            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            var b = Sql.Select<IdentityObject>().Where(where).Page(1, 15);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select \"id\" as \"IdentityID\", \"Name\" as \"Name\"" +
                " from \"IDENTITY_OBJECT\" where ((\"id\" > @p0))" +
                " order by \"id\"" +
                " offset 0 rows fetch next 15 rows only"
                ;

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(0, parameters["p0"]);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Page_2_For_SqlCe()
        {
            //  arrange
            SetDialect(new SqlCeDialect());

            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            var b = Sql.Select<IdentityObject>().Where(where).Page(2, 15);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select \"id\" as \"IdentityID\", \"Name\" as \"Name\"" +
                " from \"IDENTITY_OBJECT\" where ((\"id\" > @p0))" +
                " order by \"id\"" +
                " offset 15 rows fetch next 15 rows only"
                ;

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(0, parameters["p0"]);
        }

        #endregion

        #region Top'ing

        [TestMethod]
        public void SelectBuilder_Should_Select_Top_Using_Limit_Syntax_For_AnsiSql()
        {
            //  **  not necessarily ANSI but SQLite, Postgres & MySql that I know of
            SetDialect(new SQLiteDialect());

            var b = Sql.Select<IdentityObject>().Top(10);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select \"id\" as \"IdentityID\", \"Name\" as \"Name\" from \"IDENTITY_OBJECT\" limit 10";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(0, parameters.Count);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Top_For_MsSql_SqlCe()
        {
            var b = Sql.Select<IdentityObject>().Top(10);

            //  act
            var query = b.Query.TrimAndReduceWhitespace();

            var parameters = b.Parameters as IDictionary<string, object>;

            string sql = "select top(10) [id] as [IdentityID], [Name] as [Name] from [IDENTITY_OBJECT]";

            //  assert
            Assert.AreEqual(sql, query);
            Assert.AreEqual(0, parameters.Count);
        }

        #endregion
    }
}