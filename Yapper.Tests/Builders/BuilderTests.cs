using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Yapper.Dialects;
using Yapper.Tests.DataObjects;

namespace Yapper.Tests.Builders
{
    public abstract class BuilderTests
    {
        #region Setup

        private IFixture AutoFixture { get; set; }

        protected IdentityObject DefaultIdentityObject { get; private set; }
        protected CompositeKeyObject DefaultCompositeKeyObject { get; private set; }
        protected ComplexObject DefaultComplexObject { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            AutoFixture = new Fixture().Customize(new AutoMoqCustomization());

            DefaultIdentityObject = AutoFixture.Create<IdentityObject>();
            DefaultCompositeKeyObject = AutoFixture.Create<CompositeKeyObject>();
            DefaultComplexObject = AutoFixture.Create<ComplexObject>();

            SetDialect(new SqlServerDialect());
        }

        protected void SetDialect(ISqlDialect dialect)
        {
            FieldInfo fi = typeof(Sql).GetField("Dialect", BindingFlags.Static | BindingFlags.NonPublic);

            fi.SetValue(null, dialect);
        }

        #endregion
    }
}
