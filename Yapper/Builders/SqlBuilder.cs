using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using EnsureThat;
using Augment;
using Yapper.Dialects;
using Yapper.Mappers;
using System.Dynamic;

namespace Yapper.Builders
{
    /// <summary>
    /// Base class for building sql statements
    /// </summary>
    internal abstract class SqlBuilder : ISqlQuery
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialect"></param>
        /// <param name="mapOfType"></param>
        protected SqlBuilder(ISqlDialect dialect, Type mapOfType)
        {
            Ensure.That(dialect).IsNotNull();

            Dialect = dialect;

            ObjectMap = ObjectMapCollection.Default.GetMapFor(mapOfType);

            BuilderParameters = new Dictionary<string, object>();

            WhereClause = new StringBuilder();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        protected string GetPropertyName<T, TField>(Expression<Func<T, TField>> field)
        {
            Ensure.That(field).IsNotNull();

            MemberExpression expr = null;

            if (field.Body is MemberExpression)
            {
                expr = field.Body as MemberExpression;
            }
            else if (field.Body is UnaryExpression)
            {
                expr = (field.Body as UnaryExpression).Operand as MemberExpression;
            }
            else
            {
                string message = "Expression '{0}' not supported.".FormatArgs(field);

                throw new ArgumentException(message, "Field");
            }

            return expr.Member.Name;
        }

        protected void RequiresPrimaryKey()
        {
            if (!ObjectMap.HasPrimaryKey)
            {
                throw new InvalidOperationException(
                    "Missing Primary Key for '{0}'".FormatArgs(typeof(KeyAttribute).FullName)
                    );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        protected string GetKey(string prefix)
        {
            return "{0}:{1}:{2}".FormatArgs(prefix, Dialect.GetType().FullName, ObjectMap.ObjectType.FullName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        protected void AppendPrimaryKeyWhere(StringBuilder sb)
        {
            bool delim = false;

            sb.Append(" where ");

            foreach (PropertyMap p in ObjectMap.Properties.Where(x => x.IsPrimaryKey))
            {
                sb.AppendIf(delim, " and ")
                    .Append(Dialect.EscapeIdentifier(p.SourceName))
                    .Append(" = ")
                    .Append(Dialect.ParameterIdentifier + p.Name)
                    ;

                delim = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parms"></param>
        protected void AppendParameters(IDictionary<string, object> parms)
        {
            foreach (var item in parms)
            {
                BuilderParameters.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="data"></param>
        protected void AppendParameters(IEnumerable<PropertyMap> properties, object data)
        {
            foreach (PropertyMap p in properties)
            {
                AppendParameter(p, p.Name, p.GetValue(data));
            }
        }

        /// <summary>
        /// Uses a naming convention p0, p1, ... pN
        /// </summary>
        /// <param name="pm"></param>
        /// <param name="value"></param>
        /// <returns>Parameter Name to be used in SQL</returns>
        protected string AppendParameter(PropertyMap pm, object value)
        {
            string nm = "p{0}".FormatArgs(BuilderParameters.Count);

            return AppendParameter(pm, nm, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pm"></param>
        /// <param name="nm"></param>
        /// <param name="value"></param>
        /// <returns>Parameter Name used in SQL Statement</returns>
        protected string AppendParameter(PropertyMap pm, string nm, object value)
        {
            object sqlvalue = pm.ConvertToSqlValue(value);

            string name = Dialect.ParameterIdentifier + nm;

            BuilderParameters.Add(nm, sqlvalue);

            return name;
        }

        #endregion

        #region Where Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        protected void AppendWhere(object values, bool primaryKeyOnly = false)
        {
            foreach (PropertyInfo p in values.GetType().GetProperties())
            {
                PropertyMap pm = ObjectMap.Properties[p.Name];

                bool add = true;

                if (primaryKeyOnly)
                {
                    add = pm.IsPrimaryKey;
                }

                if (add)
                {
                    WhereClause.AppendIf(WhereClause.Length > 0, " and ")
                        .Append(Dialect.EscapeIdentifier(pm.SourceName))
                        .Append(" = ")
                        .Append(AppendParameter(pm, p.GetValue(values)))
                        ;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        protected void AppendWhere(Expression where)
        {
            Ensure.That(where).IsNotNull();

            ExpressionBuilder e = new ExpressionBuilder(Dialect, BuilderParameters.Count);

            e.Compile(where);

            if (e.Query.IsNotEmpty())
            {
                WhereClause.AppendIf(WhereClause.Length > 0, " and ").Append(e.Query);

                AppendParameters(e.Parameters);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public ObjectMap ObjectMap { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ISqlDialect Dialect { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Query { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual ExpandoObject Parameters { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        protected IDictionary<string, object> BuilderParameters { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        protected StringBuilder WhereClause { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        protected StringBuilder SourceClause { get; private set; }

        #endregion
    }
}