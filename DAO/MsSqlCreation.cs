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
    internal class MsSqlCreation : ICreatable
    {
        public void SetStoredProceduresParameters<T, TKey>(StringBuilder queryBuilder, bool setDefaultNull, bool considerPrimary) where T : Cope<T, TKey>, new() where TKey : struct
        {
            // Aqui se colocan los parametros segun las propiedades del objeto 
            foreach (KeyValuePair<string, PropertyInfo> property in Manager<T, TKey>.ModelComposition.ManagedProperties)
            {
                // Si la propiedad actual es la primaria y esta es del tipo int? y no se debe considerar para estos parametros, entonces se salta a la sig propiedad.
                // Esto significa que la propiedad primaria es Identity o Auto Increment y no se debe de mandar como parametro en un Insert.
                if (property.Value.Equals(Manager<T, TKey>.ModelComposition.PrimaryProperty) && property.Value.PropertyType.Equals(typeof(int?)) && !considerPrimary)
                {
                    continue;
                }
                // Si la propiedad es DateCreated o DateModified o AutoProperty, no se debe mandar como parametro
                // Esto es por que estos valores se alimentan desde el procedimiento almacenado.
                if (Manager<T, TKey>.ModelComposition.AutoProperties.ContainsKey(property.Value.Name))
                {
                    continue;
                }
                if (setDefaultNull)
                {
                    queryBuilder.AppendFormat("    @_{0} {1} = null,\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType));
                }
                else
                {
                    queryBuilder.AppendFormat("    @_{0} {1},\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType));
                }
            }
        }

        public string CreateInsertStoredProcedure<T, TKey>(bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            StringBuilder insertsBuilder = new StringBuilder();
            StringBuilder valuesBuilder = new StringBuilder();
            T obj = new T();

            if (Manager<T, TKey>.ModelComposition.ManagedProperties.Count == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Manager<T, TKey>.ModelComposition.TableName, Manager.InsertSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Manager<T, TKey>.ModelComposition.TableName, Manager.InsertSuffix);
            }

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters<T, TKey>(queryBuilder, false, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append("\nAS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.Append("DECLARE @actualTime datetime;\n");
            queryBuilder.Append("SET @actualTime = getdate();\n");
            queryBuilder.AppendFormat("INSERT INTO {0}.{1}{2} (\n", Manager<T, TKey>.ModelComposition.Schema, Manager.TablePrefix, Manager<T, TKey>.ModelComposition.TableName);

            // Seccion para especificar a que columnas se va a insertar y sus valores.
            foreach (KeyValuePair<string, PropertyInfo> property in Manager<T, TKey>.ModelComposition.ManagedProperties)
            {
                if (property.Value.Equals(Manager<T, TKey>.ModelComposition.PrimaryProperty) && property.Value.PropertyType.Equals(typeof(int?)))
                {
                    continue;
                }
                else
                {
                    insertsBuilder.AppendFormat("    {0},\n", property.Value.Name);
                    if (Manager<T, TKey>.ModelComposition.AutoProperties.TryGetValue(property.Value.Name, out PropertyInfo autoProperty))
                    {
                        valuesBuilder.AppendFormat("    {0},\n", GetAutoPropertyValue(Manager<T, TKey>.ModelComposition.AutoPropertyAttributes[property.Value.Name].AutoPropertyType));
                    }
                    else
                    {
                        valuesBuilder.AppendFormat("    @_{0},\n", property.Value.Name);
                    }
                }
            }
            insertsBuilder.Remove(insertsBuilder.Length - 2, 2);
            queryBuilder.Append(insertsBuilder);
            queryBuilder.Append(")\nVALUES (\n");
            valuesBuilder.Remove(valuesBuilder.Length - 2, 2);
            queryBuilder.Append(valuesBuilder);
            queryBuilder.Append(");\nEND");

            Logger.Info("Created a new query for Insert Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateUpdateStoredProcedure<T, TKey>(bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            T obj = new T();

            if (Manager<T, TKey>.ModelComposition.ManagedProperties.Count == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Manager<T, TKey>.ModelComposition.TableName, Manager.UpdateSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Manager<T, TKey>.ModelComposition.TableName, Manager.UpdateSuffix);
            }


            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters<T, TKey>(queryBuilder, false, true);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append("\nAS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.Append("DECLARE @actualTime datetime;\n");
            queryBuilder.Append("SET @actualTime = getdate();\n");
            queryBuilder.AppendFormat("UPDATE {0}.{1}{2}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.TablePrefix, Manager<T, TKey>.ModelComposition.TableName);
            queryBuilder.Append("SET\n");

            // Se especifica el parametro que va en x columna.
            foreach (KeyValuePair<string, PropertyInfo> property in Manager<T, TKey>.ModelComposition.ManagedProperties)
            {
                if (property.Value.Equals(Manager<T, TKey>.ModelComposition.PrimaryProperty) || property.Value.Name.Equals(Manager<T, TKey>.ModelComposition.DateCreatedProperty.Name))
                {
                    continue;
                }
                if (Manager<T, TKey>.ModelComposition.AutoProperties.TryGetValue(property.Value.Name, out PropertyInfo autoProperty))
                {
                    queryBuilder.AppendFormat("    {0} = {1},\n", property.Value.Name, GetAutoPropertyValue(Manager<T, TKey>.ModelComposition.AutoPropertyAttributes[property.Value.Name].AutoPropertyType));
                }
                else
                {
                    queryBuilder.AppendFormat("    {0} = ISNULL(@_{0}, {0}),\n", property.Value.Name);
                }
            }
            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.AppendFormat("WHERE Id = @_Id;\n");
            queryBuilder.Append("END");

            Logger.Info("Created a new query for Update Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateDeleteStoredProcedure<T, TKey>(bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            T obj = new T();

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Manager<T, TKey>.ModelComposition.TableName, Manager.DeleteSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Manager<T, TKey>.ModelComposition.TableName, Manager.DeleteSuffix);
            }

            queryBuilder.Append(string.Format("@_Id {0}\n", GetSqlDataType(typeof(TKey))));
            queryBuilder.Append("AS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("DELETE FROM {0}.{1}{2}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.TablePrefix, Manager<T, TKey>.ModelComposition.TableName);
            queryBuilder.AppendFormat("WHERE Id = @_Id;\n");
            queryBuilder.Append("END");

            Logger.Info("Created a new query for Delete Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateSelectAllStoredProcedure<T, TKey>(bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            T obj = new T();

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Manager<T, TKey>.ModelComposition.TableName, Manager.SelectAllSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Manager<T, TKey>.ModelComposition.TableName, Manager.SelectAllSuffix);
            }

            queryBuilder.Append("AS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("SELECT * FROM {0}.{1}{2}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.TablePrefix, Manager<T, TKey>.ModelComposition.TableName);
            queryBuilder.Append("ORDER BY FechaCreacion DESC\n");
            queryBuilder.Append("END");

            Logger.Info("Created a new query for SelectAll Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateSelectStoredProcedure<T, TKey>(bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            T obj = new T();

            if (Manager<T, TKey>.ModelComposition.FilteredProperties.Count == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Manager<T, TKey>.ModelComposition.TableName, Manager.SelectSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Manager<T, TKey>.ModelComposition.TableName, Manager.SelectSuffix);
            }

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters<T, TKey>(queryBuilder, true, true);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append("\nAS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("SELECT * FROM {0}.{1}{2}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.TablePrefix, Manager<T, TKey>.ModelComposition.TableName);
            queryBuilder.Append("WHERE\n");

            // Se especifica el parametro que va en x columna.
            foreach (KeyValuePair<string, PropertyInfo> property in Manager<T, TKey>.ModelComposition.FilteredProperties)
            {
                queryBuilder.AppendFormat("    {0} LIKE ISNULL(CONCAT('%', @_{0}, '%'), {0}) AND\n", property.Value.Name);
            }

            queryBuilder.Remove(queryBuilder.Length - 4, 4);
            queryBuilder.AppendFormat("\nORDER BY {0} desc;\n", Manager<T, TKey>.ModelComposition.DateCreatedProperty.Name);
            queryBuilder.Append("END");

            Logger.Info("Created a new query for Select Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateQueryForTableCreation<T, TKey>() where T : Cope<T, TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            T obj = new T();

            if (Manager<T, TKey>.ModelComposition.ManagedProperties.Count == 0) return string.Empty;

            queryBuilder.AppendFormat("CREATE TABLE {0}.{1}{2}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.TablePrefix, Manager<T, TKey>.ModelComposition.TableName);

            queryBuilder.Append("(");
            // Aqui se colocan las propiedades del objeto. Una por columna por su puesto.
            foreach (KeyValuePair<string, PropertyInfo> property in Manager<T, TKey>.ModelComposition.ManagedProperties)
            {
                string isNullable = Nullable.GetUnderlyingType(property.Value.PropertyType) == null || property.Value.Equals(Manager<T, TKey>.ModelComposition.PrimaryProperty) ? "NOT NULL" : string.Empty;
                if (property.Value.Equals(Manager<T, TKey>.ModelComposition.PrimaryProperty))
                {
                    if (property.Value.PropertyType.Equals(typeof(int?)))
                    {
                        queryBuilder.AppendFormat("{0} {1} IDENTITY(1,1) NOT NULL PRIMARY KEY,\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType));
                    }
                    else
                    {
                        queryBuilder.AppendFormat("{0} {1} NOT NULL PRIMARY KEY,\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType));
                    }
                }
                else
                {
                    queryBuilder.AppendFormat("{0} {1} {2},\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType), isNullable);
                }
            }
            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(");");

            Logger.Info("Created a new query for Create Table:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateQueryForTableAlteration<T, TKey>(Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, KeyDefinition> keyDetails) where T : Cope<T, TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            List<string> columnsFound = new List<string>();
            bool foundDiference = false;
            T obj = new T();

            if (Manager<T, TKey>.ModelComposition.ManagedProperties.Count == 0) return string.Empty;

            string fullyQualifiedTableName = string.Format("{0}.{1}{2}", Manager<T, TKey>.ModelComposition.Schema, Manager.TablePrefix, Manager<T, TKey>.ModelComposition.TableName);

            foreach (KeyValuePair<string, PropertyInfo> property in Manager<T, TKey>.ModelComposition.ManagedProperties)
            {
                columnDetails.TryGetValue(property.Value.Name, out ColumnDefinition columnDefinition);
                string sqlDataType = GetSqlDataType(property.Value.PropertyType);
                bool isNullable = Nullable.GetUnderlyingType(property.Value.PropertyType) == null ? false : true;
                string nullWithDefault = isNullable == true ? string.Empty : string.Format("NOT NULL DEFAULT {0}", GetDefault(property.Value.PropertyType));

                if (columnDefinition == null)
                {
                    // Agregar propiedad a tabla ya que no existe.
                    queryBuilder.AppendFormat("ALTER TABLE {0} \n", fullyQualifiedTableName);
                    queryBuilder.AppendFormat("ADD {0} {1} {2};\n", property.Value.Name, sqlDataType, nullWithDefault);
                    foundDiference = true;
                    continue;
                }
                columnDefinition.Column_Type = columnDefinition.Character_Maximum_Length != null ? string.Format("{0}({1})", columnDefinition.Data_Type, columnDefinition.Character_Maximum_Length) : columnDefinition.Data_Type;
                if (!sqlDataType.Equals(columnDefinition.Column_Type))
                {
                    // Si el data type cambio, entonces lo modifica.
                    queryBuilder.AppendFormat("ALTER TABLE {0} \n", fullyQualifiedTableName);
                    queryBuilder.AppendFormat("ALTER COLUMN {0} {1};\n", property.Value.Name, sqlDataType);
                    foundDiference = true;
                }
                if (columnDefinition.Is_Nullable.Equals("YES") && !isNullable && !property.Value.Equals(Manager<T, TKey>.ModelComposition.PrimaryProperty))
                {
                    // Si la propiedad ya no es nullable, entonces la cambia en la base de datos
                    queryBuilder.AppendFormat("ALTER TABLE {0} \n", fullyQualifiedTableName);
                    queryBuilder.AppendFormat("ALTER COLUMN {0} {1} NOT NULL;\n", property.Value.Name, sqlDataType);
                    foundDiference = true;
                }
                if (columnDefinition.Is_Nullable.Equals("NO") && isNullable && !property.Value.Equals(Manager<T, TKey>.ModelComposition.PrimaryProperty))
                {
                    // Si la propiedad ES nullable, entonces la cambia en la base de datos
                    queryBuilder.AppendFormat("ALTER TABLE {0} \n", fullyQualifiedTableName);
                    queryBuilder.AppendFormat("ALTER COLUMN {0} {1};\n", property.Value.Name, sqlDataType);
                    foundDiference = true;
                }
                if (keyDetails.TryGetValue(property.Value.Name, out KeyDefinition keyDefinition))
                {
                    // Si existe una llave en la base de datos relacionada a esta propiedad entonces...
                    ForeignModel foreignAttribute = property.Value.GetCustomAttribute<ForeignModel>();
                    if (foreignAttribute == null)
                    {
                        // En el caso de que no tenga ya el atributo, significa que dejo de ser una propiedad relacionada con algun modelo foraneo y por ende, debemos de eliminar la llave foranea
                        queryBuilder.AppendFormat("ALTER TABLE {0} \n", fullyQualifiedTableName);
                        queryBuilder.AppendFormat("DROP FOREIGN KEY {0};\n", keyDefinition.Constraint_Name);
                        keyDetails.Remove(property.Value.Name);
                        foundDiference = true;
                    }
                }
                columnsFound.Add(property.Value.Name);
            }

            // Extraemos las columnas en la tabla que ya no estan en las propiedades del modelo para quitarlas.
            foreach (KeyValuePair<string, ColumnDefinition> detail in columnDetails.Where(q => !columnsFound.Contains(q.Key)))
            {
                queryBuilder.AppendFormat("ALTER TABLE {0} \n", fullyQualifiedTableName);
                queryBuilder.AppendFormat("DROP COLUMN {0};\n", detail.Value.Column_Name);
                foundDiference = true;
                continue;
            }

            if (!foundDiference)
            {
                queryBuilder.Clear();
            }
            else
            {
                Logger.Info("Created a new query for Alter Table:");
                Logger.Info(queryBuilder.ToString());
            }

            queryBuilder.Append(GetCreateForeignKeysQuery<T, TKey>(keyDetails));

            return queryBuilder.ToString();
        }

        public object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                object value = Activator.CreateInstance(type);
                if (string.IsNullOrWhiteSpace(value.ToString()))
                {
                    value = "''";
                }
                return value;
            }
            return null;
        }

        public string GetCreateForeignKeysQuery<T, TKey>(Dictionary<string, KeyDefinition> keyDetails = null) where T : Cope<T, TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            Dictionary<string, PropertyInfo> properties = Manager<T, TKey>.ModelComposition.ForeignModelProperties.Where(q => !keyDetails.ContainsKey(q.Value.Name)).ToDictionary(q => q.Key, q => q.Value);

            if (properties.Count == 0) return string.Empty;

            queryBuilder.AppendFormat("ALTER TABLE {0}.{1}{2}\n", Manager<T, TKey>.ModelComposition.Schema, Manager.TablePrefix, Manager<T, TKey>.ModelComposition.TableName);

            foreach (KeyValuePair<string, PropertyInfo> property in properties)
            {
                ForeignModel foreignAttribute = property.Value.GetCustomAttribute<ForeignModel>();
                Cope<T, TKey> foreignModel = (Cope<T, TKey>)Activator.CreateInstance(foreignAttribute.Model);
                queryBuilder.AppendFormat("ADD CONSTRAINT FK_{0}_{1}\n", Manager<T, TKey>.ModelComposition.TableName, foreignModel.ModelComposition.TableName);
                queryBuilder.AppendFormat("FOREIGN KEY({0}) REFERENCES {1}.{2}{3}(Id) ON DELETE {4} ON UPDATE NO ACTION;\n", property.Value.Name, Manager<T, TKey>.ModelComposition.Schema, Manager.TablePrefix, foreignModel.ModelComposition.TableName, foreignAttribute.Action.ToString().Replace("_", " "));
            }

            Logger.Info("Created a new query for Create Foreign Keys:");
            Logger.Info(queryBuilder.ToString());
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
                case "char":
                    return "char(1)";
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
                    return "varbinary(1024)";
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
                    return "varbinary(1024)";
            }
        }

        private string GetAutoPropertyValue(AutoPropertyTypes type)
        {
            switch (type)
            {
                case AutoPropertyTypes.Date:
                    return "CONVERT(DATE, GETDATE()) ";
                case AutoPropertyTypes.DateTime:
                    return "GETDATE()";
                default:
                    return "GETDATE()";
            }
        }

        public string CreateInsertListStoredProcedure<T, TKey>(bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct
        {
            throw new NotImplementedException();
        }
    }
}
