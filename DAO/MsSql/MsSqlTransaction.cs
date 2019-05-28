using OneData.Attributes;
using OneData.Interfaces;
using OneData.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OneData.DAO.MsSql
{
    internal class MsSqlTransaction : ITransactionable
    {
        public string AddColumn(FullyQualifiedTableName tableName, string columnName, string sqlDataType)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] ADD [{columnName}] {sqlDataType}; \n";
        }

        public string AddDefaultToColumn(FullyQualifiedTableName tableName, string columnName, object defaultValue)
        {
            object value = defaultValue is string ? $"'{defaultValue}'" : defaultValue;
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] ADD CONSTRAINT DF_{tableName.Schema}_{tableName.Table}_{columnName} DEFAULT {value} FOR [{columnName}]; \n";
        }

        public string AddForeignKeyToColumn(FullyQualifiedTableName tableName, OneProperty property)
        {
            IManageable foreignModel = (IManageable)Activator.CreateInstance(property.ForeignKeyAttribute.Model);
            FullyQualifiedTableName foreignTableName = new FullyQualifiedTableName(foreignModel.Composition.Schema, $"{Manager.TablePrefix}{foreignModel.Composition.TableName}");

            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] ADD CONSTRAINT FK_{tableName.Schema}_{tableName.Table}_{property.Name} FOREIGN KEY([{property.Name}]) REFERENCES [{foreignTableName.Schema}].[{foreignTableName.Table}]([{foreignModel.Composition.PrimaryKeyProperty.Name}]) ON DELETE {property.ForeignKeyAttribute.OnDelete.ToString().Replace("_", " ")} ON UPDATE {property.ForeignKeyAttribute.OnUpdate.ToString().Replace("_", " ")}; \n";
        }

        public string AddNotNullToColumn(FullyQualifiedTableName tableName, string columnName, string sqlDataType)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] ALTER COLUMN [{columnName}] {sqlDataType} NOT NULL; \n";
        }

        public string AddNotNullToColumnWithUpdateData(FullyQualifiedTableName tableName, string propertyName, string sqlDataType, Type propertyType)
        {
            return $"{UpdateColumnValueToDefaultWhereNull(tableName, propertyName, propertyType)}|;|{AddNotNullToColumn(tableName, propertyName, sqlDataType)}";
        }

        public string AlterColumnWithConstraintValidation(string alterQuery, FullyQualifiedTableName tableName, Dictionary<string, ConstraintDefinition> constraints, ColumnDefinition columnDefinition, string propertyName, string sqlDataType)
        {
            MsSqlValidation validation = new MsSqlValidation();
            StringBuilder stringBuilder = new StringBuilder();
            bool isUnique = validation.IsUnique(constraints, $"UQ_{tableName.Schema}_{tableName.Table}_{propertyName}");
            bool isDefault = validation.IsDefault(columnDefinition);
            object currentDefaultValue = null;

            if (isDefault)
            {
                currentDefaultValue = columnDefinition.Column_Default.ToString().Replace("(", string.Empty).Replace(")", string.Empty).Replace("'", string.Empty);
                currentDefaultValue = columnDefinition.Column_Default.ToString().StartsWith("('") ? currentDefaultValue : Convert.ToDecimal(currentDefaultValue);
            }

            stringBuilder.Append(isUnique ? RemoveUniqueFromColumn(tableName, $"UQ_{tableName.Schema}_{tableName.Table}_{propertyName}") : string.Empty);
            stringBuilder.Append(isDefault ? RemoveDefaultFromColumn(tableName, $"DF_{tableName.Schema}_{tableName.Table}_{propertyName}") : string.Empty);
            stringBuilder.Append(alterQuery);
            stringBuilder.Append(isUnique ? AddUniqueToColumn(tableName, propertyName) : string.Empty);
            stringBuilder.Append(isDefault ? AddDefaultToColumn(tableName, propertyName, currentDefaultValue) : string.Empty);

            return stringBuilder.ToString();
        }

        public string AddPrimaryKeyToColumn(FullyQualifiedTableName tableName, string columnName)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] ADD CONSTRAINT PK_{tableName.Schema}_{tableName.Table}_{columnName} PRIMARY KEY({columnName}); \n";
        }

        public string AddTable(FullyQualifiedTableName tableName, string primaryKeyName, string primaryKeySqlDataType, bool isAutoIncrement)
        {
            string processedDataType = isAutoIncrement ? "INT IDENTITY(1,1)" : $"{primaryKeySqlDataType}";
            return $"CREATE TABLE [{tableName.Schema}].[{tableName.Table}] ({primaryKeyName} {processedDataType}); \n{AddNotNullToColumn(tableName, primaryKeyName, primaryKeySqlDataType)}{AddPrimaryKeyToColumn(tableName, primaryKeyName)}";
        }

        public string AddUniqueToColumn(FullyQualifiedTableName tableName, string columnName)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] ADD CONSTRAINT UQ_{tableName.Schema}_{tableName.Table}_{columnName} UNIQUE({columnName}); \n";
        }

        public string ChangeColumnDataType(FullyQualifiedTableName tableName, string columnName, string sqlDataType)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] ALTER COLUMN [{columnName}] {sqlDataType}; \n";
        }

        public string RemoveColumn(FullyQualifiedTableName tableName, string columnName)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] DROP COLUMN [{columnName}]; \n";
        }

        public string RemoveDefaultFromColumn(FullyQualifiedTableName tableName, string defaultConstraintName)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] DROP CONSTRAINT [{defaultConstraintName}]; \n";
        }

        public string RemoveForeignKeyFromColumn(FullyQualifiedTableName tableName, string foreignKeyName)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] DROP CONSTRAINT {foreignKeyName}; \n";
        }

        public string RemoveNotNullFromColumn(FullyQualifiedTableName tableName, string columnName, string sqlDataType)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] ALTER COLUMN [{columnName}] {sqlDataType}; \n";
        }

        public string RemovePrimaryKeyFromColumn(FullyQualifiedTableName tableName, string primaryKeyName)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] DROP CONSTRAINT {primaryKeyName}; \n";
        }

        public string RemoveTable(FullyQualifiedTableName tableName)
        {
            return $"DROP TABLE [{tableName.Schema}].[{tableName.Table}]; \n";
        }

        public string RemoveUniqueFromColumn(FullyQualifiedTableName tableName, string uniqueKeyName)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] DROP CONSTRAINT {uniqueKeyName}; \n";
        }

        public string RenewDefaultInColumn(FullyQualifiedTableName tableName, string columnName, object defaultValue)
        {
            return $"{RemoveDefaultFromColumn(tableName, $"DF_{tableName.Schema}_{tableName.Table}_{columnName}")}{AddDefaultToColumn(tableName, columnName, defaultValue)}";
        }

        public string ChangeForeignKeyRules(FullyQualifiedTableName tableName, OneProperty property)
        { 
            return $"{RemoveForeignKeyFromColumn(tableName, $"FK_{tableName.Schema}_{tableName.Table}_{property.Name}")}|;|{AddForeignKeyToColumn(tableName, property)}";
        }

        public string UpdateColumnValueToDefaultWhereNull(FullyQualifiedTableName tableName, string columnName, Type columnType)
        {
            return $"UPDATE [{tableName.Schema}].[{tableName.Table}] SET [{columnName}] = {GetDefault(columnType)} WHERE [{columnName}] IS NULL; \n";
        }

        private object GetDefault(Type type)
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
    }
}
