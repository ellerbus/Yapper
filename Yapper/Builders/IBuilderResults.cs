using System.Collections.Generic;

namespace Yamor.Builders
{
    /// <summary>
    /// The interface for a class which yields the query produced by a builder
    /// </summary>
    public interface IBuilderResults
    {
        /// <summary>
        /// Parameters if any are contained in the SQL Statement
        /// </summary>
        IDictionary<string, IParameter> Parameters { get; set; }

        /// <summary>
        /// Resulting SQL
        /// </summary>
        string SqlQuery { get; set; }
    }
}