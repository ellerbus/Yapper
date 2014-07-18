using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yapper.Mappers;

namespace Yapper.Tests.Mappers
{
    [TestClass]
    public class ObjectMapTests
    {
        [Table("DO")]
        public class DataObject_OptIn
        {
            [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int ID { get; set; }

            [Column]
            public string Name { get; set; }

            public bool Modified { get; set; }
        }
        
        public class DataObject
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void ObjectMap_Should_ListProperties()
        {
            ObjectMap optinMap = ObjectMapCollection.Default.GetMapFor<DataObject_OptIn>();

            Assert.AreEqual(2, optinMap.Properties.Count);

            ObjectMap doMap = ObjectMapCollection.Default.GetMapFor<DataObject>();

            Assert.AreEqual(2, doMap.Properties.Count);
        }

        [TestMethod]
        public void ObjectMap_ShouldHave_Identity()
        {
            ObjectMap optinMap = ObjectMapCollection.Default.GetMapFor<DataObject_OptIn>();

            Assert.IsTrue(optinMap.HasIdentity);
        }

        [TestMethod]
        public void ObjectMap_ShouldHave_SourceName()
        {
            ObjectMap optinMap = ObjectMapCollection.Default.GetMapFor<DataObject_OptIn>();

            Assert.AreEqual("DO", optinMap.SourceName);
        
            ObjectMap doMap = ObjectMapCollection.Default.GetMapFor<DataObject>();

            Assert.AreEqual("DataObject", doMap.SourceName);
        }
    }
}
