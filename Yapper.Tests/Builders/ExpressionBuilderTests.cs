using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yapper.Builders;
using Yapper.Dialects;
using Yapper.Mappers;

namespace Yapper.Tests.Building
{
    [TestClass]
    public class ExpressionBuilderTests
    {
        private static readonly Regex _alias = new Regex(@"\[t[0-9]+\]", RegexOptions.Compiled);

        public enum MyEnum
        {
            [EnumValueMap("AA")]
            A,
            [EnumValueMap("BB")]
            B
        }
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
            [Column]
            public MyEnum Enum { get; set; }
            [Column]
            public string Computed { get { return ""; } }
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateConstant()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.ID == 999;

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("([id] = @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual(999, parameters["p0"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateBoolean_True()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.YesNo;

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("([YesNo] = @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual("Y", parameters["p0"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateBoolean_False()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => !x.YesNo;

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("([YesNo] = @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual("N", parameters["p0"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateEnum()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.Enum == MyEnum.A;

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("([Enum] = @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual("AA", parameters["p0"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateBinary_And()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.ID > 0 && x.ID < 999;

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("(([id] > @p0) and ([id] < @p1))", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));
            Assert.IsTrue(parameters.ContainsKey("p1"));

            Assert.AreEqual(0, parameters["p0"]);
            Assert.AreEqual(999, parameters["p1"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateBinary_Or()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.ID > 0 || x.ID < 999;

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("(([id] > @p0) or ([id] < @p1))", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));
            Assert.IsTrue(parameters.ContainsKey("p1"));

            Assert.AreEqual(0, parameters["p0"]);
            Assert.AreEqual(999, parameters["p1"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateBinary_Math()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.ID + x.NullableID > 0;

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("(([id] + [NullableID]) > @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual(0, parameters["p0"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateNull()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.NullableID == null;

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("([NullableID] is null)", sql);

            Assert.AreEqual(0, parameters.Count);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateString_Trim()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.Name.Trim() == "A";

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("(ltrim(rtrim([Name])) = @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual("A", parameters["p0"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateString_TrimStart()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.Name.TrimStart() == "A";

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("(ltrim([Name]) = @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual("A", parameters["p0"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateString_TrimEnd()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.Name.TrimEnd() == "A";

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("(rtrim([Name]) = @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual("A", parameters["p0"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateString_Upper()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.Name.ToUpper() == "A";

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("(ucase([Name]) = @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual("A", parameters["p0"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateString_Lower()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.Name.ToLower() == "A";

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("(lcase([Name]) = @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual("A", parameters["p0"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateString_Replace()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.Name.Replace("X", "A") == "A";

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("(replace([Name], @p0, @p1) = @p2)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));
            Assert.IsTrue(parameters.ContainsKey("p1"));
            Assert.IsTrue(parameters.ContainsKey("p2"));

            Assert.AreEqual("X", parameters["p0"]);
            Assert.AreEqual("A", parameters["p1"]);
            Assert.AreEqual("A", parameters["p2"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateString_Substring_1()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.Name.Substring(1) == "A";

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("(substr([Name], @p0) = @p1)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));
            Assert.IsTrue(parameters.ContainsKey("p1"));

            Assert.AreEqual(2, parameters["p0"]);
            Assert.AreEqual("A", parameters["p1"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateString_Substring_2()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.Name.Substring(1, 2) == "A";

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("(substr([Name], @p0, @p1) = @p2)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));
            Assert.IsTrue(parameters.ContainsKey("p1"));
            Assert.IsTrue(parameters.ContainsKey("p2"));

            Assert.AreEqual(2, parameters["p0"]);
            Assert.AreEqual(2, parameters["p1"]);
            Assert.AreEqual("A", parameters["p2"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateString_StartsWith()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.Name.StartsWith("A");

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("([Name] like @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual("A%", parameters["p0"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateString_EndsWith()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.Name.EndsWith("A");

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("([Name] like @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual("%A", parameters["p0"]);
        }

        [TestMethod]
        public void ExpressionBuilder_Should_EvaluateString_Contains()
        {
            //  arrange
            var eb = DefaultExpressionBuilder;

            Expression<Func<DataObject, bool>> ex = (x) => x.Name.Contains("A");

            //  act
            eb.Invoke("Compile", ex);

            var sql = eb.GetFieldOrProperty("Query") as string;//_alias.Replace(eb.GetFieldOrProperty("Query") as string, "[t#]");
            var parameters = eb.GetFieldOrProperty("Parameters") as IDictionary<string, object>;

            //  assert
            Assert.AreEqual("([Name] like @p0)", sql);

            Assert.IsTrue(parameters.ContainsKey("p0"));

            Assert.AreEqual("%A%", parameters["p0"]);
        }

        private PrivateObject DefaultExpressionBuilder
        {
            get
            {
                return new PrivateObject("Yapper", "Yapper.Builders.ExpressionBuilder", new SqlServerDialect(), 0);
            }
        }
    }
}