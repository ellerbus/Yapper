using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Augment;
using Yapper.Dialects;

namespace Yapper.Builders
{
    sealed class SqlCombiner : ISqlQuery
    {
        #region Members

        private const string Alphabet = "abcdefghijklmnopqrstuvwxyz";

        private ISqlDialect _dialect;
        private int _index;

        #endregion

        #region Constructor

        public SqlCombiner(ISqlDialect dialect)
        {
            _dialect = dialect;

            Parameters = new ExpandoObject();
        }

        #endregion

        #region Methods

        public void Combine(ISqlQuery q0, ISqlQuery q1)
        {
            Add(q0);
            Add(q1);
        }

        public void Combine(ISqlQuery q0, ISqlQuery q1, params ISqlQuery[] queries)
        {
            Add(q0);
            Add(q1);

            foreach (ISqlQuery q in queries)
            {
                Add(q);
            }
        }


        private void Add(ISqlQuery sql)
        {
            if (Query.IsNotEmpty())
            {
                Query += _dialect.StatementSeparator + Environment.NewLine;
            }

            UpdateQuery(sql.Query);
            UpdateParameters(sql.Parameters);

            _index += 1;
        }

        private void UpdateQuery(string sql)
        {
            string pattern = @"(?<p>" + _dialect.ParameterIdentifier + @"p[0-9]+)";

            sql = Regex.Replace(sql, pattern, "${1}" + Alphabet[_index]);

            Query +=sql;
        }

        private void UpdateParameters(ExpandoObject expandoObject)
        {
            IDictionary<string, object> expandoParms = expandoObject as IDictionary<string, object>;

            IDictionary<string, object> parms = Parameters as IDictionary<string, object>;

            foreach (KeyValuePair<string, object> item in expandoParms)
            {
                parms.Add(item.Key + Alphabet[_index], item.Value);
            }
        }

        #endregion

        #region ISqlQuery Members

        public string Query { get; private set; }

        public ExpandoObject Parameters { get; private set; }

        #endregion
    }
}
