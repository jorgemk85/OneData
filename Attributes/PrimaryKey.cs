using System;

namespace OneData.Attributes
{
    /// <summary>
    /// Especifica la propiedad usada como llave primaria. Solo para administracion interna de la libreria.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKey : Attribute
    {
        public bool IsAutoIncrement { get; set; }

        public PrimaryKey(bool isAutoIncrement = false)
        {
            IsAutoIncrement = isAutoIncrement;
        }
    }
}
