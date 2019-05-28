using OneData.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneData.Interfaces
{
    internal interface ICreatable
    {
        void SetStoredProceduresParameters<T>(StringBuilder queryBuilder, bool setDefaultNull, bool considerId) where T : Cope<T>, IManageable, new();
        string CreateInsertStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new();
        string CreateMassiveOperationStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new();
        string CreateUpdateStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new();
        string CreateDeleteStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new();
        string CreateQueryForTableCreation(IManageable model, FullyQualifiedTableName tableName);
        string CreateQueryForTableAlteration(IManageable model, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, ConstraintDefinition> constraints, FullyQualifiedTableName tableName);
        string GetCreateForeignKeysQuery(IManageable model, FullyQualifiedTableName tableName, Dictionary<string, ConstraintDefinition> constraints);
        string GetSqlDataType(Type codeType, bool isUniqueKey, long dataLength);
    }
}
