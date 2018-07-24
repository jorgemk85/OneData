using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public abstract class DataConnectionType
    {
        protected Guid IdentificadorId { get; set; } = Guid.Empty;
        protected string SelectSuffix { get; set; }
        protected string InsertSuffix { get; set; }
        protected string UpdateSuffix { get; set; }
        protected string DeleteSuffix { get; set; }
        protected string SelectAllSuffix { get; set; }

        public DataConnectionType()
        {
            GetTransactionTypesSuffixes();
        }

        private void GetTransactionTypesSuffixes()
        {
            SelectSuffix = ConfigurationManager.AppSettings["SelectSuffix"].ToString();
            InsertSuffix = ConfigurationManager.AppSettings["InsertSuffix"].ToString();
            UpdateSuffix = ConfigurationManager.AppSettings["UpdateSuffix"].ToString();
            DeleteSuffix = ConfigurationManager.AppSettings["DeleteSuffix"].ToString();
            SelectAllSuffix = ConfigurationManager.AppSettings["SelectAllSuffix"].ToString();
        }

        protected string GetFriendlyTransactionSuffix(QueryEvaluation.TransactionTypes transactionType)
        {
            switch (transactionType)
            {
                case QueryEvaluation.TransactionTypes.Select:
                    return SelectSuffix;
                case QueryEvaluation.TransactionTypes.Delete:
                    return DeleteSuffix;
                case QueryEvaluation.TransactionTypes.Insert:
                    return InsertSuffix;
                case QueryEvaluation.TransactionTypes.Update:
                    return UpdateSuffix;
                case QueryEvaluation.TransactionTypes.SelectAll:
                    return SelectAllSuffix;
                default:
                    return SelectAllSuffix;
            }
        }
    }
}
