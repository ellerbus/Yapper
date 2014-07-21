using System;

namespace Yapper.Dialects
{
    /// <summary>
    /// SQL dialect provides db specific functionality related to db specific SQL syntax
    /// </summary>
    public interface ISqlDialect
    {
        string SelectStatement(string selection, string source, string conditions, string order, string grouping, int limit, int offset, int fetch);

        /// <summary>
        /// Gets the delimiter used on to separate identifiers
        /// </summary>
        string StatementSeparator { get; }

        /// <summary>
        /// Gets the delimiter used on to separate identifiers
        /// </summary>
        string IdentifierSeparator { get; }

        /// <summary>
        /// Gets the delimiter used on the left hand side to escape an SQL identifier.
        /// </summary>
        string LeftDelimiter { get; }

        /// <summary>
        /// Gets the delimiter used on the right hand side to escape an SQL identifier.
        /// </summary>
        string RightDelimiter { get; }

        /// <summary>
        /// Gets the parameter value used to identify parameters
        /// </summary>
        string ParameterIdentifier { get; }

        /// <summary>
        /// Gets the identity value on insert
        /// </summary>
        string SelectIdentity { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string EscapeIdentifier(string id);
    }
}
