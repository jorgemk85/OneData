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
        /// Controla las propiedades que NO estan marcadas como UnlinkedProperty.
        /// </summary>
        public Dictionary<string, PropertyInfo> LinkedProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Son aquellas propiedades marcadas con el atributo UnlinkedProperty.
        /// </summary>
        public Dictionary<string, PropertyInfo> UnlinkedProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Las propiedades contenidas en este diccionario son aquellas marcadas como AutoProperties, las cuales se usan para ser
        /// alimentada desde la base de datos, en el procedimiento almacenado.
        /// </summary>
        public Dictionary<string, PropertyInfo> AutoProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Esta propiedad controla las propiedades del objeto que NO estan marcadas con el atributo UnlinkedProperty NI AutoProperty.
        /// </summary>
        public Dictionary<string, PropertyInfo> FilteredProperties { get; set; } = new Dictionary<string, PropertyInfo>();
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
                LinkedProperties.Add(property.Name, property);
                FilteredProperties.Add(property.Name, property);
                attributes = property.CustomAttributes;
                foreach (CustomAttributeData attribute in attributes)
                {
                    switch (attribute.AttributeType.Name)
                    {
                        case "UnlinkedProperty":
                            UnlinkedProperties.Add(property.Name, property);
                            LinkedProperties.Remove(property.Name);
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
                        default:
                            break;
                    }
                }
            }
        }
    }
}
