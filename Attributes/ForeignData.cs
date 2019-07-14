using OneData.Enums;
using System;

namespace OneData.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignData : Attribute
    {
        public Type JoinModel { get; set; }
        public Type ReferenceModel { get; set; }
        public string ReferenceIdName { get; set; }
        public string ColumnName { get; set; }
        public JoinClauseTypes JoinClauseType { get; set; }
        internal string PropertyName { get; set; }

        /// <summary>
        /// Constructor flexible con todas las especificaciones disponibles para configuracion.
        /// </summary>
        /// <param name="joinModel">El tipo del modelo que representa la union a realizar. Debe implementar IManageable.</param>
        /// <param name="referenceModel">El tipo del modelo que representa la referencia que se hara dentro de la union. Regularmente es la misma clase que declara la propiedad. Debe implementar IManageable</param>
        /// <param name="referenceIdName">Nombre de la propiedad o columna que contiene el valor Id a localizar para la extraccion de informacion en la union.</param>
        /// <param name="columnName">Nombre de la propiedad o columna de donde se extrae la informacion.</param>
        public ForeignData(Type joinModel, Type referenceModel, string referenceIdName, string columnName, JoinClauseTypes joinClauseType = JoinClauseTypes.INNER)
        {
            JoinModel = joinModel;
            ReferenceModel = referenceModel;
            ReferenceIdName = referenceIdName;
            ColumnName = columnName;
            JoinClauseType = joinClauseType;
        }

        /// <summary>
        /// Constructor que solo recibe el tipo de modelo utilizado para llevar a cabo la union. El valor de la propiedad 'referenceModel' se asume es el modelo donde se encuentra declarada la propiedad, asi como para 'referenceIdName' donde su valor es el nombre de la clase enviada en 'joinModel' seguida por la palabta 'Id'. El valor estatico para 'columnName' es 'Name'.
        /// </summary>
        /// <param name="joinModel">El tipo del modelo que representa la union a realizar. Debe implementar IManageable.</param>
        public ForeignData(Type joinModel, JoinClauseTypes joinClauseType = JoinClauseTypes.INNER)
        {
            JoinModel = joinModel;
            ReferenceModel = null;
            ReferenceIdName = null;
            ColumnName = "Name";
            JoinClauseType = joinClauseType;
        }

        /// <summary>
        /// Constructor que recibe el tipo de modelo utilizado para llevar a cabo la union y el nombre de la propiedad o columna de donde se extrae la informacion. El valor de la propiedad 'referenceModel' se asume es el modelo donde se encuentra declarada la propiedad, asi como para 'referenceIdName' donde su valor es el nombre de la clase enviada en 'joinModel' seguida por la palabra 'Id'.
        /// </summary>
        /// <param name="joinModel">El tipo del modelo que representa la union a realizar. Debe implementar IManageable.</param>
        /// <param name="columnName">Nombre de la propiedad o columna de donde se extrae la informacion.</param>
        public ForeignData(Type joinModel, string columnName, JoinClauseTypes joinClauseType = JoinClauseTypes.INNER)
        {
            JoinModel = joinModel;
            ReferenceModel = null;
            ReferenceIdName = null;
            ColumnName = columnName;
            JoinClauseType = joinClauseType;
        }
    }
}
