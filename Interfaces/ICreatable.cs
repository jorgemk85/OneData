using DataManagement.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataManagement.Interfaces
{
    internal interface ICreatable
    {
        void SetStoredProceduresParameters<T>(StringBuilder queryBuilder, bool setDefaultNull, bool considerId) where T : Cope<T>, IManageable, new();
        string CreateInsertStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new();
        string CreateInsertMassiveStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new();
        string CreateUpdateStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new();
        string CreateDeleteStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new();
        string CreateSelectAllStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new();
        string CreateSelectStoredProcedure<T>(bool doAlter) where T : Cope<T>, IManageable, new();
        string CreateQueryForTableCreation<T>() where T : Cope<T>, IManageable, new();
        string CreateQueryForTableAlteration<T>(Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, KeyDefinition> keyDetails) where T : Cope<T>, IManageable, new();
        string GetCreateForeignKeysQuery<T>(Dictionary<string, KeyDefinition> keyDetails = null) where T : Cope<T>, IManageable, new();
        string GetSqlDataType(Type codeType, bool isUniqueKey, long dataLength);
    }
}
