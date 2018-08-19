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
    internal class MySqlCreation : Creation, ICreatable
    {
        public void SetStoredProceduresParameters(ref PropertyInfo[] properties, StringBuilder queryBuilder, bool setDefaultNull)
        {
            // Aqui se colocan los parametros segun las propiedades del objeto 
            foreach (PropertyInfo property in properties)
            {
                if (setDefaultNull)
                {
                    queryBuilder.AppendFormat("    IN _{0} {1} = null,\n", property.Name, GetSqlDataType(property.PropertyType));
                }
                else
                {
                    queryBuilder.AppendFormat("    IN _{0} {1},\n", property.Name, GetSqlDataType(property.PropertyType));
                }
            }
        }

        public string CreateInsertStoredProcedure<T>(bool doAlter) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("DROP PROCEDURE {0}{1}{2};\n", StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);
            }

            queryBuilder.AppendFormat("CREATE PROCEDURE {0}{1}{2} (\n", StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(ref properties, queryBuilder, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(")\nBEGIN\n");
            queryBuilder.Append("SET @@sql_mode:=TRADITIONAL;\n");
            queryBuilder.Append("SET @actualTime = now();\n");
            queryBuilder.AppendFormat("INSERT INTO {0}{1} (\n", TablePrefix, obj.DataBaseTableName);

            // Seccion para especificar a que columnas se va a insertar.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("    {0},\n", property.Name);
            }

            queryBuilder.Append("    fechaCreacion,\n    fechaModificacion");
            queryBuilder.Append(")\nVALUES (\n");

            // Especificamos los parametros para insertar en la base de datos.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("    _{0},\n", property.Name);
            }

            queryBuilder.Append("    @actualTime,\n    @actualTime);\n");
            queryBuilder.Append("END");

            return queryBuilder.ToString();
        }

        public string CreateUpdateStoredProcedure<T>(bool doAlter) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("DROP PROCEDURE {0}{1}{2};\n", StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);
            }

            queryBuilder.AppendFormat("CREATE PROCEDURE {0}{1}{2} (\n", StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(ref properties, queryBuilder, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(")\nBEGIN\n");
            queryBuilder.Append("SET @@sql_mode:=TRADITIONAL;\n");
            queryBuilder.Append("SET @actualTime = now();\n");
            queryBuilder.AppendFormat("UPDATE {0}{1}\n", TablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("SET\n");

            // Se especifica el parametro que va en x columna.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("    {0} =  _{0},\n", property.Name);
            }
            queryBuilder.Append("    fechaModificacion = @actualTime\n");
            queryBuilder.AppendFormat("WHERE Id = _Id;\n");
            queryBuilder.Append("END");

            return queryBuilder.ToString();
        }

        public string CreateDeleteStoredProcedure<T>(bool doAlter) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("DROP PROCEDURE {0}{1}{2};\n", StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);
            }

            queryBuilder.AppendFormat("CREATE PROCEDURE {0}{1}{2} (\n", StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);

            queryBuilder.Append("    IN _Id char(36))\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("DELETE FROM {0}{1}\n", TablePrefix, obj.DataBaseTableName);
            queryBuilder.AppendFormat("WHERE Id = _Id;\n");
            queryBuilder.Append("END");

            return queryBuilder.ToString();
        }

        public string CreateSelectAllStoredProcedure<T>(bool doAlter) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("DROP PROCEDURE {0}{1}{2};\n", StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);
            }

            queryBuilder.AppendFormat("CREATE PROCEDURE {0}{1}{2} (\n", StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);

            queryBuilder.Append(")\nBEGIN\n");
            queryBuilder.AppendFormat("SELECT * FROM {0}{1}\n", TablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("ORDER BY FechaCreacion DESC;\n");
            queryBuilder.Append("END");

            return queryBuilder.ToString();
        }

        public string CreateSelectStoredProcedure<T>(bool doAlter) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("DROP PROCEDURE {0}{1}{2};\n", StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);
            }

            queryBuilder.AppendFormat("CREATE PROCEDURE {0}{1}{2} (\n", StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(ref properties, queryBuilder, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(")\nBEGIN\n");
            queryBuilder.Append("SET @@sql_mode:=TRADITIONAL;\n");
            queryBuilder.AppendFormat("SELECT * FROM {0}{1}\n", TablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("WHERE\n");

            // Se especifica el parametro que va en x columna.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("    {0} LIKE IFNULL(CONCAT('%', _{0}, '%'), {0}) AND\n", property.Name);
            }

            queryBuilder.Remove(queryBuilder.Length - 4, 4);
            queryBuilder.AppendFormat("\nORDER BY FechaCreacion desc;\n");
            queryBuilder.Append("END");

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

            queryBuilder.AppendFormat("CREATE TABLE {0}{1} (\n", TablePrefix, obj.DataBaseTableName);

            // Aqui se colocan las propiedades del objeto. Una por columna por supuesto.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("{0} {1} NOT NULL,\n", property.Name, GetSqlDataType(property.PropertyType));
            }
            queryBuilder.Append("FechaCreacion datetime NOT NULL,\nFechaModificacion datetime NOT NULL,\n");
            queryBuilder.Append("PRIMARY KEY (Id),\n");
            queryBuilder.Append("UNIQUE KEY `id_UNIQUE` (Id))\n");
            queryBuilder.Append("ENGINE=InnoDB;");

            return queryBuilder.ToString();
        }

        public string GetAlterTableQuery(Type type, Dictionary<string, ColumnDetail> columnDetails)
        {
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            IManageable obj = (IManageable)Activator.CreateInstance(type);

            if (properties.Length == 0) return string.Empty;

            return CreateQueryForTableAlteration(obj, ref properties, columnDetails);
        }

        public string CreateQueryForTableAlteration(IManageable obj, ref PropertyInfo[] properties, Dictionary<string, ColumnDetail> columnDetails)
        {
            StringBuilder queryBuilder = new StringBuilder();
            List<string> columnsFound = new List<string>();
            // Las propiedades FechaCreacion y FechaModificacion deben de agregarse a columnsFound.
            // TODO: Necesitamos asignarle algun atributo a la FechaCreacion y FechaModificacion para indicar que son especiales.
            columnsFound.Add("FechaCreacion");
            columnsFound.Add("FechaModificacion");

            ColumnDetail columnDetail;

            queryBuilder.AppendFormat("ALTER TABLE {0}{1} \n", TablePrefix, obj.DataBaseTableName);

            // Aqui se agregan las propiedades que no estan en la tabla. Tambien guarda en una lista las columnas encontradas.
            foreach (PropertyInfo property in properties)
            {
                columnDetails.TryGetValue(property.Name, out columnDetail);
                /* TODO: 
                 * Esta funcion DEBE poder identificar los tipos de las variables y saber si se han modificado asi como borrar la columna en caso de ya no existir en el modelo.
                 * Por ahora, solo identifica si la propiedad existe o no para crearla.
                */
                if (columnDetail == null)
                {
                    queryBuilder.AppendFormat("ADD {0} {1} NOT NULL,\n", property.Name, GetSqlDataType(property.PropertyType));
                    continue;
                }
                columnsFound.Add(property.Name);
            }

            // Extraemos las columnas en la tabla que ya no estan en las propiedades del modelo para quitarlas.
            foreach (KeyValuePair<string, ColumnDetail> detail in columnDetails.Where(q => !columnsFound.Contains(q.Key)))
            {
                queryBuilder.AppendFormat("DROP {0},\n", detail.Value.Field);
                continue;
            }

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(";");

            return queryBuilder.ToString();
        }

        public string GetCreateForeignKeysQuery(Type type)
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<ForeignModel>() != null).ToArray();
            IManageable obj = (IManageable)Activator.CreateInstance(type);

            if (properties.Length == 0) return string.Empty;

            queryBuilder.AppendFormat("ALTER TABLE {0}{1} ", TablePrefix, obj.DataBaseTableName);

            foreach (PropertyInfo property in properties)
            {
                ForeignModel foreignAttribute = property.GetCustomAttribute<ForeignModel>();
                IManageable foreignModel = (IManageable)Activator.CreateInstance(foreignAttribute.Model);
                queryBuilder.AppendFormat("ADD CONSTRAINT FK_{0}_{1} ", obj.DataBaseTableName, foreignModel.DataBaseTableName);
                queryBuilder.AppendFormat("FOREIGN KEY({0}) REFERENCES {1}{2}(Id) ON DELETE {3} ON UPDATE NO ACTION;", property.Name, TablePrefix, foreignModel.DataBaseTableName, foreignAttribute.Action.ToString().Replace("_", " "));
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
                    return "boolean";
                case "bool":
                    return "boolean";
                case "guid":
                    return "char(36)";
                case "string":
                    return "varchar(255)";
                case "datetime":
                    return "datetime";
                case "decimal":
                    return "decimal(18,2)";
                case "single":
                    return "float";
                case "float":
                    return "float";
                case "double":
                    return "double";
                case "byte":
                    return "tinyint(3)";
                case "sbyte":
                    return "tinyint(3)";
                case "byte[]":
                    return "binary";
                case "short":
                    return "smallint(5)";
                case "ushort":
                    return "mediumint(5) unsigned";
                case "int":
                    return "int(10)";
                case "uint":
                    return "int(10) unsigned";
                case "int16":
                    return "smallint(5)";
                case "int32":
                    return "int(10)";
                case "uint32":
                    return "int(10) unsigned";
                case "int64":
                    return "bigint(20)";
                case "uint64":
                    return "bigint(20) unsigned";
                case "long":
                    return "bigint(20)";
                case "ulong":
                    return "bigint(20) unsigned";
                default:
                    return "varchar(255)";
            }
        }
    }
}
