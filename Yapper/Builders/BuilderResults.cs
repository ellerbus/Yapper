using System.Collections.Generic;

namespace Yamor.Builders
{
    /// <summary>
    /// Results from building an expression or SQL Statement
    /// </summary>
    public class BuilderResults : IBuilderResults
    {
        /// <summary>
        /// Parameters if any are contained in the SQL Statement
        /// </summary>
        public IDictionary<string, IParameter> Parameters { get; set; }

        /// <summary>
        /// Resulting SQL
        /// </summary>
        public string SqlQuery { get; set; }
    }
}