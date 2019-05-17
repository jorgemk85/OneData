﻿using OneData.Attributes;
using OneData.Interfaces;
using OneData.Models;
using System;
using System.Reflection;

namespace OneData.DAO
{
    internal class MsSqlTransaction : ITransactionable
    {
        public string AddColumn(FullyQualifiedTableName tableName, string columnName, string sqlDataType)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] ADD [{columnName}] {sqlDataType}; \n";
        }

        public string AddDefaultToColumn(FullyQualifiedTableName tableName, string columnName, string defaultValue)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] ADD CONSTRAINT DF_{tableName.Schema}_{tableName.Table}_{columnName} DEFAULT '{defaultValue}' FOR [{columnName}]; \n";
        }

        public string AddForeignKeyToColumn(FullyQualifiedTableName tableName, PropertyInfo property)
        {
            ForeignKey foreignAttribute = property.GetCustomAttribute<ForeignKey>();
            IManageable foreignModel = (IManageable)Activator.CreateInstance(foreignAttribute.Model);
            FullyQualifiedTableName foreignTableName = new FullyQualifiedTableName(foreignModel.Configuration.Schema, $"{Manager.TablePrefix}{foreignModel.Configuration.TableName}");

            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] ADD CONSTRAINT FK_{tableName.Schema}_{tableName.Table}_{property.Name} FOREIGN KEY([{property.Name}]) REFERENCES [{foreignTableName.Schema}].[{foreignTableName.Table}]([{foreignModel.Configuration.PrimaryKeyProperty.Name}]); \n";
        }

        public string AddNotNullToColumn(FullyQualifiedTableName tableName, string columnName, string sqlDataType)
        {
            return $"ALTER TABLE [{tableName.Schema}].[{tableName.Table}] ALTER COLUMN [{columnName}] {sqlDataType} NOT NULL; \n";
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

        public string RenewDefaultInColumn(FullyQualifiedTableName tableName, string columnName, string defaultValue)
        {
            return $"{RemoveDefaultFromColumn(tableName, $"DF_{tableName.Schema}_{tableName.Table}_{columnName}")}{AddDefaultToColumn(tableName, columnName, defaultValue)}";
        }
    }
}