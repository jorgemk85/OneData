using DataManagement.Attributes;
using DataManagement.Enums;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataManagement.DAO
{
    public class Creation
    {
        static string tablePrefix;
        static string storedProcedurePrefix;
        static string insertSuffix;
        static string selectSuffix;
        static string selectAllSuffix;
        static string updateSuffix;
        static string deleteSuffix;

        static Creation()
        {
            tablePrefix = ConsolidationTools.GetValueFromConfiguration("TablePrefix", ConfigurationTypes.AppSetting);
            storedProcedurePrefix = ConsolidationTools.GetValueFromConfiguration("StoredProcedurePrefix", ConfigurationTypes.AppSetting);
            insertSuffix = ConsolidationTools.GetValueFromConfiguration("InsertSuffix", ConfigurationTypes.AppSetting);
            selectSuffix = ConsolidationTools.GetValueFromConfiguration("SelectSuffix", ConfigurationTypes.AppSetting);
            selectAllSuffix = ConsolidationTools.GetValueFromConfiguration("SelectAllSuffix", ConfigurationTypes.AppSetting);
            updateSuffix = ConsolidationTools.GetValueFromConfiguration("UpdateSuffix", ConfigurationTypes.AppSetting);
            deleteSuffix = ConsolidationTools.GetValueFromConfiguration("DeleteSuffix", ConfigurationTypes.AppSetting);
        }

        private static void SetStoredProceduresParameters(ref PropertyInfo[] properties, StringBuilder queryBuilder, ConnectionTypes connectionType, bool setDefaultNull)
        {
            // Aqui se colocan los parametros segun las propiedades del objeto 
            foreach (PropertyInfo property in properties)
            {
                if (setDefaultNull)
                {
                    queryBuilder.AppendFormat("@_{0} {1} = null, ", property.Name, GetSqlDataType(property.PropertyType, connectionType));
                }
                else
                {
                    queryBuilder.AppendFormat("@_{0} {1}, ", property.Name, GetSqlDataType(property.PropertyType, connectionType));
                }
            }
        }

        internal static string CreateInsertStoredProcedure<T>(ConnectionTypes connectionType) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3} ", obj.Schema, storedProcedurePrefix, obj.DataBaseTableName, insertSuffix);

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(ref properties, queryBuilder, connectionType, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(" AS ");
            queryBuilder.Append("BEGIN ");
            queryBuilder.Append("DECLARE @actualTime datetime;");
            queryBuilder.Append("SET @actualTime = getdate();");
            queryBuilder.AppendFormat("INSERT INTO {0}.{1}{2} (", obj.Schema, tablePrefix, obj.DataBaseTableName);

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

        internal static string CreateUpdateStoredProcedure<T>(ConnectionTypes connectionType) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3} ", obj.Schema, storedProcedurePrefix, obj.DataBaseTableName, updateSuffix);

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(ref properties, queryBuilder, connectionType, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(" AS ");
            queryBuilder.Append("BEGIN ");
            queryBuilder.Append("DECLARE @actualTime datetime;");
            queryBuilder.Append("SET @actualTime = getdate();");
            queryBuilder.AppendFormat("UPDATE {0}.{1}{2} ", obj.Schema, tablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("SET ");

            // Se especifica el parametro que va en x columna.
            foreach (PropertyInfo property in properties)
            {
                queryBuilder.AppendFormat("{0} =  @_{0}, ", property.Name);
            }
            queryBuilder.Append("fechaModificacion = @actualTime ");
            queryBuilder.AppendFormat(" WHERE Id = @_Id; ");
            queryBuilder.Append("END ");

            return queryBuilder.ToString();
        }

        internal static string CreateDeleteStoredProcedure<T>(ConnectionTypes connectionType) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3} ", obj.Schema, storedProcedurePrefix, obj.DataBaseTableName, deleteSuffix);
            queryBuilder.Append("@_Id uniqueidentifier ");
            queryBuilder.Append(" AS ");
            queryBuilder.Append("BEGIN ");
            queryBuilder.AppendFormat("DELETE FROM {0}.{1}{2} ", obj.Schema, tablePrefix, obj.DataBaseTableName);
            queryBuilder.AppendFormat("WHERE Id = @_Id; ");
            queryBuilder.Append("END ");

            return queryBuilder.ToString();
        }

        internal static string CreateSelectAllStoredProcedure<T>(ConnectionTypes connectionType) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3} ", obj.Schema, storedProcedurePrefix, obj.DataBaseTableName, selectAllSuffix);
            queryBuilder.Append(" AS ");
            queryBuilder.Append("BEGIN ");
            queryBuilder.AppendFormat("SELECT * FROM {0}.{1}{2} ", obj.Schema, tablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("END ");

            return queryBuilder.ToString();
        }

        internal static string CreateSelectStoredProcedure<T>(ConnectionTypes connectionType) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3} ", obj.Schema, storedProcedurePrefix, obj.DataBaseTableName, selectSuffix);

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(ref properties, queryBuilder, connectionType, true);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(" AS ");
            queryBuilder.Append("BEGIN ");
            queryBuilder.AppendFormat("SELECT * FROM {0}.{1}{2} ", obj.Schema, tablePrefix, obj.DataBaseTableName);
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

        internal static string GetCreateTableQuery<T>(ConnectionTypes connectionType) where T : IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            return CreateQueryForTableCreation(obj, ref properties, connectionType);
        }

        internal static string GetCreateTableQuery(Type type, ConnectionTypes connectionType)
        {
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null).ToArray();
            IManageable obj = (IManageable)Activator.CreateInstance(type);

            if (properties.Length == 0) return string.Empty;

            return CreateQueryForTableCreation(obj, ref properties, connectionType);
        }

        private static string CreateQueryForTableCreation(IManageable obj, ref PropertyInfo[] properties, ConnectionTypes connectionType)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.AppendFormat("CREATE TABLE {0}.{1}{2} ", obj.Schema, tablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("(");
            // Aqui se colocan las propiedades del objeto. Una por columna por su puesto.
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.Equals("Id"))
                {
                    queryBuilder.AppendFormat("{0} {1} NOT NULL PRIMARY KEY, ", property.Name, GetSqlDataType(property.PropertyType, connectionType));
                }
                else
                {
                    queryBuilder.AppendFormat("{0} {1} NOT NULL, ", property.Name, GetSqlDataType(property.PropertyType, connectionType));
                }
            }
            queryBuilder.Append("FechaCreacion datetime, FechaModificacion datetime);");

            return queryBuilder.ToString();
        }

        internal static string GetCreateForeignKeysQuery(Type type, ConnectionTypes connectionType)
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<ForeignModel>() != null).ToArray();
            IManageable obj = (IManageable)Activator.CreateInstance(type);

            if (properties.Length == 0) return string.Empty;

            queryBuilder.AppendFormat("ALTER TABLE {0}.{1}{2} ", obj.Schema, tablePrefix, obj.DataBaseTableName);

            foreach (PropertyInfo property in properties)
            {
                IManageable foreignModel = (IManageable)Activator.CreateInstance(property.GetCustomAttribute<ForeignModel>().Model);
                queryBuilder.AppendFormat("ADD CONSTRAINT FK_{0}_{1} ", obj.DataBaseTableName, foreignModel.DataBaseTableName);
                queryBuilder.AppendFormat("FOREIGN KEY({0}) REFERENCES {1}.{2}{3}(Id);", property.Name, obj.Schema, tablePrefix, foreignModel.DataBaseTableName);
            }

            return queryBuilder.ToString();
        }

        private static string GetSqlDataType(Type codeType, ConnectionTypes connectionType)
        {
            Type underlyingType = Nullable.GetUnderlyingType(codeType);

            if (underlyingType == null)
            {
                underlyingType = codeType;
            }

            // TODO: Filtrar y retornar el valor correcto segun connectionType
            switch (underlyingType.Name.ToLower())
            {
                case "guid":
                    return "uniqueidentifier";
                case "string":
                    return "varchar(255)";
                case "datetime":
                    return "datetime";
                case "decimal":
                    return "decimal (18,2)";
                case "float":
                    return "float";
                case "int":
                    return "int";
                case "int32":
                    return "int";
                case "int64":
                    return "bigint";
                default:
                    return "varchar(255)";
            }
        }
    }
}
