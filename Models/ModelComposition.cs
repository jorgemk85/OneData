using FastMember;
using OneData.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OneData.Models
{
    public sealed class ModelComposition
    {
        /// <summary>
        /// Controla las propiedades que NO estan marcadas como UnmanagedProperty.
        /// </summary>
        internal Dictionary<string, OneProperty> ManagedProperties { get; private set; } = new Dictionary<string, OneProperty>();
        /// <summary>
        /// Son aquellas propiedades marcadas con el atributo UnmanagedProperty.
        /// </summary>
        internal Dictionary<string, OneProperty> UnmanagedProperties { get; private set; } = new Dictionary<string, OneProperty>();
        /// <summary>
        /// Las propiedades contenidas en este diccionario son aquellas marcadas como AutoProperties, las cuales se usan para ser
        /// alimentada desde la base de datos, en el procedimiento almacenado.
        /// </summary>
        internal Dictionary<string, OneProperty> AutoProperties { get; private set; } = new Dictionary<string, OneProperty>();
        /// <summary>
        /// Esta propiedad controla las propiedades del objeto que NO estan marcadas con el atributo UnmanagedProperty NI AutoProperty.
        /// </summary>
        internal Dictionary<string, OneProperty> FilteredProperties { get; private set; } = new Dictionary<string, OneProperty>();
        /// <summary>
        /// Esta propiedad controla las propiedades del objeto que estan marcadas como ForeignKey.
        /// </summary>
        internal Dictionary<string, OneProperty> ForeignKeyProperties { get; set; } = new Dictionary<string, OneProperty>();
        internal Dictionary<string, OneProperty> UniqueKeyProperties { get; set; } = new Dictionary<string, OneProperty>();
        internal Dictionary<string, OneProperty> AllowNullProperties { get; set; } = new Dictionary<string, OneProperty>();
        internal Dictionary<string, OneProperty> DefaultProperties { get; set; } = new Dictionary<string, OneProperty>();
        internal Dictionary<string, OneProperty> DataLengthProperties { get; set; } = new Dictionary<string, OneProperty>();
        internal Dictionary<string, OneProperty> ForeignDataProperties { get; set; } = new Dictionary<string, OneProperty>();

        internal Dictionary<string, Default> DefaultAttributes { get; set; } = new Dictionary<string, Default>();
        internal Dictionary<string, DataLength> DataLengthAttributes { get; set; } = new Dictionary<string, DataLength>();
        internal Dictionary<string, ForeignData> ForeignDataAttributes { get; set; } = new Dictionary<string, ForeignData>();
        internal Dictionary<string, ForeignKey> ForeignKeyAttributes { get; set; } = new Dictionary<string, ForeignKey>();
        internal Dictionary<string, AutoProperty> AutoPropertyAttributes { get; set; } = new Dictionary<string, AutoProperty>();
        internal string FullyQualifiedTableName { get; set; }
        internal OneProperty PrimaryKeyProperty { get; set; }
        internal OneProperty DateCreatedProperty { get; set; }
        internal OneProperty DateModifiedProperty { get; set; }
        internal PrimaryKey PrimaryKeyAttribute { get; set; }
        internal TypeAccessor Accessor { get; set; }
        internal bool IsIdentityModel { get; set; }
        internal string TableName { get; set; }
        internal string Schema { get; set; }
        internal bool IsCacheEnabled { get; set; }
        internal long CacheExpiration { get; set; }
        internal bool IsFullySynced { get; set; }

        public ModelComposition(Type type)
        {
            Accessor = TypeAccessor.Create(type);
            ModelValidation validation = new ModelValidation(this, Accessor);
            validation.ValidateAndConfigureClass(type);
            validation.ValidateAndConfigureProperties(type);
        }
    }
}
