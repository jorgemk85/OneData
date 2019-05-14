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
        string CreateQueryForTableCreation(IManageable model);
        string CreateQueryForTableAlteration(IManageable model, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, ConstraintDefinition> constraints);
        string GetCreateForeignKeysQuery(IManageable model, Dictionary<string, ConstraintDefinition> constraints = null);
        string GetSqlDataType(Type codeType, bool isUniqueKey, long dataLength);
    }
}
