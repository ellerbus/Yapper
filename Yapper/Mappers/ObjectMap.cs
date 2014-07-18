using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Augment;

namespace Yapper.Mappers
{
    /// <summary>
    /// Maps an object to a table
    /// </summary>
    public class ObjectMap<T> : ObjectMap where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        public ObjectMap() : base(typeof(T)) { }
    }

    /// <summary>
    /// Maps an object to a table
    /// </summary>
    public class ObjectMap
    {
        #region Members

        private static int _aliasNumber;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public ObjectMap(Type type)
        {
            Ensure.That(type).IsNotNull();

            ObjectType = type;

            TableAttribute table = type.GetCustomAttribute<TableAttribute>(true);

            if (table == null)
            {
                SourceName = type.Name;
            }
            else
            {
                SourceName = table.Name ?? type.Name;
            }

            Properties = new PropertyMapCollection(this, table != null);

            Alias = "t{0}".FormatArgs(_aliasNumber);

            _aliasNumber += 1;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name + "=" + Name;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Source Name=SQL
        /// </summary>
        public string SourceName { get; private set; }

        /// <summary>
        /// Name=ObjectProperty Name
        /// </summary>
        public string Name { get { return ObjectType.Name; } }

        /// <summary>
        /// Alias=t0 (future enhancement)
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Type ObjectType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public PropertyMapCollection Properties { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public PropertyMap Identity
        {
            get
            {
                if (HasIdentity && _identity == null)
                {
                    _identity = Properties.First(x => x.IsIdentity && x.IsPrimaryKey);
                }
                return _identity;
            }
        }
        private PropertyMap _identity;

        /// <summary>
        /// 
        /// </summary>
        public bool HasIdentity
        {
            get
            {
                if (_hasIdentity == null)
                {
                    _hasIdentity = Properties.Any(x => x.IsIdentity && x.IsPrimaryKey);
                }
                return _hasIdentity.Value;
            }
        }
        private bool? _hasIdentity;

        /// <summary>
        /// 
        /// </summary>
        public bool HasPrimaryKey
        {
            get
            {
                if (_hasPrimaryKey == null)
                {
                    _hasPrimaryKey = Properties.Any(x => x.IsPrimaryKey);
                }
                return _hasPrimaryKey.Value;
            }
        }
        private bool? _hasPrimaryKey;

        #endregion
    }
}