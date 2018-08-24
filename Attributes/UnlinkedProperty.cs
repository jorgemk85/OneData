using System;

namespace DataManagement.Standard.Attributes
{
    /// <summary>
    /// Utiliza este atributo cuando deseas desvincular la propiedad de la base de datos, de manera que sea ignorada al realizar consultas con la clase Manager de DataManagement.Standard.DAO.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UnlinkedProperty : Attribute
    {
        public UnlinkedProperty() { }
    }
}
