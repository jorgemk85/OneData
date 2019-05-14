using OneData.Attributes;
using OneData.Enums;
using OneData.Interfaces;
using OneData.Models;
using OneData.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OneData.DAO
{
    internal class MsSqlCreation : ICreatable, IValidatable
    {
        public void SetStoredProceduresParameters<T>(StringBuilder queryBuilder, bool setDefaultNull, bool considerPrimary) where T : Cope<T>, IManageable, new()
        {
            // Aqui se colocan los parametros segun las propiedades del objeto 
            foreach (KeyValuePair<string, PropertyInfo> property in Cope<T>.ModelComposition.ManagedProperties)
            {
                // Si la propiedad actual es la primaria y esta es del tipo int? y no se debe considerar para estos parametros, entonces se salta a la sig propiedad.
                // Esto significa que la propiedad primaria es Identity o Auto Increment y no se debe de mandar como parametro en un Insert.
                if (property.Value.Equals(Cope<T>.ModelComposition.PrimaryKeyProperty) && property.Value.PropertyType.Equals(typeof(int?)) && !considerPrimary)
                {
                    continue;
                }
                // Si la propiedad es DateCreated o DateModified o AutoProperty, no se debe mandar como parametro
                // Esto es por que estos valores se alimentan desde el procedimiento almacenado.
                if (Cope<T>.ModelComposition.AutoProperties.ContainsKey(property.Value.Name))
                {
                    continue;
                }
                if (setDefaultNull)
                {
                    queryBuilder.AppendFormat("    @_{0} {1} = null,\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType, Cope<T>.ModelComposition.UniqueKeyProperties.ContainsKey(property.Value.Name), GetDataLengthFromProperty<T>(property.Key)));
                }
                else
                {
                    queryBuilder.AppendFormat("    @_{0} {1},\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType, Cope<T>.ModelComposition.UniqueKeyProperties.ContainsKey(property.Value.Name), GetDataLengthFromProperty<T>(property.Key)));
                }
            }
        }

        private long GetDataLengthFromProperty<T>(string propertyName) where T : Cope<T>, IManageable, new()
        {
            Cope<T>.ModelComposition.DataLengthAttributes.TryGetValue(propertyName, out DataLength dataLengthAttribute);

            if (dataLengthAttribute != null)
            {
                return dataLengthAttribute.Length;
            }
            else
            {
                return 0;
            }
        }

        private long GetDataLengthFromProperty(IManageable model, string propertyName)
        {
            model.Configuration.DataLengthAttributes.TryGetValue(propertyName, out DataLength dataLengthAttribute);

            if (dataLengthAttribute != null)
            {
                return dataLengthAttribute.Length;
            }
            else
            {
                return 0;
            }
        }

        private void SetParametersForQueryOptions<T>(StringBuilder queryBuilder) where T : Cope<T>, IManageable, new()
        {
            foreach (PropertyInfo property in typeof(QueryOptions).GetProperties().Where(options => options.GetCustomAttribute(typeof(NotParameter)) == null).OrderBy(option => option.Name))
            {
                queryBuilder.AppendFormat("    IN _{0} {1},\n", property.Name, GetSqlDataType(property.PropertyType, Cope<T>.ModelComposition.UniqueKeyProperties.ContainsKey(property.Name), GetDataLengthFromProperty<T>(property.Name)));
            }
            queryBuilder.Remove(queryBuilder.Length - 2, 2);
        }

        public string CreateInsertStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            StringBuilder insertsBuilder = new StringBuilder();
            StringBuilder valuesBuilder = new StringBuilder();
            T obj = new T();

            if (Cope<T>.ModelComposition.ManagedProperties.Count == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", Cope<T>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, Manager.InsertSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", Cope<T>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, Manager.InsertSuffix);
            }

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters<T>(queryBuilder, false, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append("\nAS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("INSERT INTO {0}.{1}{2} (\n", Cope<T>.ModelComposition.Schema, Manager.TablePrefix, Cope<T>.ModelComposition.TableName);

            // Seccion para especificar a que columnas se va a insertar y sus valores.
            foreach (KeyValuePair<string, PropertyInfo> property in Cope<T>.ModelComposition.ManagedProperties)
            {
                if (property.Value.Equals(Cope<T>.ModelComposition.PrimaryKeyProperty) && property.Value.PropertyType.Equals(typeof(int?)))
                {
                    continue;
                }
                else
                {
                    insertsBuilder.AppendFormat("    {0},\n", property.Value.Name);
                    if (Cope<T>.ModelComposition.AutoProperties.TryGetValue(property.Value.Name, out PropertyInfo autoProperty))
                    {
                        valuesBuilder.AppendFormat("    {0},\n", GetAutoPropertyValue(Cope<T>.ModelComposition.AutoPropertyAttributes[property.Value.Name].AutoPropertyType));
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

        public string CreateUpdateStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            T obj = new T();

            if (Cope<T>.ModelComposition.ManagedProperties.Count == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", Cope<T>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, Manager.UpdateSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", Cope<T>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, Manager.UpdateSuffix);
            }


            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters<T>(queryBuilder, false, true);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append("\nAS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("UPDATE {0}.{1}{2}\n", Cope<T>.ModelComposition.Schema, Manager.TablePrefix, Cope<T>.ModelComposition.TableName);
            queryBuilder.Append("SET\n");

            // Se especifica el parametro que va en x columna.
            foreach (KeyValuePair<string, PropertyInfo> property in Cope<T>.ModelComposition.ManagedProperties)
            {
                if (property.Value.Equals(Cope<T>.ModelComposition.PrimaryKeyProperty) || property.Value.Name.Equals(Cope<T>.ModelComposition.DateCreatedProperty.Name))
                {
                    continue;
                }
                if (Cope<T>.ModelComposition.AutoProperties.TryGetValue(property.Value.Name, out PropertyInfo autoProperty))
                {
                    queryBuilder.AppendFormat("    {0} = {1},\n", property.Value.Name, GetAutoPropertyValue(Cope<T>.ModelComposition.AutoPropertyAttributes[property.Value.Name].AutoPropertyType));
                }
                else
                {
                    queryBuilder.AppendFormat("    {0} = ISNULL(@_{0}, {0}),\n", property.Value.Name);
                }
            }
            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.AppendFormat($"WHERE {Cope<T>.ModelComposition.PrimaryKeyProperty.Name} = @_{Cope<T>.ModelComposition.PrimaryKeyProperty.Name};\n");
            queryBuilder.Append("END");

            Logger.Info("Created a new query for Update Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateDeleteStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new()
        {
            StringBuilder queryBuilder = new StringBuilder();
            T obj = new T();

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", Cope<T>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, Manager.DeleteSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", Cope<T>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, Manager.DeleteSuffix);
            }

            queryBuilder.Append($"@_{Cope<T>.ModelComposition.PrimaryKeyProperty.Name} {GetSqlDataType(Cope<T>.ModelComposition.PrimaryKeyProperty.PropertyType, Cope<T>.ModelComposition.UniqueKeyProperties.ContainsKey(Cope<T>.ModelComposition.PrimaryKeyProperty.Name), 0)}\n");
            queryBuilder.Append("AS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("DELETE FROM {0}.{1}{2}\n", Cope<T>.ModelComposition.Schema, Manager.TablePrefix, Cope<T>.ModelComposition.TableName);
            queryBuilder.AppendFormat($"WHERE {Cope<T>.ModelComposition.PrimaryKeyProperty.Name} = @_{Cope<T>.ModelComposition.PrimaryKeyProperty.Name};\n");
            queryBuilder.Append("END");

            Logger.Info("Created a new query for Delete Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateQueryForTableCreation(IManageable model)
        {
            if (model.Configuration.ManagedProperties.Count == 0) return string.Empty;

            StringBuilder queryBuilder = new StringBuilder();
            ITransactionable sqlTransaction = new MsSqlTransaction();
            FullyQualifiedTableName tableName = new FullyQualifiedTableName(model.Configuration.Schema, $"{Manager.TablePrefix}{model.Configuration.TableName}");

            queryBuilder.Append(sqlTransaction.AddTable(tableName, model.Configuration.PrimaryKeyProperty.Name, GetSqlDataType(model.Configuration.PrimaryKeyProperty.PropertyType, false, 0), model.Configuration.PrimaryKeyProperty.PropertyType.Equals(typeof(int?)) || model.Configuration.PrimaryKeyProperty.PropertyType.Equals(typeof(int))));

            // Aqui se colocan las propiedades del objeto. Una por columna por su puesto (excepto para la primary key).
            foreach (KeyValuePair<string, PropertyInfo> property in model.Configuration.ManagedProperties.Where(q => q.Key != model.Configuration.PrimaryKeyProperty.Name))
            {
                string sqlDataType = GetSqlDataType(property.Value.PropertyType, model.Configuration.UniqueKeyProperties.ContainsKey(property.Value.Name), GetDataLengthFromProperty(model, property.Key));

                queryBuilder.Append(sqlTransaction.AddColumn(tableName, property.Value.Name, sqlDataType));
                queryBuilder.Append(!IsNullable(property.Value) ? sqlTransaction.AddNotNullToColumn(tableName, property.Value.Name, sqlDataType) : string.Empty);
                queryBuilder.Append(IsUnique(model, property.Value.Name) ? sqlTransaction.AddUniqueToColumn(tableName, property.Value.Name) : string.Empty);
                queryBuilder.Append(IsDefault(model, property.Value.Name) ? sqlTransaction.AddDefaultToColumn(tableName, property.Value.Name, model.Configuration.DefaultAttributes[property.Value.Name].Value) : string.Empty);
            }

            Logger.Info("Created a new query for Create Table:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateQueryForTableAlteration(IManageable model, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, ConstraintDefinition> constraints)
        {
            if (model.Configuration.ManagedProperties.Count == 0) return string.Empty;

            StringBuilder queryBuilder = new StringBuilder();
            ITransactionable sqlTransaction = new MsSqlTransaction();
            FullyQualifiedTableName tableName = new FullyQualifiedTableName(model.Configuration.Schema, $"{Manager.TablePrefix}{model.Configuration.TableName}");

            foreach (KeyValuePair<string, PropertyInfo> property in model.Configuration.ManagedProperties)
            {
                columnDetails.TryGetValue(property.Value.Name, out ColumnDefinition columnDefinition);
                string sqlDataType = GetSqlDataType(property.Value.PropertyType, model.Configuration.UniqueKeyProperties.ContainsKey(property.Value.Name), GetDataLengthFromProperty(model, property.Key));

                if (IsNewColumn(columnDefinition))
                {
                    queryBuilder.Append(sqlTransaction.AddColumn(tableName, property.Value.Name, sqlDataType));
                    columnDefinition = new ColumnDefinition();
                }
                if (IsColumnDataTypeChanged(columnDefinition, sqlDataType))
                {
                    if (!string.IsNullOrWhiteSpace(columnDefinition.Column_Default))
                    {
                        queryBuilder.Append(sqlTransaction.RemoveDefaultFromColumn(tableName, $"DF_{tableName.Schema}_{tableName.Table}_{property.Value.Name}"));
                        columnDefinition.Column_Default = null;
                    }

                    queryBuilder.Append(sqlTransaction.ChangeColumnDataType(tableName, property.Value.Name, sqlDataType));
                    columnDefinition.Is_Nullable = null;
                }

                queryBuilder.Append(IsNowNullable(columnDefinition, property.Value) ? sqlTransaction.RemoveNotNullFromColumn(tableName, property.Value.Name, sqlDataType) : string.Empty);
                queryBuilder.Append(IsNoLongerNullable(columnDefinition, property.Value) ? sqlTransaction.AddNotNullToColumn(tableName, property.Value.Name, sqlDataType) : string.Empty);

                queryBuilder.Append(IsNowUnique(constraints, $"UQ_{tableName.Schema}_{tableName.Table}_{property.Value.Name}", property.Value) ? sqlTransaction.AddUniqueToColumn(tableName, property.Value.Name) : string.Empty);
                queryBuilder.Append(IsNoLongerUnique(constraints, $"UQ_{tableName.Schema}_{tableName.Table}_{property.Value.Name}", property.Value) ? sqlTransaction.RemoveUniqueFromColumn(tableName, $"UQ_{tableName.Schema}_{tableName.Table}_{property.Value.Name}") : string.Empty);

                queryBuilder.Append(IsNowDefault(columnDefinition, property.Value) ? sqlTransaction.AddDefaultToColumn(tableName, property.Value.Name, model.Configuration.DefaultAttributes[property.Value.Name].Value) : string.Empty);
                queryBuilder.Append(IsDefaultChanged(columnDefinition, property.Value) ? sqlTransaction.RenewDefaultInColumn(tableName, property.Value.Name, model.Configuration.DefaultAttributes[property.Value.Name].Value) : string.Empty);
                queryBuilder.Append(IsNoLongerDefault(columnDefinition, property.Value) ? sqlTransaction.RemoveDefaultFromColumn(tableName, $"DF_{tableName.Schema}_{tableName.Table}_{property.Value.Name}") : string.Empty);

                queryBuilder.Append(IsNowPrimaryKey(constraints, $"PK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}", property.Value) ? sqlTransaction.AddPrimaryKeyToColumn(tableName, property.Value.Name) : string.Empty);
                queryBuilder.Append(IsNoLongerPrimaryKey(constraints, $"PK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}", property.Value) ? sqlTransaction.RemovePrimaryKeyFromColumn(tableName, $"PK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}") : string.Empty);

                queryBuilder.Append(IsNoLongerForeignKey(constraints, $"FK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}", property.Value) ? sqlTransaction.RemoveForeignKeyFromColumn(tableName, $"FK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}") : string.Empty);
            }

            foreach (KeyValuePair<string, ColumnDefinition> columnDetail in columnDetails.Where(q => !model.Configuration.ManagedProperties.Keys.Contains(q.Key)))
            {
                queryBuilder.Append(sqlTransaction.RemoveColumn(tableName, columnDetail.Key));
            }

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
            return 0;
        }

        public string GetCreateForeignKeysQuery(IManageable model, Dictionary<string, ConstraintDefinition> constraints = null)
        {
            StringBuilder queryBuilder = new StringBuilder();
            ITransactionable sqlTransaction = new MsSqlTransaction();
            FullyQualifiedTableName tableName = new FullyQualifiedTableName(model.Configuration.Schema, $"{Manager.TablePrefix}{model.Configuration.TableName}");

            foreach (KeyValuePair<string, PropertyInfo> property in model.Configuration.ForeignKeyProperties)
            {
                if (constraints == null)
                {
                    queryBuilder.Append(sqlTransaction.AddForeignKeyToColumn(tableName, property.Value));
                    continue;
                }
                if (!constraints.ContainsKey($"FK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}"))
                {
                    queryBuilder.Append(sqlTransaction.AddForeignKeyToColumn(tableName, property.Value));
                }
            }

            Logger.Info("Created a new query for Create Foreign Keys:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string GetSqlDataType(Type codeType, bool isUniqueKey, long dataLength)
        {
            Type underlyingType = Nullable.GetUnderlyingType(codeType);

            if (underlyingType == null)
            {
                underlyingType = codeType;
            }

            if (underlyingType.IsEnum)
            {
                return "int";
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
                    if (isUniqueKey)
                    {
                        return $"varchar({(dataLength == 0 ? 255 : dataLength > 255 ? 255 : dataLength)})";
                    }
                    else
                    {
                        return $"varchar({(dataLength == 0 ? 255 : dataLength)})";
                    }
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

        public string CreateMassiveOperationStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new()
        {
            throw new NotImplementedException();
        }

        public bool IsNewColumn(ColumnDefinition columnDefinition)
        {
            return columnDefinition == null;
        }

        public bool IsColumnDataTypeChanged(ColumnDefinition columnDefinition, string sqlDataType)
        {
            string columnMax = columnDefinition.Character_Maximum_Length != null ? $"({columnDefinition.Character_Maximum_Length})" : string.Empty;
            return columnDefinition.Data_Type == null ? false : $"{columnDefinition.Data_Type}{columnMax}" != sqlDataType;
        }

        public bool IsColumnRemoved(Dictionary<string, PropertyInfo> properties, string columnName)
        {
            return !properties.ContainsKey(columnName);
        }

        public bool IsNowNullable(ColumnDefinition columnDefinition, PropertyInfo property)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) != null && (columnDefinition.Is_Nullable == "NO" || columnDefinition.Is_Nullable == null);
        }

        public bool IsNowUnique(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, PropertyInfo property)
        {
            return !constraints.ContainsKey(uniqueConstraintName) && property.GetCustomAttribute<Unique>() != null;
        }

        public bool IsNowDefault(ColumnDefinition columnDefinition, PropertyInfo property)
        {
            return string.IsNullOrWhiteSpace(columnDefinition.Column_Default) && property.GetCustomAttribute<Default>() != null;
        }

        public bool IsNowPrimaryKey(Dictionary<string, ConstraintDefinition> constraints, string primaryKeyConstraintName, PropertyInfo property)
        {
            return !constraints.ContainsKey(primaryKeyConstraintName) && property.GetCustomAttribute<PrimaryKey>() != null;
        }

        public bool IsNowForeignKey(Dictionary<string, ConstraintDefinition> constraints, string foreignKeyConstraintName, PropertyInfo property)
        {
            return !constraints.ContainsKey(foreignKeyConstraintName) && property.GetCustomAttribute<ForeignKey>() != null;
        }

        public bool IsNoLongerNullable(ColumnDefinition columnDefinition, PropertyInfo property)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) == null && (columnDefinition.Is_Nullable == "YES" || columnDefinition.Is_Nullable == null);
        }

        public bool IsNoLongerUnique(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, PropertyInfo property)
        {
            return constraints.ContainsKey(uniqueConstraintName) && property.GetCustomAttribute<Unique>() == null;
        }

        public bool IsNoLongerDefault(ColumnDefinition columnDefinition, PropertyInfo property)
        {
            return !string.IsNullOrWhiteSpace(columnDefinition.Column_Default) && property.GetCustomAttribute<Default>() == null;
        }

        public bool IsNoLongerPrimaryKey(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, PropertyInfo property)
        {
            return constraints.ContainsKey(uniqueConstraintName) && property.GetCustomAttribute<PrimaryKey>() == null;
        }

        public bool IsNoLongerForeignKey(Dictionary<string, ConstraintDefinition> constraints, string foreignKeyConstraintName, PropertyInfo property)
        {
            return constraints.ContainsKey(foreignKeyConstraintName) && property.GetCustomAttribute<ForeignKey>() == null;
        }

        public bool IsDefaultChanged(ColumnDefinition columnDefinition, PropertyInfo property)
        {
            Default defaultValueAttribute = property.GetCustomAttribute<Default>();
            string currentDefaultValue = columnDefinition.Column_Default?.Replace("(", string.Empty).Replace(")", string.Empty).Replace("'", string.Empty);

            return !string.IsNullOrWhiteSpace(columnDefinition.Column_Default) ? defaultValueAttribute != null ? !currentDefaultValue.Equals($"{defaultValueAttribute.Value}") : false : false;
        }

        public bool IsNullable(PropertyInfo property)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) != null;
        }

        public bool IsUnique(IManageable model, string propertyName)
        {
            return model.Configuration.UniqueKeyProperties.ContainsKey(propertyName);
        }

        public bool IsDefault(IManageable model, string propertyName)
        {
            return model.Configuration.DefaultProperties.ContainsKey(propertyName);
        }

        public bool IsPrimaryKey(IManageable model, string propertyName)
        {
            return model.Configuration.PrimaryKeyProperty.Name.Equals(propertyName);
        }
    }
}
