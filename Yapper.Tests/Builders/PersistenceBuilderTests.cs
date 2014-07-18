using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Yamor.Builders;
using Yamor.Tests.DataObjects;

namespace Yamor.Tests.Builders
{
    [TestClass]
    public class PersistenceBuilderTests
    {
        #region Setup

        private IFixture AutoFixture { get; set; }
        private Mock<ISession> MockSession { get; set; }

        private IdentityObject DefaultIdentityObject { get; set; }
        private CompositeKeyObject DefaultCompositeKeyObject { get; set; }
        private ComplexObject DefaultComplexObject { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            AutoFixture = new Fixture().Customize(new AutoMoqCustomization());

            MockSession = AutoFixture.Freeze<Mock<ISession>>();

            DefaultIdentityObject = AutoFixture.Create<IdentityObject>();
            DefaultCompositeKeyObject = AutoFixture.Create<CompositeKeyObject>();
            DefaultComplexObject = AutoFixture.Create<ComplexObject>();
        }

        #endregion

        #region Insert Persistence Tests

        [TestMethod]
        public void PersistenceBuilder_Should_Insert_Identity_Correctly_For_MsSql()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetPersistenceBuilder<IdentityObject>(MockSession.Object);

            var data = DefaultIdentityObject;

            //  act
            var results = GetBuilderResults<IdentityObject>(b, "BuildInsertQuery", data);

            var sql = "insert into [IDENTITY_OBJECT] ([Name]) values (@Name);select SCOPE_IDENTITY()"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(data.Name, results.Parameters["@Name"].Value);
        }

        [TestMethod]
        public void PersistenceBuilder_Should_Insert_Identity_Correctly_For_MySql()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.MySqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetPersistenceBuilder<IdentityObject>(MockSession.Object);

            var data = DefaultIdentityObject;

            //  act
            var results = GetBuilderResults<IdentityObject>(b, "BuildInsertQuery", data);

