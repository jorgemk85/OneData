using DataManagement.Enums;
using DataManagement.Tools;

namespace DataManagement.DAO
{
    internal abstract class Creation
    {
        public string TablePrefix { get; set; }
        public string StoredProcedurePrefix { get; set; }
        public string InsertSuffix { get; set; }
        public string SelectSuffix { get; set; }
        public string SelectAllSuffix { get; set; }
        public string UpdateSuffix { get; set; }
        public string DeleteSuffix { get; set; }

        public Creation()
        {
            Logger.Info("Getting configuration properties for Creation class.");
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
    }
}
