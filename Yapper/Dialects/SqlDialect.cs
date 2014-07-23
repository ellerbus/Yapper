using System;
using System.Linq;
using EnsureThat;
using Augment;

namespace Yapper.Dialects
{
    /// <summary>
    /// Provides functionality common to <b>most</b> databases
    /// </summary>
    public abstract class SqlDialect : ISqlDialect
    {
        #region ISqlDialect Members

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
        public virtual string SelectStatement(string selection, string source, string conditions, string order, string grouping, int limit, int offset, int fetch)
        {
            Ensure.That(limit)
                .WithExtraMessageOf(() => "Top (or limit) is not supported")
                .Is(0);

            Ensure.That(offset)
                .WithExtraMessageOf(() => "Paging is not supported")
                .Is(0);

            Ensure.That(fetch)
                .WithExtraMessageOf(() => "Paging is not supported")
                .Is(0);

            return "select {0} from {1} {2} {3} {4}"
                .FormatArgs(selection, source, conditions, order, grouping);
        }

        /// <summary>
        /// Gets the delimiter used on to separate identifiers
        /// </summary>
        public virtual string StatementSeparator { get { return ";"; } }

        /// <summary>
        /// Gets the delimiter used on to separate identifiers
        /// </summary>
        public virtual string IdentifierSeparator { get { return "."; } }

        /// <summary>
        /// Gets the delimiter used on the left hand side to escape an SQL identifier.
        /// </summary>
        public virtual string LeftDelimiter { get { return "["; } }

        /// <summary>
        /// Gets the delimiter used on the right hand side to escape an SQL identifier.
        /// </summary>
        public virtual string RightDelimiter { get { return "]"; } }

        /// <summary>
        /// Gets the parameter value used to identify parameters
        /// </summary>
        public virtual string ParameterIdentifier { get { return "@"; } }

        /// <summary>
        /// Gets the identity value on insert
        /// </summary>
        public abstract string SelectIdentity { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Escapes the specified SQL using the left and right delimiters.
        /// </summary>
        /// <param name="id">Identifier to be escaped with left/right delimiters</param>
        /// <returns>Escaped Identifier String</returns>
        public string EscapeIdentifier(string id)
        {
            Ensure.That(id).IsNotNull();

            if (IsEscaped(id))
            {
                return id;
            }

            if (!id.Contains(IdentifierSeparator))
            {
                return LeftDelimiter + id + RightDelimiter;
            }

            string[] pieces = id.Split(IdentifierSeparator[0]);

            return string.Join(IdentifierSeparator, pieces.Select(s => LeftDelimiter + s + RightDelimiter));
        }

        /// <summary>
        /// Determines whether the specified SQL is escaped.
        /// </summary>
        /// <param name="identifier">The SQL to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified SQL is escaped; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsEscaped(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return false;
            }

            return identifier.StartsWith(LeftDelimiter, StringComparison.OrdinalIgnoreCase)
                && identifier.EndsWith(RightDelimiter, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}