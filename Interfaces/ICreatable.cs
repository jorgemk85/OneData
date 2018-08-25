using DataManagement.Standard.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataManagement.Standard.Interfaces
{
    internal interface ICreatable
    {
        void SetStoredProceduresParameters<T, TKey>(PropertiesData<T> properties, T obj, StringBuilder queryBuilder, bool setDefaultNull, bool considerId) where T : IManageable<TKey>, new() where TKey : struct;
        string CreateInsertStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new() where TKey : struct;
        string CreateInsertListStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new() where TKey : struct;
        string CreateUpdateStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new() where TKey : struct;
        string CreateDeleteStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new() where TKey : struct;
        string CreateSelectAllStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new() where TKey : struct;
        string CreateSelectStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new() where TKey : struct;
        string GetCreateTableQuery<TKey>(Type type) where TKey : struct;
        string CreateQueryForTableCreation<TKey>(IManageable<TKey> obj, ref PropertyInfo[] properties) where TKey : struct;
        string GetAlterTableQuery<TKey>(Type type, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, KeyDefinition> keyDetails) where TKey : struct;
        string CreateQueryForTableAlteration<TKey>(IManageable<TKey> obj, ref PropertyInfo[] properties, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, KeyDefinition> keyDetails) where TKey : struct;
        string GetCreateForeignKeysQuery<TKey>(Type type, Dictionary<string, KeyDefinition> keyDetails = null) where TKey : struct;
        string GetSqlDataType(Type codeType);
    }
}
