using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Yapper.Core
{
    class CustomTypeMap : SqlMapper.ITypeMap
    {
        #region Members

        private readonly Dictionary<string, SqlMapper.IMemberMap> members = new Dictionary<string, SqlMapper.IMemberMap>(StringComparer.OrdinalIgnoreCase);

        private readonly SqlMapper.ITypeMap _originalMap;

        #endregion

        #region Constructors

        public CustomTypeMap(Type type, SqlMapper.ITypeMap originalMap)
        {
            Type = type;

            _originalMap = originalMap;
        }

        #endregion

        #region Methods

        public void MapColumn(string columnName, string memberName)
        {
            members[columnName] = new MemberMap(columnName, Type.GetMember(memberName).Single());
        }

        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            return _originalMap.FindConstructor(names, types);
        }

        public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            return _originalMap.GetConstructorParameter(constructor, columnName);
        }

        public SqlMapper.IMemberMap GetMember(string columnName)
        {
            SqlMapper.IMemberMap map = null;

            if (!members.TryGetValue(columnName, out map))
            {
                //  you might want to return null if
                //  you prefer not to fallback to the
                //  default implementation
                map = _originalMap.GetMember(columnName);
            }

            return map;
        }

        #endregion

        #region Properties

        public Type Type { get; private set; }

        #endregion
    }

    class MemberMap : SqlMapper.IMemberMap
    {
        #region Members

        private readonly MemberInfo _member;

        #endregion

        #region Constructors

        public MemberMap(string columnName, MemberInfo member)
        {
            ColumnName = columnName;

            _member = member;
        }

        #endregion

        #region Properties

        public string ColumnName { get; private set; }

        public FieldInfo Field { get { return _member as FieldInfo; } }

        public Type MemberType
        {
            get
            {
                switch (_member.MemberType)
                {
                    case MemberTypes.Field:
                        return (_member as FieldInfo).FieldType;

                    case MemberTypes.Property:
                        return (_member as PropertyInfo).PropertyType;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public ParameterInfo Parameter { get { return null; } }

        public PropertyInfo Property { get { return _member as PropertyInfo; } }

        #endregion
    }
}