using System;

namespace DataManagement.Standard.Attributes
{
    /// <summary>
    /// Especifica la propiedad usada como llave primaria. Solo para administracion interna de la libreria.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class PrimaryProperty : Attribute
    {
    }
}
