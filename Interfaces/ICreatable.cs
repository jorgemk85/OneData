using DataManagement.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataManagement.Interfaces
{
    internal interface ICreatable
    {
        void SetStoredProceduresParameters<T, TKey>(StringBuilder queryBuilder, bool setDefaultNull, bool considerId) where T : Cope<T, TKey>, new() where TKey : struct;
        string CreateInsertStoredProcedure<T, TKey>(bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct;
        string CreateInsertListStoredProcedure<T, TKey>(bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct;
        string CreateUpdateStoredProcedure<T, TKey>(bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct;
        string CreateDeleteStoredProcedure<T, TKey>(bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct;
        string CreateSelectAllStoredProcedure<T, TKey>(bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct;
        string CreateSelectStoredProcedure<T, TKey>(bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct;
        string CreateQueryForTableCreation<T, TKey>() where T : Cope<T, TKey>, new() where TKey : struct;
        string CreateQueryForTableAlteration<T, TKey>(Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, KeyDefinition> keyDetails) where T : Cope<T, TKey>, new() where TKey : struct;
        string GetCreateForeignKeysQuery<T, TKey>(Dictionary<string, KeyDefinition> keyDetails = null) where T : Cope<T, TKey>, new() where TKey : struct;
        string GetSqlDataType(Type codeType);
    }
}
