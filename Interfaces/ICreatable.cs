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

        string CreateInsertStoredProcedure<T>() where T : IManageable, new();

        string CreateUpdateStoredProcedure<T>() where T : IManageable, new();

        string CreateDeleteStoredProcedure<T>() where T : IManageable, new();

        string CreateSelectAllStoredProcedure<T>() where T : IManageable, new();

        string CreateSelectStoredProcedure<T>() where T : IManageable, new();

        string GetCreateTableQuery<T>() where T : IManageable, new();

        string GetCreateTableQuery(Type type);

        string CreateQueryForTableCreation(IManageable obj, ref PropertyInfo[] properties);

        string GetCreateForeignKeysQuery(Type type);

        string GetSqlDataType(Type codeType);
    }
}
