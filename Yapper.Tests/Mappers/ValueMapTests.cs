using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yapper.Mappers;

namespace Yapper.Tests.Mappers
{

    [TestClass]
    public class ValueMapTests
    {
        public enum DefaultEnum
        {
            None,
            First
        }

        public enum StringEnumMap
        {
            [EnumMap("N")]
            None,
            [EnumMap("F")]
            First
        }

        [TestMethod]
        public void ValueMap_Should_MapDefaultEnum()
        {
            //  arrange
            EnumMap vmap = new EnumMap(typeof(DefaultEnum));

            //  assert/act
            Assert.AreEqual("0", vmap.ToSql(DefaultEnum.None));
            Assert.AreEqual("1", vmap.ToSql(DefaultEnum.First));
        }

        [TestMethod]
        public void ValueMap_Should_MapEnum()
        {
            //  arrange
            EnumMap vmap = new EnumMap(typeof(StringEnumMap));

            //  assert/act
            Assert.AreEqual("N", vmap.ToSql(StringEnumMap.None));
            Assert.AreEqual("F", vmap.ToSql(StringEnumMap.First));
        }
    }
}
