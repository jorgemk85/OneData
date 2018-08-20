using DataManagement.Attributes;
using DataManagement.Interfaces;
using DataManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataManagement.DAO
{
    internal class MsSqlCreation : Creation, ICreatable
    {
        public void SetStoredProceduresParameters(ref PropertyInfo[] properties, StringBuilder queryBuilder, bool setDefaultNull)
        {
            // Aqui se colocan los parametros segun las propiedades del objeto 
            foreach (PropertyInfo property in properties)
            {
                if (setDefaultNull)
                {
                    queryBuilder.AppendFormat("@_{0} {1} = null, ", property.Name, GetSqlDataType(property.PropertyType));
                }
                else
                {
                    queryBuilder.AppendFormat("@_{0} {1}, ", property.Name, GetSqlDataType(property.PropertyType));
                }
            }
        }

        public string CreateInsertStoredProcedure<T>(bool doAlter) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<InternalProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3} ", obj.Schema, StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3} ", obj.Schema, StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);
            }

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(ref properties, queryBuilder, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(" AS ");
            queryBuilder.Append("BEGIN ");
            queryBuilder.Append("DECLARE @actualTime datetime;");
            queryBuilder.Append("SET @actualTime = getdate();");
            queryBuilder.AppendFormat("INSERT INTO {0}.{1}{2} (", obj.Schema, TablePrefix, obj.DataBaseTableName);

            // Seccion para especificar a que columnas se va a insertar.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("{0}, ", property.Name);
            }

            queryBuilder.Append("fechaCreacion, fechaModificacion");
            queryBuilder.Append(") VALUES (");

            // Especificamos los parametros para insertar en la base de datos.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("@_{0}, ", property.Name);
            }

            queryBuilder.Append("@actualTime, @actualTime);");
            queryBuilder.Append("END ");

            return queryBuilder.ToString();
        }

        public string CreateUpdateStoredProcedure<T>(bool doAlter) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<InternalProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3} ", obj.Schema, StoredProcedurePrefix, obj.DataBaseTableName, UpdateSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3} ", obj.Schema, StoredProcedurePrefix, obj.DataBaseTableName, UpdateSuffix);
            }


            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(ref properties, queryBuilder, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(" AS ");
            queryBuilder.Append("BEGIN ");
            queryBuilder.Append("DECLARE @actualTime datetime;");
            queryBuilder.Append("SET @actualTime = getdate();");
            queryBuilder.AppendFormat("UPDATE {0}.{1}{2} ", obj.Schema, TablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("SET ");

            // Se especifica el parametro que va en x columna.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("{0} = ISNULL(@_{0}, {0}), ", property.Name);
            }
            queryBuilder.Append("fechaModificacion = @actualTime ");
            queryBuilder.AppendFormat(" WHERE Id = @_Id; ");
            queryBuilder.Append("END ");

            return queryBuilder.ToString();
        }

        public string CreateDeleteStoredProcedure<T>(bool doAlter) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<InternalProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3} ", obj.Schema, StoredProcedurePrefix, obj.DataBaseTableName, DeleteSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3} ", obj.Schema, StoredProcedurePrefix, obj.DataBaseTableName, DeleteSuffix);
            }

            queryBuilder.Append("@_Id uniqueidentifier ");
            queryBuilder.Append(" AS ");
            queryBuilder.Append("BEGIN ");
            queryBuilder.AppendFormat("DELETE FROM {0}.{1}{2} ", obj.Schema, TablePrefix, obj.DataBaseTableName);
            queryBuilder.AppendFormat("WHERE Id = @_Id; ");
            queryBuilder.Append("END ");

            return queryBuilder.ToString();
        }

        public string CreateSelectAllStoredProcedure<T>(bool doAlter) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<InternalProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3} ", obj.Schema, StoredProcedurePrefix, obj.DataBaseTableName, SelectAllSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3} ", obj.Schema, StoredProcedurePrefix, obj.DataBaseTableName, SelectAllSuffix);
            }

            queryBuilder.Append(" AS ");
            queryBuilder.Append("BEGIN ");
            queryBuilder.AppendFormat("SELECT * FROM {0}.{1}{2} ", obj.Schema, TablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("ORDER BY FechaCreacion DESC ");
            queryBuilder.Append("END ");

            return queryBuilder.ToString();
        }

        public string CreateSelectStoredProcedure<T>(bool doAlter) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<InternalProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3} ", obj.Schema, StoredProcedurePrefix, obj.DataBaseTableName, SelectSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3} ", obj.Schema, StoredProcedurePrefix, obj.DataBaseTableName, SelectSuffix);
            }

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(ref properties, queryBuilder, true);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(" AS ");
            queryBuilder.Append("BEGIN ");
            queryBuilder.AppendFormat("SELECT * FROM {0}.{1}{2} ", obj.Schema, TablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("WHERE ");

            // Se especifica el parametro que va en x columna.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("{0} LIKE ISNULL(CONCAT('%', @_{0}, '%'), {0}) AND ", property.Name);
            }

            queryBuilder.Remove(queryBuilder.Length - 5, 5);
            queryBuilder.AppendFormat(" ORDER BY FechaCreacion desc;");
            queryBuilder.Append("END ");

            return queryBuilder.ToString();
        }

        public string GetCreateTableQuery(Type type)
        {
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            IManageable obj = (IManageable)Activator.CreateInstance(type);

            if (properties.Length == 0) return string.Empty;

            return CreateQueryForTableCreation(obj, ref properties);
        }

        public string CreateQueryForTableCreation(IManageable obj, ref PropertyInfo[] properties)
        {
            StringBuilder queryBuilder = new StringBuilder();

            queryBuilder.AppendFormat("CREATE TABLE {0}.{1}{2} ", obj.Schema, TablePrefix, obj.DataBaseTableName);

            queryBuilder.Append("(");
            // Aqui se colocan las propiedades del objeto. Una por columna por su puesto.
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.Equals("Id"))
                {
                    queryBuilder.AppendFormat("{0} {1} NOT NULL PRIMARY KEY, ", property.Name, GetSqlDataType(property.PropertyType));
                }
                else
                {
                    queryBuilder.AppendFormat("{0} {1} NOT NULL, ", property.Name, GetSqlDataType(property.PropertyType));
                }
            }
            //queryBuilder.Append("FechaCreacion datetime NOT NULL, FechaModificacion datetime NOT NULL);");

            return queryBuilder.ToString();
        }

        public string GetAlterTableQuery(Type type, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, KeyDefinition> keyDetails)
        {
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            IManageable obj = (IManageable)Activator.CreateInstance(type);

            if (properties.Length == 0) return string.Empty;

            return CreateQueryForTableAlteration(obj, ref properties, columnDetails, keyDetails);
        }

        public string CreateQueryForTableAlteration(IManageable obj, ref PropertyInfo[] properties, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, KeyDefinition> keyDetails)
        {
            StringBuilder queryBuilder = new StringBuilder();
            List<string> columnsFound = new List<string>();
            string sqlDataType = string.Empty;
            // Las propiedades FechaCreacion y FechaModificacion deben de agregarse a columnsFound.
            // TODO: Necesitamos asignarle algun atributo a la FechaCreacion y FechaModificacion para indicar que son especiales.
            //columnsFound.Add("FechaCreacion");
            //columnsFound.Add("FechaModificacion");

            ColumnDefinition columnDetail;

            queryBuilder.AppendFormat("ALTER TABLE {0}{1} \n", TablePrefix, obj.DataBaseTableName);

            // Aqui se agregan las propiedades que no estan en la tabla. Tambien guarda en una lista las columnas encontradas.
            foreach (PropertyInfo property in properties)
            {
                sqlDataType = GetSqlDataType(property.PropertyType);
                columnDetails.TryGetValue(property.Name, out columnDetail);

                if (columnDetail == null)
                {
                    queryBuilder.AppendFormat("ADD {0} {1} NOT NULL,\n", property.Name, sqlDataType);
                    continue;
                }
                if (!sqlDataType.Equals(columnDetail.Column_Type))
                {
                    queryBuilder.AppendFormat("MODIFY COLUMN {0} {1} NOT NULL,\n", property.Name, sqlDataType);
                }
                if (columnDetail.Column_Key.Equals("MUL"))
                {
                    ForeignModel foreignAttribute = property.GetCustomAttribute<ForeignModel>();
                    if (foreignAttribute == null)
                    {
                        queryBuilder.AppendFormat("DROP FOREIGN KEY {0},\n", keyDetails[columnDetail.Column_Name].Constraint_Name);
                    }
                }
                columnsFound.Add(property.Name);
            }

            // Extraemos las columnas en la tabla que ya no estan en las propiedades del modelo para quitarlas.
            foreach (KeyValuePair<string, ColumnDefinition> detail in columnDetails.Where(q => !columnsFound.Contains(q.Key)))
            {
                queryBuilder.AppendFormat("DROP {0},\n", detail.Value.Column_Name);
                continue;
            }

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(";");

            queryBuilder.Append(GetCreateForeignKeysQuery(obj.GetType()));

            return queryBuilder.ToString();
        }

        public string GetCreateForeignKeysQuery(Type type)
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<ForeignModel>() != null).ToArray();
            IManageable obj = (IManageable)Activator.CreateInstance(type);

            if (properties.Length == 0) return string.Empty;

            queryBuilder.AppendFormat("ALTER TABLE {0}.{1}{2} ", obj.Schema, TablePrefix, obj.DataBaseTableName);

            foreach (PropertyInfo property in properties)
            {
                ForeignModel foreignAttribute = property.GetCustomAttribute<ForeignModel>();
                IManageable foreignModel = (IManageable)Activator.CreateInstance(foreignAttribute.Model);
                queryBuilder.AppendFormat("ADD CONSTRAINT FK_{0}_{1} ", obj.DataBaseTableName, foreignModel.DataBaseTableName);
                queryBuilder.AppendFormat("FOREIGN KEY({0}) REFERENCES {1}.{2}{3}(Id) ON DELETE {4} ON UPDATE NO ACTION;", property.Name, obj.Schema, TablePrefix, foreignModel.DataBaseTableName, foreignAttribute.Action.ToString().Replace("_", " "));
            }

            return queryBuilder.ToString();
        }

        public string GetSqlDataType(Type codeType)
        {
            Type underlyingType = Nullable.GetUnderlyingType(codeType);

            if (underlyingType == null)
            {
                underlyingType = codeType;
            }

            switch (underlyingType.Name.ToLower())
            {
                case "boolean":
                    return "bit";
                case "bool":
                    return "bit";
                case "guid":
                    return "uniqueidentifier";
                case "string":
                    return "varchar(255)";
                case "datetime":
                    return "datetime";
                case "decimal":
                    return "decimal(18,2)";
                case "single":
                    return "real";
                case "float":
                    return "float";
                case "double":
                    return "float";
                case "byte":
                    return "tinyint";
                case "sbyte":
                    return "smallint";
                case "byte[]":
                    return "binary";
                case "short":
                    return "smallint";
                case "ushort":
                    return "numeric(5)";
                case "int":
                    return "int";
                case "uint":
                    return "numeric(10)";
                case "int16":
                    return "smallint";
                case "int32":
                    return "int";
                case "uint32":
                    return "numeric(10)";
                case "int64":
                    return "bigint";
                case "uint64":
                    return "numeric(20)";
                case "long":
                    return "bigint";
                case "ulong":
                    return "numeric(20)";
                default:
                    return "varchar(255)";
            }
        }
    }
}
