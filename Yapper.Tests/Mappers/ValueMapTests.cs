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
            [EnumValueMap("N")]
            None,
            [EnumValueMap("F")]
            First
        }

        public enum InvalidEnumMap
        {
            [EnumValueMap(0)]
            None,
            [EnumValueMap("F")]
            First
        }

        [TestMethod]
        public void ValueMap_Should_MapBoolean()
        {
            //  arrange
            BooleanValueMapAttribute map = new BooleanValueMapAttribute("Y", "N");

            ValueMap vmap = new ValueMap(map);

            //  assert/act
            Assert.AreEqual("Y", vmap.ToSql(true));
            Assert.AreEqual("N", vmap.ToSql(false));
        }

        [TestMethod]
        public void ValueMap_Should_MapDefaultEnum()
        {
            //  arrange
            ValueMap vmap = new ValueMap(typeof(DefaultEnum));

            //  assert/act
            Assert.AreEqual(0, vmap.ToSql(DefaultEnum.None));
            Assert.AreEqual(1, vmap.ToSql(DefaultEnum.First));
        }

        [TestMethod]
        public void ValueMap_Should_MapEnum()
        {
            //  arrange
            ValueMap vmap = new ValueMap(typeof(StringEnumMap));

            //  assert/act
            Assert.AreEqual("N", vmap.ToSql(StringEnumMap.None));
            Assert.AreEqual("F", vmap.ToSql(StringEnumMap.First));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ValueMap_Should_ThrowException()
        {
            //  arrange
            ValueMap vmap = new ValueMap(typeof(InvalidEnumMap));
        }
    }
}
