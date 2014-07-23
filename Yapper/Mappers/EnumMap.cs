using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Augment;

namespace Yapper.Mappers
{
    /// <summary>
    /// Contains a map of values to be used for booleans and enums
    /// </summary>
    public sealed class EnumMap
    {
        #region Members

        private IDictionary<object, string> _toSqlValues;
        private IDictionary<string, object> _toClrValues;
        private Type _enumType;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumType"></param>
        public EnumMap(Type enumType)
        {
            Ensure.That(enumType).IsNotNull();
            Ensure.That(enumType.IsEnum).IsTrue();

            _toSqlValues = new Dictionary<object, string>(8);
            _toClrValues = new Dictionary<string, object>(8);

            MapEnum(enumType);
        }

        #endregion

        #region Methods

        private void MapEnum(Type enumType)
        {
            _enumType = enumType;

            StringBuilder sb = new StringBuilder("case");

            foreach (string nm in Enum.GetNames(enumType))
            {
                MemberInfo enumMember = enumType.GetMember(nm).First();

                EnumMapAttribute enumMap = enumMember.GetCustomAttribute<EnumMapAttribute>(true);

                UsesMap |= enumMap != null;

                object clr = Enum.Parse(enumType, nm);

                string sql = enumMap == null ? ((int)clr).ToString() : enumMap.Value;

                MapValue(clr, sql);

                sb.Append(" when {0} = '").Append(sql).Append("' then ").Append((int)clr);
            }

            FromSql = sb.Append(" else 0 end").ToString();
        }

        private void MapValue(object clr, string sql)
        {
            _toSqlValues.Add(clr, sql);
            _toClrValues.Add(sql, clr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clr"></param>
        /// <returns></returns>
        public string ToSql(object clr)
        {
            Ensure.That(_toSqlValues.ContainsKey(clr))
                .WithExtraMessageOf(() => "Enum Map {0} does not have CLR Value {1} (missing SQL)".FormatArgs(_enumType.Name, clr))
                .IsTrue()
                ;

            return _toSqlValues[clr];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object ToClr(string sql)
        {
            Ensure.That(_toClrValues.ContainsKey(sql))
                .WithExtraMessageOf(() => "Enum Map {0} does not have SQL Value {1} (missing CLR)".FormatArgs(_enumType.Name, sql))
                .IsTrue()
                ;

            return _toClrValues[sql];
        }

        #endregion

        #region Properties

        /// <summary>
        /// The CLR represented type stored in .NET
        /// </summary>
        public Type FromType { get { return _toClrValues.First().Value.GetType(); } }

        /// <summary>
        /// The SQL to use to select values from the database
        /// </summary>
        public string FromSql { get; private set; }
        
        /// <summary>
        /// The CLR represented type going 'to' the database
        /// </summary>
        public Type ToType { get { return _toSqlValues.First().Value.GetType(); } }

        /// <summary>
        /// Whether or not this ENUM has a map to a string
        /// </summary>
        public bool UsesMap { get; private set; }

        #endregion
    }
}