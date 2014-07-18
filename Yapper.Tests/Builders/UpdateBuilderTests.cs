using System;
using System.Linq.Expressions;
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
    public class UpdateBuilderTests
    {
        #region Setup

        private IFixture AutoFixture { get; set; }
        private Mock<ISession> MockSession { get; set; }

        private CompositeKeyObject DefaultCompositeKeyObject { get; set; }
        private ComplexObject DefaultComplexObject { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            AutoFixture = new Fixture().Customize(new AutoMoqCustomization());

            MockSession = AutoFixture.Freeze<Mock<ISession>>();

            DefaultCompositeKeyObject = AutoFixture.Create<CompositeKeyObject>();
            DefaultComplexObject = AutoFixture.Create<ComplexObject>();
        }

        #endregion

        [TestMethod]
        public void UpdateBuilder_Should_Update_All_For_AnsiSql()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetUpdateBuilder<ComplexObject>(MockSession.Object);

            b.Set(x => x.ID, 999);

            //  act
            var results = GetBuilderResults<ComplexObject>(b);

            string sql = "update [COMPLEX_OBJECT] set [id] = @p0";

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(999, results.Parameters["@p0"].Value);
        }

        [TestMethod]
        public void UpdateBuilder_Should_Update_All_Incremental_For_AnsiSql()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetUpdateBuilder<ComplexObject>(MockSession.Object);

            b.Set(x => x.ID, x => x.ID + 1);

            //  act
            var results = GetBuilderResults<ComplexObject>(b);

            string sql = "update [COMPLEX_OBJECT] set [id] = ([id] + @p0)";

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(1, results.Parameters.Count);
            Assert.AreEqual(1, results.Parameters["@p0"].Value);
        }

        [TestMethod]
        public void UpdateBuilder_Should_Update_Many_For_AnsiSql()
        {
            //  arrange
            Expression<Func<ComplexObject, bool>> where = x => x.ID > 0;

            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetUpdateBuilder<ComplexObject>(MockSession.Object);

            b.Set(x => x.YesNo, false);

            b.Where(where);

            //  act
            var results = GetBuilderResults<ComplexObject>(b);

            string sql = "update [COMPLEX_OBJECT] set [YesNo] = @p0 where ([id] > @p1)";

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(2, results.Parameters.Count);
            Assert.AreEqual("N", results.Parameters["@p0"].Value);
            Assert.AreEqual(0, results.Parameters["@p1"].Value);
        }

        [TestMethod]
        public void UpdateBuilder_Should_Update_Many_For_AnonymousObject()
        {
            //  arrange
            MockSession.SetupGet(x => x.Driver).Returns(Helper.MsSqlDriver);

            var b = MockSession.Object.Driver.Dialect.GetUpdateBuilder<IdentityObject>(MockSession.Object);

            b.Set(new { Name = "abc" }).Where(new { IdentityID = 123 });

            //  act
            var results = GetBuilderResults<IdentityObject>(b);

            string sql = "update [IDENTITY_OBJECT] set [Name] = @p0 where [id] = @p1";

            //  assert
            Assert.AreEqual(sql, results.SqlQuery);
            Assert.AreEqual(2, results.Parameters.Count);
            Assert.AreEqual("abc", results.Parameters["@p0"].Value);
            Assert.AreEqual(123, results.Parameters["@p1"].Value);
        }

        private IBuilderResults GetBuilderResults<T>(IUpdateBuilder<T> b) where T : class
        {
            Type persistenceBuilder = typeof(ISession).Assembly.GetType("Yamor.Builders.UpdateBuilder`1");

            Type builder = persistenceBuilder.MakeGenericType(typeof(T));

            PrivateObject po = new PrivateObject(b, new PrivateType(builder));

            //  act
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            return po.Invoke("BuildQuery", flags) as IBuilderResults;
        }
    }
}
