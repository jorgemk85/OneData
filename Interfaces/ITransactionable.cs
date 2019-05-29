using OneData.Models;
using System;
using System.Collections.Generic;

namespace OneData.Interfaces
{
    internal interface ITransactionable
    {
        string AddTable(FullyQualifiedTableName tableName, string primaryKeyName, string primaryKeySqlDataType, bool isAutoIncrement);
        string RemoveTable(FullyQualifiedTableName tableName);

        string AddColumn(FullyQualifiedTableName tableName, string columnName, string sqlDataType);
        string ChangeColumnDataType(FullyQualifiedTableName tableName, string columnName, string sqlDataType);
        string RemoveColumn(FullyQualifiedTableName tableName, string columnName);

        string AddNotNullToColumn(FullyQualifiedTableName tableName, string columnName, string sqlDataType);
        string AddUniqueToColumn(FullyQualifiedTableName tableName, string columnName);
        string AddPrimaryKeyToColumn(FullyQualifiedTableName tableName, string columnName);
        string AddForeignKeyToColumn(FullyQualifiedTableName tableName, OneProperty property);
        string AddDefaultToColumn(FullyQualifiedTableName tableName, string columnName, object defaultValue);

        string RemoveNotNullFromColumn(FullyQualifiedTableName tableName, string columnName, string sqlDataType);
        string RemoveUniqueFromColumn(FullyQualifiedTableName tableName, string uniqueConstraintName);
        string RemovePrimaryKeyFromColumn(FullyQualifiedTableName tableName, string primaryKeyName);
        string RemoveForeignKeyFromColumn(FullyQualifiedTableName tableName, string foreignKeyName);
        string RemoveDefaultFromColumn(FullyQualifiedTableName tableName, string defaultConstraintName);

        string RenewDefaultInColumn(FullyQualifiedTableName tableName, string columnName, object defaultValue);
        string UpdateColumnValueToDefaultWhereNull(FullyQualifiedTableName tableName, string columnName, Type columnType);
        string ChangeForeignKeyRules(FullyQualifiedTableName tableName, OneProperty property);

        string AlterColumnWithConstraintValidation(string alterQuery, FullyQualifiedTableName tableName, Dictionary<string, ConstraintDefinition> constraints, ColumnDefinition columnDefinition, string propertyName, string sqlDataType);
        string AddNotNullToColumnWithUpdateData(FullyQualifiedTableName tableName, string propertyName, string sqlDataType, Type propertyType);
    }
}
