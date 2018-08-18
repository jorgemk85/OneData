using DataManagement.Attributes;
using DataManagement.Interfaces;
using System;
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
                    queryBuilder.AppendFormat("IN _{0} {1} = null, ", property.Name, GetSqlDataType(property.PropertyType));
                }
                else
                {
                    queryBuilder.AppendFormat("IN _{0} {1}, ", property.Name, GetSqlDataType(property.PropertyType));
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
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}{1}{2} (",  StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}{1}{2} (",  StoredProcedurePrefix, obj.DataBaseTableName, InsertSuffix);
            }

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(ref properties, queryBuilder, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(") BEGIN ");
            queryBuilder.Append("SET @actualTime = now(); ");
            queryBuilder.AppendFormat("INSERT INTO {0}{1} ( ",  TablePrefix, obj.DataBaseTableName);

            // Seccion para especificar a que columnas se va a insertar.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("{0}, ", property.Name);
            }

            queryBuilder.Append("fechaCreacion, fechaModificacion");
            queryBuilder.Append(") VALUES ( ");

            // Especificamos los parametros para insertar en la base de datos.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("_{0}, ", property.Name);
            }

            queryBuilder.Append("@actualTime, @actualTime); ");
            queryBuilder.Append("END ");

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
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}{1}{2} (", StoredProcedurePrefix, obj.DataBaseTableName, UpdateSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}{1}{2} (", StoredProcedurePrefix, obj.DataBaseTableName, UpdateSuffix);
            }


            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(ref properties, queryBuilder, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(") BEGIN ");
            queryBuilder.Append("SET @actualTime = now();");
            queryBuilder.AppendFormat("UPDATE {0}{1} ",  TablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("SET ");

            // Se especifica el parametro que va en x columna.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("{0} =  _{0}, ", property.Name);
            }
            queryBuilder.Append("fechaModificacion = @actualTime ");
            queryBuilder.AppendFormat(" WHERE Id = _Id; ");
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
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}{1}{2} (", StoredProcedurePrefix, obj.DataBaseTableName, DeleteSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}{1}{2} (",  StoredProcedurePrefix, obj.DataBaseTableName, DeleteSuffix);
            }

            queryBuilder.Append("_Id char(36) ");
            queryBuilder.Append(") BEGIN ");
            queryBuilder.AppendFormat("DELETE FROM {0}{1} ", TablePrefix, obj.DataBaseTableName);
            queryBuilder.AppendFormat("WHERE Id = _Id; ");
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
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}{1}{2} (", StoredProcedurePrefix, obj.DataBaseTableName, SelectAllSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}{1}{2} (", StoredProcedurePrefix, obj.DataBaseTableName, SelectAllSuffix);
            }
            
            queryBuilder.Append(") BEGIN ");
            queryBuilder.AppendFormat("SELECT * FROM {0}{1} ", TablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("ORDER BY FechaCreacion DESC; ");
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
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}{1}{2} (",  StoredProcedurePrefix, obj.DataBaseTableName, SelectSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}{1}{2} (",  StoredProcedurePrefix, obj.DataBaseTableName, SelectSuffix);
            }

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(ref properties, queryBuilder, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(") BEGIN ");
            queryBuilder.AppendFormat("SELECT * FROM {0}{1} ",  TablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("WHERE ");

            // Se especifica el parametro que va en x columna.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("{0} LIKE IFNULL(CONCAT('%', _{0}, '%'), {0}) AND ", property.Name);
            }

            queryBuilder.Remove(queryBuilder.Length - 5, 5);
            queryBuilder.AppendFormat(" ORDER BY FechaCreacion desc;");
            queryBuilder.Append("END");

            return queryBuilder.ToString();
        }

        public string GetCreateTableQuery<T>(bool doAlter) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            return CreateQueryForTableCreation(obj, ref properties, doAlter);
        }

        public string GetCreateTableQuery(Type type, bool doAlter)
        {
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            IManageable obj = (IManageable)Activator.CreateInstance(type);

            if (properties.Length == 0) return string.Empty;

            return CreateQueryForTableCreation(obj, ref properties, doAlter);
        }

        public string CreateQueryForTableCreation(IManageable obj, ref PropertyInfo[] properties, bool doAlter)
        {
            StringBuilder queryBuilder = new StringBuilder();
            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER TABLE {0}{1} (", TablePrefix, obj.DataBaseTableName);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE TABLE {0}{1} (",  TablePrefix, obj.DataBaseTableName);
            }

            // Aqui se colocan las propiedades del objeto. Una por columna por su puesto.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("{0} {1} NOT NULL, ", property.Name, GetSqlDataType(property.PropertyType));
            }
            queryBuilder.Append("FechaCreacion datetime NOT NULL,  FechaModificacion datetime NOT NULL, ");
            queryBuilder.Append("PRIMARY KEY (Id));");

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
                IManageable foreignModel = (IManageable)Activator.CreateInstance(property.GetCustomAttribute<ForeignModel>().Model);
                queryBuilder.AppendFormat("ADD CONSTRAINT FK_{0}_{1} ", obj.DataBaseTableName, foreignModel.DataBaseTableName);
                queryBuilder.AppendFormat("FOREIGN KEY({0}) REFERENCES {1}{2}(Id);", property.Name, TablePrefix, foreignModel.DataBaseTableName);
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
                    return "decimal (18,2)";
                case "single":
                    return "float";
                case "float":
                    return "float";
                case "double":
                    return "double";
                case "byte":
                    return "tinyint (3)";
                case "sbyte":
                    return "tinyint (3)";
                case "byte[]":
                    return "binary";
                case "short":
                    return "smallint(5)";
                case "ushort":
                    return "mediumint(5)";
                case "int":
                    return "int(10)";
                case "uint":
                    return "int(10)";
                case "int16":
                    return "smallint(5)";
                case "int32":
                    return "int(10)";
                case "uint32":
                    return "int(10)";
                case "int64":
                    return "bigint (20)";
                case "uint64":
                    return "bigint (20)";
                case "long":
                    return "bigint (20)";
                case "ulong":
                    return "bigint (20)";
                default:
                    return "varchar(255)";
            }
        }
    }
}
