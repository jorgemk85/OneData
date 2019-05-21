using OneData.Attributes;
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
        /// Esta propiedad controla las propiedades del objeto que NO estan marcadas con el atributo UnmanagedProperty NI AutoProperty.
        /// </summary>
        internal Dictionary<string, PropertyInfo> FilteredProperties { get; private set; } = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Esta propiedad controla las propiedades del objeto que estan marcadas como ForeignKey.
        /// </summary>
        internal Dictionary<string, PropertyInfo> ForeignKeyProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        internal Dictionary<string, PropertyInfo> UniqueKeyProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        internal Dictionary<string, PropertyInfo> AllowNullProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        internal Dictionary<string, PropertyInfo> DefaultProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        internal Dictionary<string, PropertyInfo> DataLengthProperties { get; set; } = new Dictionary<string, PropertyInfo>();
        internal Dictionary<string, PropertyInfo> ForeignDataProperties { get; set; } = new Dictionary<string, PropertyInfo>();

        internal Dictionary<string, Default> DefaultAttributes { get; set; } = new Dictionary<string, Default>();
        internal Dictionary<string, DataLength> DataLengthAttributes { get; set; } = new Dictionary<string, DataLength>();
        internal Dictionary<string, ForeignData> ForeignDataAttributes { get; set; } = new Dictionary<string, ForeignData>();
        internal Dictionary<string, ForeignKey> ForeignKeyAttributes { get; set; } = new Dictionary<string, ForeignKey>();
        internal Dictionary<string, AutoProperty> AutoPropertyAttributes { get; set; } = new Dictionary<string, AutoProperty>();
        internal string FullyQualifiedTableName { get; set; }
        internal PropertyInfo PrimaryKeyProperty { get; set; }
        internal PropertyInfo DateCreatedProperty { get; set; }
        internal PropertyInfo DateModifiedProperty { get; set; }
        internal PrimaryKey PrimaryKeyAttribute { get; set; }
        internal bool IsIdentityModel { get; set; }
        internal string TableName { get; set; }
        internal string Schema { get; set; }
        internal bool IsCacheEnabled { get; set; }
        internal long CacheExpiration { get; set; }

        public ModelComposition(Type type)
        {
            ModelValidation validation = new ModelValidation(this);
            Properties = type.GetProperties();
            validation.ValidateAndConfigureClass(type);
            validation.ValidateAndConfigureProperties(type);
        }
    }
}
