namespace DataManagement.Attributes
{
    /// <summary>
    /// Utiliza este atributo cuando deseas desvincular la propiedad de la base de datos, de manera que sea ignorada al realizar consultas con la clase Manager de DataManagement.DAO.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class UnlinkedProperty : System.Attribute
    {
        public UnlinkedProperty() { }
    }
}
