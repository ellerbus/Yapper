using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yapper.Mappers;

namespace Yapper.Tests.Mappers
{
    [TestClass]
    public class PropertyMapTests
    {
        [Table("DO")]
        public class DataObject
        {
            [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int ID { get; set; }
            [Column]
            public int? NullableID { get; set; }
            [Column]
            public string Name { get; set; }
            [Column, BooleanValueMap("Y", "N")]
            public bool YesNo { get; set; }
            [Column, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public string Computed { get; set; }
            [Column]
            public string ReadOnly { get { return ""; } }
        }

        [TestMethod]
        public void PropertyMap_ShouldHave_SourceName()
        {
            Assert.AreEqual("id", GetPropertyMap("ID").SourceName);
            Assert.AreEqual("Name", GetPropertyMap("Name").SourceName);
        }

        [TestMethod]
        public void PropertyMap_Should_UsesValueMap()
        {
            Assert.IsFalse(GetPropertyMap("ID").UsesValueMap);
            Assert.IsTrue(GetPropertyMap("YesNo").UsesValueMap);
        }

        [TestMethod]
        public void PropertyMap_Should_Be_PrimaryKey()
        {
            Assert.IsTrue(GetPropertyMap("ID").IsPrimaryKey);
            Assert.IsFalse(GetPropertyMap("YesNo").IsPrimaryKey);
        }

        [TestMethod]
        public void PropertyMap_Should_Be_DatabaseGenerated()
        {
            Assert.IsTrue(GetPropertyMap("ID").IsDatabaseGenerated);
            Assert.IsFalse(GetPropertyMap("Name").IsDatabaseGenerated);
            Assert.IsTrue(GetPropertyMap("Computed").IsDatabaseGenerated);
            Assert.IsFalse(GetPropertyMap("ReadOnly").IsDatabaseGenerated);
        }

        [TestMethod]
        public void PropertyMap_Should_Be_Identity()
        {
            Assert.IsTrue(GetPropertyMap("ID").IsIdentity);
            Assert.IsFalse(GetPropertyMap("Name").IsIdentity);
        }

        [TestMethod]
        public void PropertyMap_Should_Be_Computed()
        {
            Assert.IsFalse(GetPropertyMap("ID").IsComputed);
            Assert.IsFalse(GetPropertyMap("Name").IsComputed);
            Assert.IsTrue(GetPropertyMap("Computed").IsComputed);
            Assert.IsFalse(GetPropertyMap("ReadOnly").IsComputed);
        }

        [TestMethod]
        public void PropertyMap_Should_Be_ReadOnly()
        {
            Assert.IsFalse(GetPropertyMap("ID").IsReadOnly);
            Assert.IsFalse(GetPropertyMap("Name").IsReadOnly);
            Assert.IsFalse(GetPropertyMap("Computed").IsReadOnly);
            Assert.IsTrue(GetPropertyMap("ReadOnly").IsReadOnly);
        }

        [TestMethod]
        public void PropertyMap_Should_Be_Nullable()
        {
            Assert.IsFalse(GetPropertyMap("ID").IsNullable);
            Assert.IsTrue(GetPropertyMap("Name").IsNullable);
            Assert.IsTrue(GetPropertyMap("NullableID").IsNullable);
        }

        [TestMethod]
        public void PropertyMap_Should_Be_Selectable()
        {
            Assert.IsTrue(GetPropertyMap("ID").IsSelectable);
            Assert.IsTrue(GetPropertyMap("Name").IsSelectable);
            Assert.IsTrue(GetPropertyMap("NullableID").IsSelectable);
            Assert.IsFalse(GetPropertyMap("ReadOnly").IsSelectable);
        }

        [TestMethod]
        public void PropertyMap_Should_Be_Insertable()
        {
            Assert.IsFalse(GetPropertyMap("ID").IsInsertable);
            Assert.IsTrue(GetPropertyMap("Name").IsInsertable);
            Assert.IsTrue(GetPropertyMap("NullableID").IsInsertable);
            Assert.IsFalse(GetPropertyMap("Computed").IsInsertable);
        }

        [TestMethod]
        public void PropertyMap_Should_Be_Updatable()
        {
            Assert.IsFalse(GetPropertyMap("ID").IsUpdatable);
            Assert.IsTrue(GetPropertyMap("Name").IsUpdatable);
            Assert.IsTrue(GetPropertyMap("NullableID").IsUpdatable);
            Assert.IsFalse(GetPropertyMap("Computed").IsUpdatable);
        }
    
        private PropertyMap GetPropertyMap(string name)
        {
            var map = ObjectMapCollection.Default.GetMapFor<DataObject>();

            var p = map.Properties[name];

            return p;
        }
    }
}
