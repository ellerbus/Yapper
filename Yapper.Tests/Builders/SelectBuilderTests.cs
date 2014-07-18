using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Yamor.Builders;
using Yamor.Tests.DataObjects;

//  TODO SQLITE, POSTGRESQL, FIREBIRD, MYSQL, SQLCE paging sql verifications on 0 vs 1 indexing

namespace Yamor.Tests.Builders
{
    [TestClass]
    public class SelectBuilderTests
    {
        #region Setup

        private IFixture AutoFixture { get; set; }
        private Mock<ISession> MockSession { get; set; }

        private CompositeKeyObject DefaultCompositeKeyObject { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            AutoFixture = new Fixture().Customize(new AutoMoqCustomization());

            MockSession = AutoFixture.Freeze<Mock<ISession>>();

            DefaultCompositeKeyObject = AutoFixture.Create<CompositeKeyObject>();
        }

        #endregion

        [TestMethod]
        public void SelectBuilder_Should_Select_All_For_AnsiSql()
        {
            //  arrange
            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetSelectBuilder<IdentityObject>(MockSession.Object);

            b.Where(where);

            //  act
            var results = b.BuildQuery();

            string sql = "select [id] as [IdentityID], [Name] as [Name] from [IDENTITY_OBJECT] where ([id] > @p0)";

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(0, results.Parameters["@p0"].Value);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Many_For_AnsiSql()
        {
            //  arrange
            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetSelectBuilder<IdentityObject>(MockSession.Object);

            b.Where(where);

            //  act
            var results = b.BuildQuery();

            string sql = "select [id] as [IdentityID], [Name] as [Name] from [IDENTITY_OBJECT] where ([id] > @p0)";

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(0, results.Parameters["@p0"].Value);
        }

        #region Paging

        [TestMethod]
        public void SelectBuilder_Should_Select_Page_For_AnsiSql()
        {
            //  **  not necessarily ANSI but SQLite, Postgres & MySql that I know of
            //  arrange
            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            MockSession.SetupGet(x => x.Driver).Returns(Helper.SQLiteDriver);

            var b = MockSession.Object.Driver.Dialect.GetSelectBuilder<IdentityObject>(MockSession.Object);

            b.Where(where);

            AppendPage(b);

            //  act
            var results = b.BuildQuery();

            string sql = "select \"id\" as \"IdentityID\", \"Name\" as \"Name\"" +
                " from \"IDENTITY_OBJECT\" where (\"id\" > @p0)" +
                " order by \"id\"" +
                " limit 1, 15"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(0, results.Parameters["@p0"].Value);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Page_For_MsSql()
        {
            //  arrange
            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetSelectBuilder<IdentityObject>(MockSession.Object);

            b.Where(where);

            AppendPage(b);

            //  act
            var results = b.BuildQuery();

            string sql = "select [IdentityID], [Name] from" +
                " (select [id] as [IdentityID], [Name] as [Name], row_number() over (order by [id]) as paging_rownumber" +
                " from [IDENTITY_OBJECT] where ([id] > @p0)) as paging_select" +
                " where paging_select.paging_rownumber between 1 and 15"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(0, results.Parameters["@p0"].Value);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Page_For_PostgreSql()
        {
            //  **  not necessarily ANSI but SQLite, Postgres & MySql that I know of
            //  arrange
            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            MockSession.SetupGet(x => x.Driver).Returns(Helper.PostgreSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetSelectBuilder<IdentityObject>(MockSession.Object);

            b.Where(where);

            AppendPage(b);

            //  act
            var results = b.BuildQuery();

            string sql = "select \"id\" as \"IdentityID\", \"Name\" as \"Name\"" +
                " from \"IDENTITY_OBJECT\" where (\"id\" > @p0)" +
                " order by \"id\"" +
                " limit 1 offset 15"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(0, results.Parameters["@p0"].Value);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Page_For_FirebirdSql()
        {
            //  **  not necessarily ANSI but SQLite, Postgres & MySql that I know of
            //  arrange
            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            MockSession.SetupGet(x => x.Driver).Returns(Helper.FirebirdSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetSelectBuilder<IdentityObject>(MockSession.Object);

            b.Where(where);

            AppendPage(b);

            //  act
            var results = b.BuildQuery();

            string sql = "select \"id\" as \"IdentityID\", \"Name\" as \"Name\"" +
                " from \"IDENTITY_OBJECT\" where (\"id\" > @p0)" +
                " order by \"id\"" +
                " rows 1 to 16"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(0, results.Parameters["@p0"].Value);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Page_For_SqlCe()
        {
            //  **  not necessarily ANSI but SQLite, Postgres & MySql that I know of
            //  arrange
            Expression<Func<IdentityObject, bool>> where = x => x.IdentityID > 0;

            MockSession.SetupGet(x => x.Driver).Returns(Helper.SqlCeDriver);

            var b = MockSession.Object.Driver.Dialect.GetSelectBuilder<IdentityObject>(MockSession.Object);

            b.Where(where);

            AppendPage(b);

            //  act
            var results = b.BuildQuery();

            string sql = "select \"id\" as \"IdentityID\", \"Name\" as \"Name\"" +
                " from \"IDENTITY_OBJECT\" where (\"id\" > @p0)" +
                " order by \"id\"" +
                " offset 1 rows fetch next 15 rows only"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(0, results.Parameters["@p0"].Value);
        }

        private void AppendPage(ISelectBuilder<IdentityObject> b)
        {
            Type selectBuilder = typeof(ISession).Assembly.GetType("Yamor.Builders.SelectBuilder`1");

            Type builder = selectBuilder.MakeGenericType(typeof(IdentityObject));

            PrivateObject po = new PrivateObject(b, new PrivateType(builder));

            //  act
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

            po.SetFieldOrProperty("PageNumber", flags, 1);
            po.SetFieldOrProperty("ItemsPerPage", flags, 15);
        }

        #endregion

        #region Top'ing

        [TestMethod]
        public void SelectBuilder_Should_Select_Top_For_AnsiSql()
        {
            //  **  not necessarily ANSI but SQLite, Postgres & MySql that I know of
            MockSession.SetupGet(x => x.Driver).Returns(Helper.SQLiteDriver);

            var b = MockSession.Object.Driver.Dialect.GetSelectBuilder<IdentityObject>(MockSession.Object);

            b.Limit(10);

            //  act
            var results = b.BuildQuery();

            string sql = "select \"id\" as \"IdentityID\", \"Name\" as \"Name\"" +
                " from \"IDENTITY_OBJECT\"" +
                " limit 10"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(0, results.Parameters.Count);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Top_For_MsSql()
        {
            //  **  not necessarily ANSI but SQLite, Postgres & MySql that I know of
            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetSelectBuilder<IdentityObject>(MockSession.Object);

            b.Limit(10);

            //  act
            var results = b.BuildQuery();

            string sql = "select top 10 [id] as [IdentityID], [Name] as [Name]" +
                " from [IDENTITY_OBJECT]"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(0, results.Parameters.Count);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Top_For_SqlCe()
        {
            //  **  not necessarily ANSI but SQLite, Postgres & MySql that I know of
            MockSession.SetupGet(x => x.Driver).Returns(Helper.SqlCeDriver);

            var b = MockSession.Object.Driver.Dialect.GetSelectBuilder<IdentityObject>(MockSession.Object);

            b.Limit(10);

            //  act
            var results = b.BuildQuery();

            string sql = "select top(10) \"id\" as \"IdentityID\", \"Name\" as \"Name\"" +
                " from \"IDENTITY_OBJECT\""
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(0, results.Parameters.Count);
        }

        [TestMethod]
        public void SelectBuilder_Should_Select_Top_For_Firebird()
        {
            //  **  not necessarily ANSI but SQLite, Postgres & MySql that I know of
            MockSession.SetupGet(x => x.Driver).Returns(Helper.FirebirdSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetSelectBuilder<IdentityObject>(MockSession.Object);

            b.Limit(10);

            //  act
            var results = b.BuildQuery();

            string sql = "select first 10 \"id\" as \"IdentityID\", \"Name\" as \"Name\"" +
                " from \"IDENTITY_OBJECT\""
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(0, results.Parameters.Count);
        }

        #endregion
    }
}