            var sql = "insert into `IDENTITY_OBJECT` (`Name`) values (@Name);select LAST_INSERT_ID()"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(data.Name, results.Parameters["@Name"].Value);
        }

        [TestMethod]
        public void PersistenceBuilder_Should_Insert_Identity_Correctly_For_SqlCe()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.SqlCeDriver);

            var b = MockSession.Object.Driver.Dialect.GetPersistenceBuilder<IdentityObject>(MockSession.Object);

            var data = DefaultIdentityObject;

            //  act
            var results = GetBuilderResults<IdentityObject>(b, "BuildInsertQuery", data);

            var sql = "insert into \"IDENTITY_OBJECT\" (\"Name\") values (@Name);select @@IDENTITY"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(data.Name, results.Parameters["@Name"].Value);
        }

        [TestMethod]
        public void PersistenceBuilder_Should_Insert_Identity_Correctly_For_PostgreSql()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.PostgreSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetPersistenceBuilder<IdentityObject>(MockSession.Object);

            var data = DefaultIdentityObject;

            //  act
            var results = GetBuilderResults<IdentityObject>(b, "BuildInsertQuery", data);

            var sql = "insert into \"IDENTITY_OBJECT\" (\"Name\") values (@Name);select lastval()"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(data.Name, results.Parameters["@Name"].Value);
        }

        [TestMethod]
        public void PersistenceBuilder_Should_Insert_Identity_Correctly_For_SQLite()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.SQLiteDriver);

            var b = MockSession.Object.Driver.Dialect.GetPersistenceBuilder<IdentityObject>(MockSession.Object);

            var data = DefaultIdentityObject;

            //  act
            var results = GetBuilderResults<IdentityObject>(b, "BuildInsertQuery", data);

            var sql = "insert into \"IDENTITY_OBJECT\" (\"Name\") values (@Name);select last_insert_rowid()"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(data.Name, results.Parameters["@Name"].Value);
        }

        [TestMethod]
        public void PersistenceBuilder_Should_Insert_Identity_Correctly_For_Firebird()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.FirebirdSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetPersistenceBuilder<IdentityObject>(MockSession.Object);

            var data = DefaultIdentityObject;

            //  act
            var results = GetBuilderResults<IdentityObject>(b, "BuildInsertQuery", data);

            var sql = "insert into \"IDENTITY_OBJECT\" (\"Name\") values (@Name) returning \"id\""
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(data.Name, results.Parameters["@Name"].Value);
        }

        [TestMethod]
        public void InsertBuilder_Should_Insert_Composite_Correctly_For_AnsiSql()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetPersistenceBuilder<CompositeKeyObject>(MockSession.Object);

            var data = DefaultCompositeKeyObject;

            //  act
            var results = GetBuilderResults<CompositeKeyObject>(b, "BuildInsertQuery", data);

            var sql = "insert into [COMPOSITE_KEY_OBJECT] ([parent_id], [this_id], [Name]) "
                + "values (@ParentID, @ThisID, @Name)"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(3, results.Parameters.Count);
            Assert.AreEqual(data.ParentID, results.Parameters["@ParentID"].Value);
            Assert.AreEqual(data.ThisID, results.Parameters["@ThisID"].Value);
            Assert.AreEqual(data.Name, results.Parameters["@Name"].Value);
        }

        #endregion

        #region Update Persistence Tests

        [TestMethod]
        public void UpdateBuilder_Should_Update_Primary_Key_Correctly_For_AnsiSql()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetPersistenceBuilder<CompositeKeyObject>(MockSession.Object);

            var data = DefaultCompositeKeyObject;

            //  act
            var results = GetBuilderResults<CompositeKeyObject>(b, "BuildUpdateQuery", data);

            var sql = "update [COMPOSITE_KEY_OBJECT] "
                + "set [Name] = @Name "
                + "where [parent_id] = @ParentID and [this_id] = @ThisID"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(3, results.Parameters.Count);
            Assert.AreEqual(data.ParentID, results.Parameters["@ParentID"].Value);
            Assert.AreEqual(data.ThisID, results.Parameters["@ThisID"].Value);
            Assert.AreEqual(data.Name, results.Parameters["@Name"].Value);
        }

        [TestMethod]
        public void UpdateBuilder_Should_Update_Complex_Properties_Correctly_For_AnsiSql()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetPersistenceBuilder<ComplexObject>(MockSession.Object);

            var data = DefaultComplexObject;

            data.NullableID = null;

            //  act
            var results = GetBuilderResults<ComplexObject>(b, "BuildUpdateQuery", data);

            var sql = "update [COMPLEX_OBJECT] "
                + "set [NullableID] = @NullableID, [Name] = @Name, [YesNo] = @YesNo, [ReadOnly] = @ReadOnly "
                + "where [id] = @ID"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(5, results.Parameters.Count);
            Assert.AreEqual(data.ID, results.Parameters["@ID"].Value);
            Assert.AreEqual(DBNull.Value, results.Parameters["@NullableID"].Value);
            Assert.AreEqual(data.Name, results.Parameters["@Name"].Value);
            Assert.AreEqual(data.YesNo ? "Y" : "N", results.Parameters["@YesNo"].Value);
            Assert.AreEqual(data.ReadOnly, results.Parameters["@ReadOnly"].Value);
        }

        #endregion

        #region Delete Persistence Tests

        [TestMethod]
        public void DeleteBuilder_Should_Delete_Primary_Key_Correctly_For_AnsiSql()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetPersistenceBuilder<CompositeKeyObject>(MockSession.Object);

            var data = DefaultCompositeKeyObject;

            //  act
            var results = GetBuilderResults<CompositeKeyObject>(b, "BuildDeleteQuery", data);

            var sql = "delete from [COMPOSITE_KEY_OBJECT] "
                + "where [parent_id] = @ParentID and [this_id] = @ThisID"
                ;

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(2, results.Parameters.Count);
            Assert.AreEqual(data.ParentID, results.Parameters["@ParentID"].Value);
            Assert.AreEqual(data.ThisID, results.Parameters["@ThisID"].Value);
        }

        #endregion

        private IBuilderResults GetBuilderResults<T>(IPersistenceBuilder<T> b, string invokeMethod, object data) where T : class
        {
            Type persistenceBuilder = typeof(ISession).Assembly.GetType("Yamor.Builders.PersistenceBuilder`1");

            Type builder = persistenceBuilder.MakeGenericType(typeof(T));

            PrivateObject po = new PrivateObject(b, new PrivateType(builder));

            //  act
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            return po.Invoke(invokeMethod, flags, new[] { typeof(object) }, new[] { data }) as IBuilderResults;
        }
    }
}
