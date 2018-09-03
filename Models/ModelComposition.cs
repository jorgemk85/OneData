using DataManagement.Attributes;
using DataManagement.Enums;
using DataManagement.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataManagement.Models
{
    internal sealed class ModelComposition
    {
        /// <summary>
        /// Arreglo completo de las propiedades sin filtrar.
        /// </summary>
        internal PropertyInfo[] Properties { get; private set; }
        /// <summary>
        /// Controla las propiedades que NO estan marcadas como UnmanagedProperty.
        /// </summary>
        internal Dictionary<string, PropertyInfo> ManagedProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Son aquellas propiedades marcadas con el atributo UnmanagedProperty.
        /// </summary>
        internal Dictionary<string, PropertyInfo> UnmanagedProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Las propiedades contenidas en este diccionario son aquellas marcadas como AutoProperties, las cuales se usan para ser
        /// alimentada desde la base de datos, en el procedimiento almacenado.
        /// </summary>
        internal Dictionary<string, PropertyInfo> AutoProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Esta propiedad controla las propiedades del objeto que NO estan marcadas con el atributo UnmanagedProperty NI AutoProperty NI ForeignCollection.
        /// </summary>
        internal Dictionary<string, PropertyInfo> FilteredProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Esta propiedad controla las propiedades del objeto que estan marcadas como ForeignKey.
        /// </summary>
        internal Dictionary<string, PropertyInfo> ForeignKeyProperties { get; private set; } = new Dictionary<string, PropertyInfo>();

        internal Dictionary<string, PropertyInfo> ForeignCollectionProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        internal Dictionary<string, ForeignCollection> ForeignCollectionAttributes { get; private set; } = new Dictionary<string, ForeignCollection>();

        internal Dictionary<string, ForeignKey> ForeignKeyAttributes { get; private set; } = new Dictionary<string, ForeignKey>();
        internal Dictionary<string, AutoProperty> AutoPropertyAttributes { get; private set; } = new Dictionary<string, AutoProperty>();
        internal DataTable DataTableAttribute { get; private set; }
        internal CacheEnabled CacheEnabledAttribute { get; private set; }
        private PropertyInfo _primaryKeyProperty;
        private PropertyInfo _dateCreatedProperty;
        private PropertyInfo _dateModifiedProperty;
        private bool _isCacheEnabled;
        private long _cacheExpiration;
        private string _tableName;
        private string _schema;
        private string _foreignPrimaryKeyName;

        internal ref readonly PropertyInfo PrimaryKeyProperty => ref _primaryKeyProperty;
        internal ref readonly PropertyInfo DateCreatedProperty => ref _dateCreatedProperty;
        internal ref readonly PropertyInfo DateModifiedProperty => ref _dateModifiedProperty;
        internal ref readonly string TableName => ref _tableName;
        internal ref readonly string Schema => ref _schema;
        internal ref readonly bool IsCacheEnabled => ref _isCacheEnabled;
        internal ref readonly long CacheExpiration => ref _cacheExpiration;
        internal ref readonly string ForeignPrimaryKeyName => ref _foreignPrimaryKeyName;

        public ModelComposition(Type type)
        {
            Properties = type.GetProperties();
            SetClass(type);
            SetProperties(type);
        }

        private void PerformClassValidation(Type type)
        {
            if (string.IsNullOrWhiteSpace(DataTableAttribute.TableName))
            {
                throw new RequiredAttributeNotFound("DataTable", type.FullName);
            }
        }

        private void PerformPropertiesValidation(Type type)
        {
            if (PrimaryKeyProperty == null)
            {
                throw new RequiredAttributeNotFound("PrimaryKeyProperty", type.FullName);
            }
            else if (Nullable.GetUnderlyingType(PrimaryKeyProperty.PropertyType) == null)
            {
                throw new InvalidDataType(PrimaryKeyProperty.Name, type.FullName, "Nullable<struct>");
            }
            else if (!Nullable.GetUnderlyingType(PrimaryKeyProperty.PropertyType).IsValueType)
            {
                throw new InvalidDataType(PrimaryKeyProperty.Name, type.FullName, "Nullable<struct>");
            }

            if (DateCreatedProperty == null)
            {
                throw new RequiredAttributeNotFound("DateCreatedProperty", type.FullName);
            }
            else if (!DateCreatedProperty.PropertyType.Equals(typeof(DateTime)) && !Nullable.GetUnderlyingType(DateCreatedProperty.PropertyType).Equals(typeof(DateTime)))
            {
                throw new InvalidDataType(DateCreatedProperty.Name, type.FullName, "DateTime");
            }

            if (DateModifiedProperty == null)
            {
                throw new RequiredAttributeNotFound("DateModifiedProperty", type.FullName);
            }
            else if (!DateModifiedProperty.PropertyType.Equals(typeof(DateTime)) && !Nullable.GetUnderlyingType(DateModifiedProperty.PropertyType).Equals(typeof(DateTime)))
            {
                throw new InvalidDataType(DateModifiedProperty.Name, type.FullName, "DateTime");
            }
        }

        private void SetClass(Type type)
        {
            foreach (CustomAttributeData attribute in type.CustomAttributes)
            {
                switch (attribute.AttributeType.Name)
                {
                    case "DataTable":
                        DataTableAttribute = type.GetCustomAttribute<DataTable>();
                        _tableName = DataTableAttribute.TableName;
                        _schema = DataTableAttribute.Schema;
                        break;
                    case "CacheEnabled":
                        CacheEnabledAttribute = type.GetCustomAttribute<CacheEnabled>();
                        _isCacheEnabled = true;
                        _cacheExpiration = CacheEnabledAttribute.Expiration * TimeSpan.TicksPerSecond;
                        break;
                    default:
                        break;
                }
            }
            PerformClassValidation(type);
        }

        private void SetProperties(Type type)
        {
            foreach (PropertyInfo property in Properties)
            {
                ManagedProperties.Add(property.Name, property);
                FilteredProperties.Add(property.Name, property);
                foreach (CustomAttributeData attribute in property.CustomAttributes)
                {
                    switch (attribute.AttributeType.Name)
                    {
                        case "UnmanagedProperty":
                            UnmanagedProperties.Add(property.Name, property);
                            ManagedProperties.Remove(property.Name);
                            FilteredProperties.Remove(property.Name);
                            break;
                        case "AutoProperty":
                            AutoProperties.Add(property.Name, property);
                            AutoPropertyAttributes.Add(property.Name, property.GetCustomAttribute<AutoProperty>());
                            FilteredProperties.Remove(property.Name);
                            break;
                        case "PrimaryKeyProperty":
                            _primaryKeyProperty = property;
                            _foreignPrimaryKeyName = $"{type.Name}{property.Name}";
                            break;
                        case "DateCreatedProperty":
                            _dateCreatedProperty = property;
                            AutoProperties.Add(property.Name, property);
                            AutoPropertyAttributes.Add(property.Name, new AutoProperty(AutoPropertyTypes.DateTime));
                            FilteredProperties.Remove(property.Name);
                            break;
                        case "DateModifiedProperty":
                            _dateModifiedProperty = property;
                            AutoProperties.Add(property.Name, property);
                            AutoPropertyAttributes.Add(property.Name, new AutoProperty(AutoPropertyTypes.DateTime));
                            FilteredProperties.Remove(property.Name);
                            break;
                        case "ForeignKey":
                            ForeignKeyProperties.Add(property.Name, property);
                            ForeignKeyAttributes.Add(property.Name, property.GetCustomAttribute<ForeignKey>());
                            break;
                        case "ForeignCollection":
                            ForeignCollectionProperties.Add(property.Name, property);
                            ForeignCollectionAttributes.Add(property.Name, property.GetCustomAttribute<ForeignCollection>());
                            ManagedProperties.Remove(property.Name);
                            FilteredProperties.Remove(property.Name);
                            break;
                        default:
                            break;
                    }
                }
            }
            PerformPropertiesValidation(type);
        }
    }
}
