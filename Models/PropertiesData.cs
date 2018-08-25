using DataManagement.Standard.Attributes;
using DataManagement.Standard.Enums;
using System.Collections.Generic;
using System.Reflection;

namespace DataManagement.Standard.Models
{
    internal class PropertiesData<T>
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
        /// Esta propiedad controla las propiedades del objeto que NO estan marcadas con el atributo UnmanagedProperty NI AutoProperty.
        /// </summary>
        public Dictionary<string, PropertyInfo> ForeignModels { get; set; } = new Dictionary<string, PropertyInfo>();
        public Dictionary<string, AutoPropertyTypes> AutoPropertyTypes { get; set; } = new Dictionary<string, AutoPropertyTypes>();
        public PropertyInfo PrimaryProperty { get; set; }
        public PropertyInfo DateCreatedProperty { get; set; }
        public PropertyInfo DateModifiedProperty { get; set; }

        public PropertiesData()
        {
            Properties = typeof(T).GetProperties();
            SetProperties();
        }

        private void SetProperties()
        {
            IEnumerable<CustomAttributeData> attributes = null;
            foreach (PropertyInfo property in Properties)
            {
                ManagedProperties.Add(property.Name, property);
                FilteredProperties.Add(property.Name, property);
                attributes = property.CustomAttributes;
                foreach (CustomAttributeData attribute in attributes)
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
                            AutoPropertyTypes.Add(property.Name, property.GetCustomAttribute<AutoProperty>().AutoPropertyType);
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
                            ForeignModels.Add(property.Name, property);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
