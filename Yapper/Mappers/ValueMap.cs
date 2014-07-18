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
    public class ValueMap
    {
        #region Members

        private IDictionary<object, object> _toSqlValues;
        private IDictionary<object, object> _toClrValues;
        private Type _enumType;

        #endregion

        #region Constructors

        private ValueMap()
        {
            _toSqlValues = new Dictionary<object, object>(8);
            _toClrValues = new Dictionary<object, object>(8);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        public ValueMap(BooleanValueMapAttribute map)
            : this()
        {
            Ensure.That(map).IsNotNull();

            MapBoolean(map.TrueValue, map.FalseValue);

            ValidateSqlValueTypes();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumType"></param>
        public ValueMap(Type enumType)
            : this()
        {
            Ensure.That(enumType).IsNotNull();
            Ensure.That(enumType.IsEnum).IsTrue();

            MapEnum(enumType);

            ValidateSqlValueTypes();
        }

        #endregion

        #region Methods

        private void MapBoolean(object trueValue, object falseValue)
        {
            MapValue(true, trueValue);
            MapValue(false, falseValue);
        }

        private void MapEnum(Type enumType)
        {
            _enumType = enumType;

            foreach (string nm in Enum.GetNames(enumType))
            {
                MemberInfo enumMember = enumType.GetMember(nm).First();

                EnumValueMapAttribute enumMap = enumMember.GetCustomAttribute<EnumValueMapAttribute>(true);

                object value = Enum.Parse(enumType, nm);

                if (enumMap == null)
                {
                    MapValue(value, (int)value);
                }
                else
                {
                    MapValue(value, enumMap.Value);
                }
            }
        }

        private void MapValue(object clr, object sql)
        {
            _toSqlValues.Add(clr, sql);
            _toClrValues.Add(sql, clr);
        }

        private void ValidateSqlValueTypes()
        {
            IList<Type> types = _toClrValues.Keys.Select(x => x.GetType()).ToList();

            if (types.Count > 1)
            {
                for (int i = 1; i < types.Count; i++)
                {
                    Type a = types[i - 0];
                    Type b = types[i - 1];

                    if (a != b)
                    {
                        throw new InvalidOperationException(
                            "Invalid Enum Map, Type Mismatch using {0} ({1} != {2})"
                            .FormatArgs(_enumType.FullName, a.FullName, b.FullName)
                            );
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clr"></param>
        /// <returns></returns>
        public object ToSql(object clr)
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
        public object ToClr(object sql)
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
        /// The CLR represented type going 'to' the database
        /// </summary>
        public Type ToType { get { return _toSqlValues.First().Value.GetType(); } }

        #endregion
    }
}