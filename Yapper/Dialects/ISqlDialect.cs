using System;

namespace Yapper.Dialects
{
    /// <summary>
    /// SQL dialect provides db specific functionality related to db specific SQL syntax
    /// </summary>
    public interface ISqlDialect
    {
        /// <summary>
        /// Provides the items used to construct a Select statement given a <see cref="ISqlDialect"/>
        /// </summary>
        /// <param name="selection">Select Clause</param>
        /// <param name="source">From Clause</param>
        /// <param name="conditions">Where Clause</param>
        /// <param name="order">Order By Clause</param>
        /// <param name="grouping">Group By Clause</param>
        /// <param name="limit">Top Clause</param>
        /// <param name="offset">Page offset index (zero based)</param>
        /// <param name="fetch">Page number of records to return</param>
        /// <returns>A SQL Select statement specific to a given <see cref="ISqlDialect"/></returns>
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
