using DataManagement.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataManagement.Interfaces
{
    internal interface ICreatable
    {
        void SetStoredProceduresParameters(ref PropertyInfo[] properties, StringBuilder queryBuilder, bool setDefaultNull);

        string CreateInsertStoredProcedure<T>(bool doAlter) where T : IManageable, new();

        string CreateUpdateStoredProcedure<T>(bool doAlter) where T : IManageable, new();

        string CreateDeleteStoredProcedure<T>(bool doAlter) where T : IManageable, new();

        string CreateSelectAllStoredProcedure<T>(bool doAlter) where T : IManageable, new();

        string CreateSelectStoredProcedure<T>(bool doAlter) where T : IManageable, new();

        string GetCreateTableQuery(Type type);

        string CreateQueryForTableCreation(IManageable obj, ref PropertyInfo[] properties);

        string GetAlterTableQuery(Type type, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, KeyDefinition> keyDetails);

        string CreateQueryForTableAlteration(IManageable obj, ref PropertyInfo[] properties, Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, KeyDefinition> keyDetails);

        string GetCreateForeignKeysQuery(Type type, Dictionary<string, KeyDefinition> keyDetails = null);

        string GetSqlDataType(Type codeType);
    }
}
