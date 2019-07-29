using OneData.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneData.Interfaces
{
    internal interface ICreatable
    {
        void SetStoredProceduresParameters(IManageable model, StringBuilder queryBuilder, bool setDefaultNull, bool considerId);
        string CreateInsertStoredProcedure(IManageable model, bool doAlter);
        string CreateMassiveOperationStoredProcedure(IManageable model, bool doAlter);
        string CreateUpdateStoredProcedure(IManageable model, bool doAlter);
        string CreateDeleteStoredProcedure(IManageable model, bool doAlter);
        string CreateQueryForTableCreation(IManageable model, FullyQualifiedTableName tableName);
        string CreateQueryForTableAlteration(IManageable model, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, ConstraintDefinition> constraints, FullyQualifiedTableName tableName);
        string GetCreateForeignKeysQuery(IManageable model, FullyQualifiedTableName tableName, Dictionary<string, ConstraintDefinition> constraints);
        string GetSqlDataType(Type codeType, bool isUniqueKey, long dataLength);
    }
}
