using DataManagement.Attributes;
using DataManagement.Enums;
using DataManagement.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataManagement.Models
{
    public sealed class ModelComposition
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
        public PropertyInfo PrimaryProperty { get; private set; }
        public PropertyInfo DateCreatedProperty { get; private set; }
        public PropertyInfo DateModifiedProperty { get; private set; }
        public string TableName { get; private set; }
        public string Schema { get; private set; }
        public bool IsCacheEnabled { get; private set; }
        public long CacheExpiration { get; private set; }

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
            if (PrimaryProperty == null)
            {
                throw new RequiredAttributeNotFound("PrimaryProperty", type.FullName);
            }
            else if (Nullable.GetUnderlyingType(PrimaryProperty.PropertyType) == null)
            {
                throw new InvalidDataType(PrimaryProperty.Name, type.FullName, "Nullable<struct>");
            }
            else if (!Nullable.GetUnderlyingType(PrimaryProperty.PropertyType).IsValueType)
            {
                throw new InvalidDataType(PrimaryProperty.Name, type.FullName, "Nullable<struct>");
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
                        TableName = DataTableAttribute.TableName;
                        Schema = DataTableAttribute.Schema;
                        break;
                    case "CacheEnabled":
                        CacheEnabledAttribute = type.GetCustomAttribute<CacheEnabled>();
                        IsCacheEnabled = true;
                        CacheExpiration = CacheEnabledAttribute.Expiration * TimeSpan.TicksPerSecond;
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
                        case "PrimaryProperty":
                            PrimaryProperty = property;
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
