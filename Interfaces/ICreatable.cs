using DataManagement.Standard.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataManagement.Standard.Interfaces
{
    internal interface ICreatable
    {
        void SetStoredProceduresParameters(ref PropertyInfo[] properties, StringBuilder queryBuilder, bool setDefaultNull);
        string CreateInsertStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new();
        string CreateInsertListStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new();
        string CreateUpdateStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new();
        string CreateDeleteStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new();
        string CreateSelectAllStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new();
        string CreateSelectStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new();
        string GetCreateTableQuery<TKey>(Type type);
        string CreateQueryForTableCreation<TKey>(IManageable<TKey> obj, ref PropertyInfo[] properties);
        string GetAlterTableQuery<TKey>(Type type, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, KeyDefinition> keyDetails);
        string CreateQueryForTableAlteration<TKey>(IManageable<TKey> obj, ref PropertyInfo[] properties, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, KeyDefinition> keyDetails);
        string GetCreateForeignKeysQuery<TKey>(Type type, Dictionary<string, KeyDefinition> keyDetails = null);
        string GetSqlDataType(Type codeType);
    }
}
