using DataManagement.Enums;
using DataManagement.Interfaces;
using DataManagement.Tools;
using System;
using System.Reflection;
using System.Text;

namespace DataManagement.DAO
{
    internal class MySqlCreation : ICreatable
    {
        public string TablePrefix { get; set; }
        public string StoredProcedurePrefix { get; set; }
        public string InsertSuffix { get; set; }
        public string SelectSuffix { get; set; }
        public string SelectAllSuffix { get; set; }
        public string UpdateSuffix { get; set; }
        public string DeleteSuffix { get; set; }

        public MySqlCreation()
        {
            SetConfigurationProperties();
        }

        public void SetConfigurationProperties()
        {
            TablePrefix = ConsolidationTools.GetValueFromConfiguration("TablePrefix", ConfigurationTypes.AppSetting);
            StoredProcedurePrefix = ConsolidationTools.GetValueFromConfiguration("StoredProcedurePrefix", ConfigurationTypes.AppSetting);
            InsertSuffix = ConsolidationTools.GetValueFromConfiguration("InsertSuffix", ConfigurationTypes.AppSetting);
            SelectSuffix = ConsolidationTools.GetValueFromConfiguration("SelectSuffix", ConfigurationTypes.AppSetting);
            SelectAllSuffix = ConsolidationTools.GetValueFromConfiguration("SelectAllSuffix", ConfigurationTypes.AppSetting);
            UpdateSuffix = ConsolidationTools.GetValueFromConfiguration("UpdateSuffix", ConfigurationTypes.AppSetting);
            DeleteSuffix = ConsolidationTools.GetValueFromConfiguration("DeleteSuffix", ConfigurationTypes.AppSetting);
        }

        public string CreateDeleteStoredProcedure<T>() where T : IManageable, new()
        {
            throw new NotImplementedException();
        }

        public string CreateInsertStoredProcedure<T>() where T : IManageable, new()
        {
            throw new NotImplementedException();
        }

        public string CreateQueryForTableCreation(IManageable obj, ref PropertyInfo[] properties)
        {
            throw new NotImplementedException();
        }

        public string CreateSelectAllStoredProcedure<T>() where T : IManageable, new()
        {
            throw new NotImplementedException();
        }

        public string CreateSelectStoredProcedure<T>() where T : IManageable, new()
        {
            throw new NotImplementedException();
        }

        public string CreateUpdateStoredProcedure<T>() where T : IManageable, new()
        {
            throw new NotImplementedException();
        }

        public string GetCreateForeignKeysQuery(Type type)
        {
            throw new NotImplementedException();
        }

        public string GetCreateTableQuery<T>() where T : IManageable, new()
        {
            throw new NotImplementedException();
        }

        public string GetCreateTableQuery(Type type)
        {
            throw new NotImplementedException();
        }

        public string GetSqlDataType(Type codeType)
        {
            throw new NotImplementedException();
        }

        public void SetStoredProceduresParameters(ref PropertyInfo[] properties, StringBuilder queryBuilder, bool setDefaultNull)
        {
            throw new NotImplementedException();
        }
    }
}
