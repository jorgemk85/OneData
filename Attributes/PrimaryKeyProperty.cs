using System;

namespace DataManagement.Attributes
{
    /// <summary>
    /// Especifica la propiedad usada como llave primaria. Solo para administracion interna de la libreria.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyProperty : Attribute
    {
    }
}
