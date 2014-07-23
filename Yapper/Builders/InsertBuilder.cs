using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yapper.Dialects;
using Yapper.Mappers;
using Augment;

namespace Yapper.Builders
{
    /// <summary>
    /// Implementation for InsertBuilder
    /// </summary>
    sealed class InsertBuilder<T> : SqlBuilder
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialect"></param>
        /// <param name="item"></param>
        public InsertBuilder(ISqlDialect dialect, T item)
            : this(dialect)
        {
            InsertClause.Append("insert into ").Append(Dialect.EscapeIdentifier(ObjectMap.SourceName));

            IList<PropertyMap> properties = ObjectMap.Properties.Where(x => x.IsInsertable).ToList();

            IList<string> fields = properties.Select(x => Dialect.EscapeIdentifier(x.SourceName)).ToList();
            IList<string> parms = properties.Select(x => AppendParameter(x, x.GetValue(item))).ToList();

            InsertClause.Append(" (")
                .Append(fields.Join(", "))
                .Append(") values (")
                .Append(parms.Join(", "))
                .Append(")")
                ;

            if (ObjectMap.HasIdentity)
            {
                string columnName = Dialect.EscapeIdentifier(ObjectMap.Identity.SourceName);

                InsertClause.Append(Dialect.SelectIdentity.FormatArgs(columnName));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialect"></param>
        public InsertBuilder(ISqlDialect dialect)
            : base(dialect, typeof(T))
        {
            InsertClause = new StringBuilder();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetQuery()
        {
            Parameters = BuilderParameters.ToExpando();

            return InsertClause.ToString();
        }

        #endregion

        #region Properties

        private StringBuilder InsertClause { get; set; }

        #endregion
    }
}