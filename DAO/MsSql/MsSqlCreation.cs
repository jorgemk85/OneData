using OneData.Attributes;
using OneData.Enums;
using OneData.Interfaces;
using OneData.Models;
using OneData.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneData.DAO.MsSql
{
    internal class MsSqlCreation : ICreatable
    {
        public void SetStoredProceduresParameters(IManageable model, StringBuilder queryBuilder, bool setDefaultNull, bool considerPrimary)
        {
            // Aqui se colocan los parametros segun las propiedades del objeto 
            foreach (KeyValuePair<string, OneProperty> property in model.GetComposition().ManagedProperties)
            {
                // Si la propiedad actual es la primaria y es auto increment y no se debe considerar para estos parametros, entonces se salta a la sig propiedad.
                // Esto significa que la propiedad primaria es Identity o Auto Increment y no se debe de mandar como parametro en un Insert.
                if (property.Value.Equals(model.GetComposition().PrimaryKeyProperty) && model.GetComposition().PrimaryKeyAttribute.IsAutoIncrement && !considerPrimary)
                {
                    continue;
                }
                // Si la propiedad es DateCreated o DateModified o AutoProperty, no se debe mandar como parametro
                // Esto es por que estos valores se alimentan desde el procedimiento almacenado.
                if (model.GetComposition().AutoProperties.ContainsKey(property.Value.Name))
                {
                    continue;
                }
                if (setDefaultNull)
                {
                    queryBuilder.AppendFormat("    @_{0} {1} = null,\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType, model.GetComposition().UniqueKeyProperties.ContainsKey(property.Value.Name), GetDataLengthFromProperty(model, property.Key)));
                }
                else
                {
                    queryBuilder.AppendFormat("    @_{0} {1},\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType, model.GetComposition().UniqueKeyProperties.ContainsKey(property.Value.Name), GetDataLengthFromProperty(model, property.Key)));
                }
            }
        }

        private long GetDataLengthFromProperty(IManageable model, string propertyName)
        {
            model.GetComposition().DataLengthAttributes.TryGetValue(propertyName, out DataLength dataLengthAttribute);

            if (dataLengthAttribute != null)
            {
                return dataLengthAttribute.Length;
            }
            else
            {
                return 0;
            }
        }

        public string CreateInsertStoredProcedure(IManageable model, bool doAlter)
        {
            StringBuilder queryBuilder = new StringBuilder();
            StringBuilder insertsBuilder = new StringBuilder();
            StringBuilder valuesBuilder = new StringBuilder();

            if (model.GetComposition().ManagedProperties.Count == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", model.GetComposition().Schema, Manager.StoredProcedurePrefix, model.GetComposition().TableName, Manager.InsertSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", model.GetComposition().Schema, Manager.StoredProcedurePrefix, model.GetComposition().TableName, Manager.InsertSuffix);
            }

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(model, queryBuilder, false, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append("\nAS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("INSERT INTO {0}.{1}{2} (\n", model.GetComposition().Schema, Manager.TablePrefix, model.GetComposition().TableName);

            // Seccion para especificar a que columnas se va a insertar y sus valores.
            foreach (KeyValuePair<string, OneProperty> property in model.GetComposition().ManagedProperties)
            {
                if (property.Value.Equals(model.GetComposition().PrimaryKeyProperty) && model.GetComposition().PrimaryKeyAttribute.IsAutoIncrement)
                {
                    continue;
                }
                else
                {
                    insertsBuilder.AppendFormat("    [{0}],\n", property.Value.Name);
                    if (model.GetComposition().AutoProperties.TryGetValue(property.Value.Name, out OneProperty autoProperty))
                    {
                        valuesBuilder.AppendFormat("    {0},\n", GetAutoPropertyValue(model.GetComposition().AutoPropertyAttributes[property.Value.Name].AutoPropertyType));
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

        public string CreateUpdateStoredProcedure(IManageable model, bool doAlter)
        {
            StringBuilder queryBuilder = new StringBuilder();

            if (model.GetComposition().ManagedProperties.Count == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", model.GetComposition().Schema, Manager.StoredProcedurePrefix, model.GetComposition().TableName, Manager.UpdateSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", model.GetComposition().Schema, Manager.StoredProcedurePrefix, model.GetComposition().TableName, Manager.UpdateSuffix);
            }


            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(model, queryBuilder, false, true);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append("\nAS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("UPDATE {0}.{1}{2}\n", model.GetComposition().Schema, Manager.TablePrefix, model.GetComposition().TableName);
            queryBuilder.Append("SET\n");

            // Se especifica el parametro que va en x columna.
            foreach (KeyValuePair<string, OneProperty> property in model.GetComposition().ManagedProperties)
            {
                if (property.Value.Equals(model.GetComposition().PrimaryKeyProperty) || property.Value.Name.Equals(model.GetComposition().DateCreatedProperty.Name))
                {
                    continue;
                }
                if (model.GetComposition().AutoProperties.TryGetValue(property.Value.Name, out OneProperty autoProperty))
                {
                    queryBuilder.AppendFormat("    [{0}] = {1},\n", property.Value.Name, GetAutoPropertyValue(model.GetComposition().AutoPropertyAttributes[property.Value.Name].AutoPropertyType));
                }
                else
                {
                    queryBuilder.AppendFormat("    [{0}] = ISNULL(@_{0}, [{0}]),\n", property.Value.Name);
                }
            }
            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.AppendFormat($"WHERE {model.GetComposition().PrimaryKeyProperty.Name} = @_{model.GetComposition().PrimaryKeyProperty.Name};\n");
            queryBuilder.Append("END");

            Logger.Info("Created a new query for Update Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateDeleteStoredProcedure(IManageable model, bool doAlter)
        {
            StringBuilder queryBuilder = new StringBuilder();

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", model.GetComposition().Schema, Manager.StoredProcedurePrefix, model.GetComposition().TableName, Manager.DeleteSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", model.GetComposition().Schema, Manager.StoredProcedurePrefix, model.GetComposition().TableName, Manager.DeleteSuffix);
            }

            queryBuilder.Append($"@_{model.GetComposition().PrimaryKeyProperty.Name} {GetSqlDataType(model.GetComposition().PrimaryKeyProperty.PropertyType, model.GetComposition().UniqueKeyProperties.ContainsKey(model.GetComposition().PrimaryKeyProperty.Name), 0)}\n");
            queryBuilder.Append("AS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("DELETE FROM {0}.{1}{2}\n", model.GetComposition().Schema, Manager.TablePrefix, model.GetComposition().TableName);
            queryBuilder.AppendFormat($"WHERE [{model.GetComposition().PrimaryKeyProperty.Name}] = @_{model.GetComposition().PrimaryKeyProperty.Name};\n");
            queryBuilder.Append("END");

            Logger.Info("Created a new query for Delete Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateQueryForTableCreation(IManageable model, FullyQualifiedTableName tableName)
        {
            if (model.GetComposition().ManagedProperties.Count == 0) return string.Empty;

            StringBuilder queryBuilder = new StringBuilder();
            ITransactionable transaction = new MsSqlTransaction();
            IValidatable validation = new MsSqlValidation();

            queryBuilder.Append(transaction.AddTable(tableName, model.GetComposition().PrimaryKeyProperty.Name, GetSqlDataType(model.GetComposition().PrimaryKeyProperty.PropertyType, false, 0), model.GetComposition().PrimaryKeyAttribute.IsAutoIncrement));

            // Aqui se colocan las propiedades del objeto. Una por columna por su puesto (excepto para la primary key).
            foreach (KeyValuePair<string, OneProperty> property in model.GetComposition().ManagedProperties.Where(q => q.Key != model.GetComposition().PrimaryKeyProperty.Name))
            {
                string sqlDataType = GetSqlDataType(property.Value.PropertyType, model.GetComposition().UniqueKeyProperties.ContainsKey(property.Value.Name), GetDataLengthFromProperty(model, property.Key));

                queryBuilder.Append(transaction.AddColumn(tableName, property.Value.Name, sqlDataType));
                queryBuilder.Append(!validation.IsNullable(property.Value) ? transaction.AddNotNullToColumn(tableName, property.Value.Name, sqlDataType) : string.Empty);
                queryBuilder.Append(validation.IsUnique(model, property.Value.Name) ? transaction.AddUniqueToColumn(tableName, property.Value.Name) : string.Empty);
                queryBuilder.Append(validation.IsDefault(model, property.Value.Name) ? transaction.AddDefaultToColumn(tableName, property.Value.Name, model.GetComposition().DefaultAttributes[property.Value.Name].Value) : string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(queryBuilder.ToString()))
            {
                Logger.Info("Created a new query for Create Table:");
                Logger.Info(queryBuilder.ToString());
            }
            return queryBuilder.ToString();
        }

        public string CreateQueryForTableAlteration(IManageable model, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, ConstraintDefinition> constraints, FullyQualifiedTableName tableName)
        {
            if (model.GetComposition().ManagedProperties.Count == 0) return string.Empty;

            StringBuilder queryBuilder = new StringBuilder();
            ITransactionable transaction = new MsSqlTransaction();
            IValidatable validation = new MsSqlValidation();

            foreach (KeyValuePair<string, OneProperty> property in model.GetComposition().ManagedProperties)
            {
                columnDetails.TryGetValue(property.Value.Name, out ColumnDefinition columnDefinition);
                string sqlDataType = GetSqlDataType(property.Value.PropertyType, model.GetComposition().UniqueKeyProperties.ContainsKey(property.Value.Name), GetDataLengthFromProperty(model, property.Key));
                model.GetComposition().DefaultAttributes.TryGetValue(property.Key, out Default defaultAttribute);

                if (validation.IsNewColumn(columnDefinition))
                {
                    queryBuilder.Append($"{transaction.AddColumn(tableName, property.Value.Name, sqlDataType)}|;|");
                    columnDefinition = new ColumnDefinition();
                    queryBuilder.Append(validation.IsNowNullable(columnDefinition, property.Value) ? string.Empty : transaction.UpdateColumnValueToDefaultWhereNull(tableName, property.Value.Name, property.Value.PropertyType));
                }
                if (validation.IsColumnDataTypeChanged(columnDefinition, sqlDataType))
                {
                    queryBuilder.Append(transaction.AlterColumnWithConstraintValidation(transaction.ChangeColumnDataType(tableName, property.Value.Name, sqlDataType), tableName, constraints, columnDefinition, property.Value.Name, sqlDataType));
                }
                queryBuilder.Append(validation.IsForeignKeyRulesChanged(constraints, $"FK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}", property.Value.ForeignKeyAttribute) ? transaction.ChangeForeignKeyRules(tableName, property.Value) : string.Empty);

                queryBuilder.Append(validation.IsNowNullable(columnDefinition, property.Value) ? transaction.RemoveNotNullFromColumn(tableName, property.Value.Name, sqlDataType) : string.Empty);
                queryBuilder.Append(validation.IsNoLongerNullable(columnDefinition, property.Value) ? transaction.AlterColumnWithConstraintValidation(transaction.AddNotNullToColumnWithUpdateData(tableName, property.Value.Name, sqlDataType, property.Value.PropertyType), tableName, constraints, columnDefinition, property.Value.Name, sqlDataType) : string.Empty);

                queryBuilder.Append(validation.IsNowUnique(constraints, $"UQ_{tableName.Schema}_{tableName.Table}_{property.Value.Name}", property.Value) ? transaction.AddUniqueToColumn(tableName, property.Value.Name) : string.Empty);
                queryBuilder.Append(validation.IsNoLongerUnique(constraints, $"UQ_{tableName.Schema}_{tableName.Table}_{property.Value.Name}", property.Value) ? transaction.RemoveUniqueFromColumn(tableName, $"UQ_{tableName.Schema}_{tableName.Table}_{property.Value.Name}") : string.Empty);

                queryBuilder.Append(validation.IsNowDefault(columnDefinition, property.Value) ? transaction.AddDefaultToColumn(tableName, property.Value.Name, defaultAttribute.Value) : string.Empty);
                queryBuilder.Append(validation.IsDefaultChanged(columnDefinition, property.Value) ? transaction.RenewDefaultInColumn(tableName, property.Value.Name, defaultAttribute.Value) : string.Empty);
                queryBuilder.Append(validation.IsNoLongerDefault(columnDefinition, property.Value) ? transaction.RemoveDefaultFromColumn(tableName, $"DF_{tableName.Schema}_{tableName.Table}_{property.Value.Name}") : string.Empty);

                queryBuilder.Append(validation.IsNowPrimaryKey(constraints, $"PK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}", property.Value) ? transaction.AddPrimaryKeyToColumn(tableName, property.Value.Name) : string.Empty);
                queryBuilder.Append(validation.IsNoLongerPrimaryKey(constraints, $"PK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}", property.Value) ? transaction.RemovePrimaryKeyFromColumn(tableName, $"PK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}") : string.Empty);

                queryBuilder.Append(validation.IsNoLongerForeignKey(constraints, $"FK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}", property.Value) ? transaction.RemoveForeignKeyFromColumn(tableName, $"FK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}") : string.Empty);
            }

            foreach (KeyValuePair<string, ColumnDefinition> columnDefinition in columnDetails.Where(q => !model.GetComposition().ManagedProperties.Keys.Contains(q.Key)))
            {
                queryBuilder.Append(validation.IsNoLongerDefault(columnDefinition.Value, null) ? $"{transaction.RemoveDefaultFromColumn(tableName, $"DF_{tableName.Schema}_{tableName.Table}_{columnDefinition.Key}")}|;|" : string.Empty);
                queryBuilder.Append(validation.IsNoLongerUnique(constraints, $"UQ_{tableName.Schema}_{tableName.Table}_{columnDefinition.Key}", null) ? transaction.RemoveUniqueFromColumn(tableName, $"UQ_{tableName.Schema}_{tableName.Table}_{columnDefinition.Key}") : string.Empty);
                queryBuilder.Append(transaction.RemoveColumn(tableName, columnDefinition.Key));
            }

            if (!string.IsNullOrWhiteSpace(queryBuilder.ToString()))
            {
                Logger.Info("Created a new query for Alter Table:");
                Logger.Info(queryBuilder.ToString());

            }
            return queryBuilder.ToString();
        }

        public string GetCreateForeignKeysQuery(IManageable model, FullyQualifiedTableName tableName, Dictionary<string, ConstraintDefinition> constraints = null)
        {
            StringBuilder queryBuilder = new StringBuilder();
            ITransactionable transaction = new MsSqlTransaction();

            foreach (KeyValuePair<string, OneProperty> property in model.GetComposition().ForeignKeyProperties)
            {
                if (constraints == null)
                {
                    queryBuilder.Append(transaction.AddForeignKeyToColumn(tableName, property.Value));
                    continue;
                }
                if (!constraints.ContainsKey($"FK_{tableName.Schema}_{tableName.Table}_{property.Value.Name}"))
                {
                    queryBuilder.Append(transaction.AddForeignKeyToColumn(tableName, property.Value));
                }
            }

            if (!string.IsNullOrWhiteSpace(queryBuilder.ToString()))
            {
                Logger.Info("Created a new query for Create Foreign Keys:");
                Logger.Info(queryBuilder.ToString());
            }

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

        public string CreateMassiveOperationStoredProcedure(IManageable model, bool doAlter)
        {
            return string.Empty;
        }


    }
}
