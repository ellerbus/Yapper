using System.Text.RegularExpressions;
using FluentAssertions;

namespace Yapper.Tests
{
    public class BaseStatementBuilder
    {
        private static readonly Regex _cleaner = new Regex(@"\s+");

        protected void AssertSql(string actual, string expected)
        {
            actual = _cleaner.Replace(actual, " ").Trim();

            actual.Should().Be(expected);
        }
    }
}