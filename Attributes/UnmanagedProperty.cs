using System;

namespace OneData.Attributes
{
    /// <summary>
    /// Utiliza este atributo cuando deseas desvincular la propiedad de la base de datos, de manera que sea ignorada al realizar consultas con la clase Manager de OneData.DAO.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UnmanagedProperty : Attribute
    {
        public UnmanagedProperty() { }
    }
}
