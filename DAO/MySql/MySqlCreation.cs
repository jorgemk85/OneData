using OneData.Attributes;
using OneData.Enums;
using OneData.Interfaces;
using OneData.Models;
using OneData.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneData.DAO.MySql
{
    internal class MySqlCreation : ICreatable
    {
        public void SetStoredProceduresParameters(IManageable model, StringBuilder queryBuilder, bool setDefaultNull, bool considerPrimary)
        {
            // Aqui se colocan los parametros segun las propiedades del objeto 
            foreach (KeyValuePair<string, OneProperty> property in model.GetComposition().FilteredProperties)
            {
                // Si la propiedad actual es la primaria y es auto increment y no se debe considerar para estos parametros, entonces se salta a la sig propiedad.
                // Esto significa que la propiedad primaria es Identity o Auto Increment y no se debe de mandar como parametro en un Insert.
                if (property.Value.Equals(model.GetComposition().PrimaryKeyProperty) && model.GetComposition().PrimaryKeyAttribute.IsAutoIncrement && !considerPrimary)
                {
                    continue;
                }

                if (setDefaultNull)
                {
                    queryBuilder.AppendFormat("    IN `_{0}` {1} = null,\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType, model.GetComposition().UniqueKeyProperties.ContainsKey(property.Value.Name), GetDataLengthFromProperty(model, property.Key)));
                }
                else
                {
                    queryBuilder.AppendFormat("    IN `_{0}` {1},\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType, model.GetComposition().UniqueKeyProperties.ContainsKey(property.Value.Name), GetDataLengthFromProperty(model, property.Key)));
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
                queryBuilder.AppendFormat("DROP PROCEDURE `{0}{1}{2}`;\n", Manager.StoredProcedurePrefix, model.GetComposition().TableName, Manager.InsertSuffix);
            }

            queryBuilder.AppendFormat("CREATE PROCEDURE `{0}{1}{2}` (\n", Manager.StoredProcedurePrefix, model.GetComposition().TableName, Manager.InsertSuffix);

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(model, queryBuilder, false, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(")\nBEGIN\n");
            queryBuilder.Append("SET @@sql_mode:=TRADITIONAL;\n");
            queryBuilder.AppendFormat("INSERT INTO `{0}{1}` (\n", Manager.TablePrefix, model.GetComposition().TableName);

            // Seccion para especificar a que columnas se va a insertar y sus valores.
            foreach (KeyValuePair<string, OneProperty> property in model.GetComposition().ManagedProperties)
            {
                if (property.Value.Equals(model.GetComposition().PrimaryKeyProperty) && model.GetComposition().PrimaryKeyAttribute.IsAutoIncrement)
                {
                    continue;
                }
                else
                {
                    insertsBuilder.AppendFormat("    `{0}`,\n", property.Value.Name);
                    if (model.GetComposition().AutoProperties.TryGetValue(property.Value.Name, out OneProperty autoProperty))
                    {
                        valuesBuilder.AppendFormat("    {0},\n", GetAutoPropertyValue(model.GetComposition().AutoPropertyAttributes[property.Value.Name].AutoPropertyType));
                    }
                    else
                    {
                        valuesBuilder.AppendFormat("    `_{0}`,\n", property.Value.Name);
                    }
                }
            }
            insertsBuilder.Remove(insertsBuilder.Length - 2, 2);
            queryBuilder.Append(insertsBuilder);
            queryBuilder.Append(")\nVALUES (\n");
            valuesBuilder.Remove(valuesBuilder.Length - 2, 2);
            queryBuilder.Append(valuesBuilder);
            queryBuilder.Append(");\nEND");

            Logger.Info("(MySql) Created a new query for Insert Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateUpdateStoredProcedure(IManageable model, bool doAlter)
        {
            StringBuilder queryBuilder = new StringBuilder();

            if (model.GetComposition().ManagedProperties.Count == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("DROP PROCEDURE `{0}{1}{2}`;\n", Manager.StoredProcedurePrefix, model.GetComposition().TableName, Manager.UpdateSuffix);
            }

            queryBuilder.AppendFormat("CREATE PROCEDURE `{0}{1}{2}` (\n", Manager.StoredProcedurePrefix, model.GetComposition().TableName, Manager.UpdateSuffix);

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters(model, queryBuilder, false, true);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(")\nBEGIN\n");
            queryBuilder.Append("SET @@sql_mode:=TRADITIONAL;\n");
            queryBuilder.AppendFormat("UPDATE `{0}{1}`\n", Manager.TablePrefix, model.GetComposition().TableName);
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
                    queryBuilder.AppendFormat("    `{0}` = {1},\n", property.Value.Name, GetAutoPropertyValue(model.GetComposition().AutoPropertyAttributes[property.Value.Name].AutoPropertyType));
                }
                else
                {
                    queryBuilder.AppendFormat("    `{0}` = IFNULL(`_{0}`, `{0}`),\n", property.Value.Name);
                }
            }
            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append($"WHERE `{model.GetComposition().PrimaryKeyProperty.Name}` = `_{model.GetComposition().PrimaryKeyProperty.Name}`;\n");
            queryBuilder.Append("END");

            Logger.Info("(MySql) Created a new query for Update Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateDeleteStoredProcedure(IManageable model, bool doAlter)
        {
            StringBuilder queryBuilder = new StringBuilder();

            if (doAlter)
            {
                queryBuilder.AppendFormat("DROP PROCEDURE `{0}{1}{2}`;\n", Manager.StoredProcedurePrefix, model.GetComposition().TableName, Manager.DeleteSuffix);
            }

            queryBuilder.AppendFormat("CREATE PROCEDURE `{0}{1}{2}` (\n", Manager.StoredProcedurePrefix, model.GetComposition().TableName, Manager.DeleteSuffix);

            queryBuilder.Append($"    IN `_{model.GetComposition().PrimaryKeyProperty.Name}` {GetSqlDataType(model.GetComposition().PrimaryKeyProperty.PropertyType, model.GetComposition().UniqueKeyProperties.ContainsKey(model.GetComposition().PrimaryKeyProperty.Name), 0)})\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("DELETE FROM `{0}{1}`\n", Manager.TablePrefix, model.GetComposition().TableName);
            queryBuilder.Append($"WHERE `{model.GetComposition().PrimaryKeyProperty.Name}` = `_{model.GetComposition().PrimaryKeyProperty.Name}`;\n");
            queryBuilder.Append("END");

            Logger.Info("(MySql) Created a new query for Delete Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateQueryForTableAlteration(IManageable model, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, ConstraintDefinition> constraints, FullyQualifiedTableName tableName)
        {
            if (model.GetComposition().ManagedProperties.Count == 0) return string.Empty;

            StringBuilder queryBuilder = new StringBuilder();
            ITransactionable transaction = new MySqlTransaction();
            IValidatable validation = new MySqlValidation();

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
                Logger.Info("(MySql) Created a new query for Alter Table:");
                Logger.Info(queryBuilder.ToString());
            }
            return queryBuilder.ToString();
        }

        public string GetCreateForeignKeysQuery(IManageable model, FullyQualifiedTableName tableName, Dictionary<string, ConstraintDefinition> constraints = null)
        {
            StringBuilder queryBuilder = new StringBuilder();
            ITransactionable transaction = new MySqlTransaction();

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
                Logger.Info("(MySql) Created a new query for Create Foreign Keys:");
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
                return "int(10)";
            }

            switch (underlyingType.Name.ToLower())
            {
                case "boolean":
                    return "tinyint(1)";
                case "bool":
                    return "tinyint(1)";
                case "guid":
                    return "char(36)";
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
                    return "blob";
            }
        }

        public static string GetAutoPropertyValue(AutoPropertyTypes type)
        {
            switch (type)
            {
                case AutoPropertyTypes.Date:
                    return "CURDATE()";
                case AutoPropertyTypes.DateTime:
                    return "NOW()";
                default:
                    return "NOW()";
            }
        }

        public string CreateMassiveOperationStoredProcedure(IManageable model, bool doAlter)
        {
            StringBuilder queryBuilder = new StringBuilder();

            if (model.GetComposition().ManagedProperties.Count == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.Append($"DROP PROCEDURE `{Manager.StoredProcedurePrefix}massive_operation`;\n");
            }

            queryBuilder.Append($"CREATE PROCEDURE `{Manager.StoredProcedurePrefix}massive_operation` (\n");

            // Aqui se colocan los parametros fijos
            queryBuilder.Append("    IN `_xmlValues` LONGTEXT,\n");
            queryBuilder.Append("    IN `_xmlNames` TEXT,\n");
            queryBuilder.Append("    IN `_procedureName` VARCHAR(255))\n");

            queryBuilder.Append("BEGIN\n");
            queryBuilder.Append("  DECLARE indexValue INT UNSIGNED DEFAULT 1;\n");
            queryBuilder.Append("  DECLARE indexName INT UNSIGNED DEFAULT 1;\n");
            queryBuilder.Append("  DECLARE countValues INT UNSIGNED DEFAULT ExtractValue(_xmlValues, 'count(//object)');\n");
            queryBuilder.Append("  DECLARE countNames INT UNSIGNED DEFAULT ExtractValue(_xmlNames, 'count(//column)');\n");
            queryBuilder.Append("  DECLARE currentName varchar(255);\n");
            queryBuilder.Append("  DECLARE currentValue TEXT;\n");
            queryBuilder.Append("  DECLARE currentObject TEXT DEFAULT '';\n");
            queryBuilder.Append("  DECLARE node varchar(255);\n");
            queryBuilder.Append("  SET @@sql_mode:= TRADITIONAL;\n");
            queryBuilder.Append("  WHILE indexValue <= countValues DO\n");
            queryBuilder.Append("    SET indexName = 1;\n");
            queryBuilder.Append("    SET currentObject = '';\n");
            queryBuilder.Append("    WHILE indexName <= countNames  DO\n");
            queryBuilder.Append("        SET currentName = ExtractValue(_xmlNames,  '//column[$indexName]/name');\n");
            queryBuilder.Append("        SET node = concat('//object[$indexValue]/', currentName);\n");
            queryBuilder.Append("        SET currentValue = ExtractValue(_xmlValues, node);\n");
            queryBuilder.Append("        SET currentObject = concat(currentObject, currentValue, ',');\n");
            queryBuilder.Append("        SET indexName = indexName + 1;\n");
            queryBuilder.Append("    END WHILE;\n");
            queryBuilder.Append("    SET currentObject = LEFT(currentObject, length(currentObject) -1);\n");
            queryBuilder.Append("    SET @currentCommand = concat(' CALL ', _procedureName , ' (',  currentObject,')');\n");
            queryBuilder.Append("    PREPARE stmt1 FROM @currentCommand;\n");
            queryBuilder.Append("    EXECUTE stmt1;\n");
            queryBuilder.Append("    SET indexValue = indexValue + 1;\n");
            queryBuilder.Append("  END WHILE;\n");
            queryBuilder.Append("END");

            Logger.Info("(MySql) Created a new query for Massive Operacion Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateQueryForTableCreation(IManageable model, FullyQualifiedTableName tableName)
        {
            if (model.GetComposition().ManagedProperties.Count == 0) return string.Empty;

            StringBuilder queryBuilder = new StringBuilder();
            ITransactionable transaction = new MySqlTransaction();
            IValidatable valitation = new MySqlValidation();

            queryBuilder.Append(transaction.AddTable(tableName, model.GetComposition().PrimaryKeyProperty.Name, GetSqlDataType(model.GetComposition().PrimaryKeyProperty.PropertyType, false, 0), model.GetComposition().PrimaryKeyAttribute.IsAutoIncrement));

            // Aqui se colocan las propiedades del objeto. Una por columna por su puesto.
            foreach (KeyValuePair<string, OneProperty> property in model.GetComposition().ManagedProperties.Where(q => q.Key != model.GetComposition().PrimaryKeyProperty.Name))
            {
                string sqlDataType = GetSqlDataType(property.Value.PropertyType, model.GetComposition().UniqueKeyProperties.ContainsKey(property.Value.Name), GetDataLengthFromProperty(model, property.Key));

                queryBuilder.Append(transaction.AddColumn(tableName, property.Value.Name, sqlDataType));
                queryBuilder.Append(!valitation.IsNullable(property.Value) ? transaction.AddNotNullToColumn(tableName, property.Value.Name, sqlDataType) : string.Empty);
                queryBuilder.Append(valitation.IsUnique(model, property.Value.Name) ? transaction.AddUniqueToColumn(tableName, property.Value.Name) : string.Empty);
                queryBuilder.Append(valitation.IsDefault(model, property.Value.Name) ? transaction.AddDefaultToColumn(tableName, property.Value.Name, model.GetComposition().DefaultAttributes[property.Value.Name].Value) : string.Empty);
            }
            if (!string.IsNullOrWhiteSpace(queryBuilder.ToString()))
            {
                Logger.Info("(MySql) Created a new query for Create Table:");
                Logger.Info(queryBuilder.ToString());
            }
            return queryBuilder.ToString();
        }


    }
}
