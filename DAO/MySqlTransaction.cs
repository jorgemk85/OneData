using OneData.Attributes;
using OneData.Interfaces;
using OneData.Models;
using System;
using System.Reflection;

namespace OneData.DAO
{
    internal class MySqlTransaction : ITransactionable
    {
        public string AddColumn(FullyQualifiedTableName tableName, string columnName, string sqlDataType)
        {
            return $"ALTER TABLE `{tableName.Schema}`.`{tableName.Table}` ADD `{columnName}` {sqlDataType}; \n";
        }

        public string AddDefaultToColumn(FullyQualifiedTableName tableName, string columnName, string defaultValue)
        {
            return $"ALTER TABLE `{tableName.Schema}`.`{tableName.Table}` ALTER `{columnName}` SET DEFAULT '{defaultValue}'; \n";
        }

        public string AddForeignKeyToColumn(FullyQualifiedTableName tableName, PropertyInfo property)
        {
            ForeignKey foreignAttribute = property.GetCustomAttribute<ForeignKey>();
            IManageable foreignModel = (IManageable)Activator.CreateInstance(foreignAttribute.Model);
            FullyQualifiedTableName foreignTableName = new FullyQualifiedTableName(foreignModel.Configuration.Schema, $"{Manager.TablePrefix}{foreignModel.Configuration.TableName}");
            // TODO: Falta agregar ON DELETE y ON UPDATE !!!
            return $"ALTER TABLE `{tableName.Schema}`.`{tableName.Table}` ADD CONSTRAINT FK_{tableName.Schema}_{tableName.Table}_{property.Name} FOREIGN KEY(`{property.Name}`) REFERENCES `{foreignTableName.Schema}`.`{foreignTableName.Table}`(`{foreignModel.Configuration.PrimaryKeyProperty.Name}`); \n";
        }

        public string AddNotNullToColumn(FullyQualifiedTableName tableName, string columnName, string sqlDataType)
        {
            return $"ALTER TABLE `{tableName.Schema}`.`{tableName.Table}` MODIFY `{columnName}` {sqlDataType} NOT NULL; \n";
        }

        public string AddPrimaryKeyToColumn(FullyQualifiedTableName tableName, string columnName)
        {
            return $"ALTER TABLE `{tableName.Schema}`.`{tableName.Table}` ADD CONSTRAINT PK_{tableName.Schema}_{tableName.Table}_{columnName} PRIMARY KEY({columnName}); \n";
        }

        public string AddTable(FullyQualifiedTableName tableName, string primaryKeyName, string primaryKeySqlDataType, bool isAutoIncrement)
        {
            return $"CREATE TABLE `{tableName.Schema}`.`{tableName.Table}` ({primaryKeyName} {primaryKeySqlDataType} NOT NULL {(isAutoIncrement ? "AUTO_INCREMENT" : "")}, PRIMARY KEY ({primaryKeyName})) ENGINE=InnoDB; \n";
        }

        public string AddUniqueToColumn(FullyQualifiedTableName tableName, string columnName)
        {
            return $"ALTER TABLE `{tableName.Schema}`.`{tableName.Table}` ADD CONSTRAINT UQ_{tableName.Schema}_{tableName.Table}_{columnName} UNIQUE({columnName}); \n";
        }

        public string ChangeColumnDataType(FullyQualifiedTableName tableName, string columnName, string sqlDataType)
        {
            return $"ALTER TABLE `{tableName.Schema}`.`{tableName.Table}` MODIFY COLUMN `{columnName}` {sqlDataType}; \n";
        }

        public string RemoveColumn(FullyQualifiedTableName tableName, string columnName)
        {
            return $"ALTER TABLE `{tableName.Schema}`.`{tableName.Table}` DROP COLUMN `{columnName}`; \n";
        }

        public string RemoveDefaultFromColumn(FullyQualifiedTableName tableName, string defaultConstraintName)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] DROP CONSTRAINT [{defaultConstraintName}]; \n";
        }

        public string RemoveForeignKeyFromColumn(FullyQualifiedTableName tableName, string foreignKeyName)
        {
            return $"ALTER TABLE `{tableName.Schema}`.`{tableName.Table}` DROP FOREIGN KEY {foreignKeyName}; \n";
        }

        public string RemoveNotNullFromColumn(FullyQualifiedTableName tableName, string columnName, string sqlDataType)
        {
            return $"ALTER TABLE `{tableName.Schema}`.`{tableName.Table}` MODIFY `{columnName}` {sqlDataType}; \n";
        }

        public string RemovePrimaryKeyFromColumn(FullyQualifiedTableName tableName, string primaryKeyName)
        {
            return $"ALTER TABLE `{tableName.Schema}`.`{tableName.Table}` DROP PRIMARY KEY; \n";
        }

        public string RemoveTable(FullyQualifiedTableName tableName)
        {
            return $"DROP TABLE `{tableName.Schema}`.`{tableName.Table}`; \n";
        }

        public string RemoveUniqueFromColumn(FullyQualifiedTableName tableName, string uniqueKeyName)
        {
            return $"ALTER TABLE `{tableName.Schema}`.`{tableName.Table}` DROP INDEX {uniqueKeyName}; \n";
        }

        public string RenewDefaultInColumn(FullyQualifiedTableName tableName, string columnName, string defaultValue)
        {
            return $"{RemoveDefaultFromColumn(tableName, $"DF_{tableName.Schema}_{tableName.Table}_{columnName}")}{AddDefaultToColumn(tableName, columnName, defaultValue)}";
        }
    }
}
