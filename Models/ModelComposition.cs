using OneData.Attributes;
using OneData.DAO;
using OneData.Enums;
using OneData.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OneData.Models
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
        internal Dictionary<string, PropertyInfo> UniqueKeyProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        internal Dictionary<string, PropertyInfo> DataLengthProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        internal Dictionary<string, PropertyInfo> ForeignDataProperties { get; private set; } = new Dictionary<string, PropertyInfo>();

        internal Dictionary<string, PropertyInfo> ForeignCollectionProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        internal Dictionary<string, ForeignCollection> ForeignCollectionAttributes { get; private set; } = new Dictionary<string, ForeignCollection>();

        internal Dictionary<string, UniqueKey> UniqueKeyAttributes { get; private set; } = new Dictionary<string, UniqueKey>();
        internal Dictionary<string, DataLength> DataLengthAttributes { get; private set; } = new Dictionary<string, DataLength>();
        internal Dictionary<string, ForeignData> ForeignDataAttributes { get; private set; } = new Dictionary<string, ForeignData>();
        internal Dictionary<string, ForeignKey> ForeignKeyAttributes { get; private set; } = new Dictionary<string, ForeignKey>();
        internal Dictionary<string, AutoProperty> AutoPropertyAttributes { get; private set; } = new Dictionary<string, AutoProperty>();

        internal PropertyInfo PrimaryKeyProperty { get; private set; }
        internal PropertyInfo DateCreatedProperty { get; private set; }
        internal PropertyInfo DateModifiedProperty { get; private set; }
        internal bool IsIdentityModel { get; private set; }
        internal string TableName { get; private set; }
        internal string Schema { get; private set; }
        internal bool IsCacheEnabled { get; private set; }
        internal long CacheExpiration { get; private set; }
        internal string ForeignPrimaryKeyName { get; private set; }

        public ModelComposition(Type type)
        {
            Properties = type.GetProperties();
            SetClass(type);
            SetProperties(type);
        }

        private void PerformClassValidation(Type type)
        {
            if (string.IsNullOrWhiteSpace(TableName))
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
            else if (!PrimaryKeyProperty.PropertyType.IsValueType)
            {
                throw new InvalidDataType(PrimaryKeyProperty.Name, type.FullName, "struct");
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
            else if (!DateModifiedProperty.PropertyType.Equals(typeof(DateTime)) && !Nullable.GetUnderlyingType(DateCreatedProperty.PropertyType).Equals(typeof(DateTime)))
            {
                throw new InvalidDataType(DateModifiedProperty.Name, type.FullName, "DateTime");
            }
        }

        private void SetClass(Type type)
        {
            DataTable dataTableAttribute;
            CacheEnabled cacheEnabledAttribute;

            foreach (CustomAttributeData attribute in type.CustomAttributes)
            {
                switch (attribute.AttributeType.Name)
                {
                    case "DataTable":
                        dataTableAttribute = type.GetCustomAttribute<DataTable>();
                        TableName = dataTableAttribute.TableName;
                        Schema = string.IsNullOrWhiteSpace(dataTableAttribute.Schema) ? Manager.DefaultSchema : dataTableAttribute.Schema;
                        break;
                    case "CacheEnabled":
                        cacheEnabledAttribute = type.GetCustomAttribute<CacheEnabled>();
                        IsCacheEnabled = true;
                        CacheExpiration = cacheEnabledAttribute.Expiration * TimeSpan.TicksPerSecond;
                        break;
                    case "IdentityModel":
                        IsIdentityModel = true;
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
                            PrimaryKeyProperty = property;
                            ForeignPrimaryKeyName = $"{type.Name}{property.Name}";
                            break;
                        case "DateCreatedProperty":
                            DateCreatedProperty = property;
                            AutoProperties.Add(property.Name, property);
                            AutoPropertyAttributes.Add(property.Name, new AutoProperty(AutoPropertyTypes.DateTime));
                            FilteredProperties.Remove(property.Name);
                            break;
                        case "DateModifiedProperty":
                            DateModifiedProperty = property;
                            AutoProperties.Add(property.Name, property);
                            AutoPropertyAttributes.Add(property.Name, new AutoProperty(AutoPropertyTypes.DateTime));
                            FilteredProperties.Remove(property.Name);
                            break;
                        case "ForeignKey":
                            ForeignKeyProperties.Add(property.Name, property);
                            ForeignKeyAttributes.Add(property.Name, property.GetCustomAttribute<ForeignKey>());
                            break;
                        case "UniqueKey":
                            UniqueKeyProperties.Add(property.Name, property);
                            UniqueKeyAttributes.Add(property.Name, property.GetCustomAttribute<UniqueKey>());
                            break;
                        case "DataLength":
                            DataLengthProperties.Add(property.Name, property);
                            DataLengthAttributes.Add(property.Name, property.GetCustomAttribute<DataLength>());
                            break;
                        case "ForeignData":
                            ForeignDataProperties.Add(property.Name, property);
                            ForeignDataAttributes.Add(property.Name, ConfigureForeignDataAttribute(property.GetCustomAttribute<ForeignData>(), property));
                            ManagedProperties.Remove(property.Name);
                            FilteredProperties.Remove(property.Name);
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

        private ForeignData ConfigureForeignDataAttribute(ForeignData foreignData, PropertyInfo property)
        {
            if (foreignData.ReferenceModel == null)
            {
                foreignData.ReferenceModel = property.ReflectedType;
                foreignData.ReferenceIdName = $"{foreignData.JoinModel.Name}Id";
            }
            foreignData.PropertyName = property.Name;

            return foreignData;
        }
    }
}
