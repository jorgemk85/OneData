using DataManagement.Standard.Attributes;
using DataManagement.Standard.DAO;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataManagement.Standard.Models
{
    public class ModelComposition
    {
        /// <summary>
        /// Arreglo completo de las propiedades sin filtrar.
        /// </summary>
        public PropertyInfo[] Properties { get; set; }
        /// <summary>
        /// Controla las propiedades que NO estan marcadas como UnmanagedProperty.
        /// </summary>
        public Dictionary<string, PropertyInfo> ManagedProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Son aquellas propiedades marcadas con el atributo UnmanagedProperty.
        /// </summary>
        public Dictionary<string, PropertyInfo> UnmanagedProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Las propiedades contenidas en este diccionario son aquellas marcadas como AutoProperties, las cuales se usan para ser
        /// alimentada desde la base de datos, en el procedimiento almacenado.
        /// </summary>
        public Dictionary<string, PropertyInfo> AutoProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Esta propiedad controla las propiedades del objeto que NO estan marcadas con el atributo UnmanagedProperty NI AutoProperty.
        /// </summary>
        public Dictionary<string, PropertyInfo> FilteredProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Esta propiedad controla las propiedades del objeto que estan marcadas como ForeignModel.
        /// </summary>
        public Dictionary<string, PropertyInfo> ForeignModelProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        public Dictionary<string, ForeignModel> ForeignModelAttributes { get; set; } = new Dictionary<string, ForeignModel>();
        public Dictionary<string, AutoProperty> AutoPropertyAttributes { get; set; } = new Dictionary<string, AutoProperty>();
        public PropertyInfo PrimaryProperty { get; set; }
        public PropertyInfo DateCreatedProperty { get; set; }
        public PropertyInfo DateModifiedProperty { get; set; }
        public DataTableName DataTableNameAttribute { get; set; }
        public CacheEnabled CacheEnabledAttribute { get; set; }
        public string TableName { get; set; }
        public string Schema { get; set; }
        public bool IsCacheEnabled { get; set; }
        public long CacheExpiration { get; set; }

        public ModelComposition(Type type)
        {
            Properties = type.GetProperties();
            SetClass(type);
            SetProperties();
        }

        private void SetClass(Type type)
        {
            foreach (CustomAttributeData attribute in type.CustomAttributes)
            {
                switch (attribute.AttributeType.Name)
                {
                    case "DataTableName":
                        DataTableNameAttribute = type.GetCustomAttribute<DataTableName>();
                        TableName = DataTableNameAttribute.TableName;
                        Schema = DataTableNameAttribute.Schema;
                        break;
                    case "CacheEnabled":
                        CacheEnabledAttribute = type.GetCustomAttribute<CacheEnabled>();
                        IsCacheEnabled = true;
                        CacheExpiration = CacheEnabledAttribute.Expiration;
                        break;
                    default:
                        break;
                }
            }
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
