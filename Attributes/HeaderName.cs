using System;

namespace OneData.Attributes
{
    /// <summary>
    /// Utilizar este atributo en las propiedades que se desee especificar el nombre de la columna que la representa y si es o no importante su presencia en el archivo.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class HeaderName : Attribute
    {
        public string Name { get; set; }
        public bool Important { get; set; }

        public HeaderName(string name, bool important = true)
        {
            Name = name;
            Important = important;
        }
    }
}