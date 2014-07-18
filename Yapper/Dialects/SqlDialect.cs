using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnsureThat;

namespace Yapper.Dialects
{
    /// <summary>
    /// Provides functionality common to <b>most</b> databases
    /// </summary>
    public abstract class SqlDialect : ISqlDialect
    {
        //public string QueryString(string selection, string source, string conditions, string order, string grouping, string having)
        //{
        //    return string.Format("SELECT {0} FROM {1} {2} {3} {4} {5}",
        //                         selection, source, conditions, order, grouping, having);
        //}

        #region ISqlDialect Members

        public virtual string StatementSeparator { get { return ";"; } }

        public virtual string IdentifierSeparator { get { return "."; } }

        public virtual string LeftDelimiter { get { return "["; } }

        public virtual string RightDelimiter { get { return "]"; } }

        public virtual string ParameterIdentifier { get { return "@"; } }

        #endregion

        #region Methods

        /// <summary>
        /// Escapes the specified SQL using the left and right delimiters.
        /// </summary>
        /// <param name="dialect"></param>
        /// <param name="id">The SQL to be escaped.</param>
        /// <returns>The escaped SQL.</returns>
        /// <exception cref="ArgumentNullException">Thrown if sql is null.</exception>
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