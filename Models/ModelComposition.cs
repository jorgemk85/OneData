using DataManagement.Attributes;
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
        public PropertyInfo[] Properties { get; private set; }
        /// <summary>
        /// Controla las propiedades que NO estan marcadas como UnmanagedProperty.
        /// </summary>
        public Dictionary<string, PropertyInfo> ManagedProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Son aquellas propiedades marcadas con el atributo UnmanagedProperty.
        /// </summary>
        public Dictionary<string, PropertyInfo> UnmanagedProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Las propiedades contenidas en este diccionario son aquellas marcadas como AutoProperties, las cuales se usan para ser
        /// alimentada desde la base de datos, en el procedimiento almacenado.
        /// </summary>
        public Dictionary<string, PropertyInfo> AutoProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Esta propiedad controla las propiedades del objeto que NO estan marcadas con el atributo UnmanagedProperty NI AutoProperty.
        /// </summary>
        public Dictionary<string, PropertyInfo> FilteredProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Esta propiedad controla las propiedades del objeto que estan marcadas como ForeignModel.
        /// </summary>
        public Dictionary<string, PropertyInfo> ForeignModelProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        public Dictionary<string, ForeignModel> ForeignModelAttributes { get; private set; } = new Dictionary<string, ForeignModel>();
        public Dictionary<string, AutoProperty> AutoPropertyAttributes { get; private set; } = new Dictionary<string, AutoProperty>();
        public PropertyInfo PrimaryProperty { get; private set; }
        public PropertyInfo DateCreatedProperty { get; private set; }
        public PropertyInfo DateModifiedProperty { get; private set; }
        public DataTable DataTableAttribute { get; private set; }
        public CacheEnabled CacheEnabledAttribute { get; private set; }
        public string TableName { get; private set; }
        public string Schema { get; private set; }
        public bool IsCacheEnabled { get; private set; }
        public long CacheExpiration { get; private set; }

        public ModelComposition(Type type)
        {
            Properties = type.GetProperties();
            SetClass(type);
            SetProperties();
        }

        private void PerformClassValidation(Type type)
        {
            if (string.IsNullOrWhiteSpace(TableName))
            {
                throw new RequiredAttributeNotFound("DataTableName", type.FullName);
            }
        }

        private void SetClass(Type type)
        {
            foreach (CustomAttributeData attribute in type.CustomAttributes)
            {
                switch (attribute.AttributeType.Name)
                {
                    case "DataTableName":
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

        private void SetProperties()
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
                            break;
                        case "DateModifiedProperty":
                            DateModifiedProperty = property;
                            break;
                        case "ForeignModel":
                            ForeignModelProperties.Add(property.Name, property);
                            ForeignModelAttributes.Add(property.Name, property.GetCustomAttribute<ForeignModel>());
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
