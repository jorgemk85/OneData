﻿using DataManagement.Enums;
using System;
using System.Reflection;
using System.Text;

namespace DataManagement.Interfaces
{
    internal interface ICreatable
    {
        string TablePrefix { get; set; }
        string StoredProcedurePrefix { get; set; }
        string InsertSuffix { get; set; }
        string SelectSuffix { get; set; }
        string SelectAllSuffix { get; set; }
        string UpdateSuffix { get; set; }
        string DeleteSuffix { get; set; }

        void SetConfigurationProperties();

        void SetStoredProceduresParameters(ref PropertyInfo[] properties, StringBuilder queryBuilder, bool setDefaultNull);

        string CreateInsertStoredProcedure<T>(bool doAlter) where T : IManageable, new();

        string CreateUpdateStoredProcedure<T>(bool doAlter) where T : IManageable, new();

        string CreateDeleteStoredProcedure<T>(bool doAlter) where T : IManageable, new();

        string CreateSelectAllStoredProcedure<T>(bool doAlter) where T : IManageable, new();

        string CreateSelectStoredProcedure<T>(bool doAlter) where T : IManageable, new();

        string GetCreateTableQuery<T>(bool doAlter) where T : IManageable, new();

        string GetCreateTableQuery(Type type, bool doAlter);

        string CreateQueryForTableCreation(IManageable obj, ref PropertyInfo[] properties, bool doAlter);

        string GetCreateForeignKeysQuery(Type type);

        string GetSqlDataType(Type codeType);
    }
}