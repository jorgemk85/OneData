using DataAccess.BO;
using DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TDA
{
    public class Connector : DataAccess<Connector>
    {
        bool useAppConfig = false;

        public Connector()
        {
            Connection.ConnectionString = "server=infected.biz;user id=infected_system;password=N5941Qnu4ucIejO;persistsecurityinfo=True;database=infected_rms_licence;";
        }

        public override Result Delete<T>(T obj) => Execute(StoredProcedures.TransactionTypes.Delete, obj);

        public override Result Insert<T>(T obj) => Execute(StoredProcedures.TransactionTypes.Insert, obj);

        public override Result Update<T>(T obj) => Execute(StoredProcedures.TransactionTypes.Update, obj);

        public override Result SelectOther(string tableName, string storedProcedure, params Parameter[] parameters)
        {
            return Execute<Object>(StoredProcedures.TransactionTypes.SelectOther, tableName: tableName, storedProcedure: storedProcedure, parameters: parameters);
        }

        public override T Select<T>(params Parameter[] parameters)
        {
            return Tools.ConvertDataTableToObjectOfType<T>(Execute<T>(StoredProcedures.TransactionTypes.Select, parameters: parameters).Data);
        }

        public override Dictionary<Guid, T> SelectDictionary<T>(params Parameter[] parameters)
        {
            return Tools.ConvertDataTableToDictionaryOfType<T>(Execute<T>(StoredProcedures.TransactionTypes.Select, parameters: parameters).Data);
        }

        public override List<T> SelectList<T>(params Parameter[] parameters)
        {
            return Tools.ConvertDataTableToListOfType<T>(Execute<T>(StoredProcedures.TransactionTypes.Select, parameters: parameters).Data);
        }

        public override Dictionary<Guid, T> SelectAllDictionary<T>()
        {
            return Tools.ConvertDataTableToDictionaryOfType<T>(Execute<T>(StoredProcedures.TransactionTypes.SelectAll).Data);
        }

        public override List<T> SelectAllList<T>()
        {
            return Tools.ConvertDataTableToListOfType<T>(Execute<T>(StoredProcedures.TransactionTypes.SelectAll).Data);
        }

        private Result Execute<T>(StoredProcedures.TransactionTypes transactionType, T obj = default, string tableName = "", string storedProcedure = "", params Parameter[] parameters) where T : new()
        {
            Result result = null;
            switch (transactionType)
            {
                case StoredProcedures.TransactionTypes.Select:
                    result = DataAccess<T>.Select(useAppConfig, parameters);
                    break;
                case StoredProcedures.TransactionTypes.SelectAll:
                    result = DataAccess<T>.SelectAll(useAppConfig);
                    break;
                case StoredProcedures.TransactionTypes.Delete:
                    result = DataAccess<T>.Delete(obj, useAppConfig);
                    break;
                case StoredProcedures.TransactionTypes.Insert:
                    result = DataAccess<T>.Insert(obj, useAppConfig);
                    break;
                case StoredProcedures.TransactionTypes.Update:
                    result = DataAccess<T>.Update(obj, useAppConfig);
                    break;
                case StoredProcedures.TransactionTypes.SelectOther:
                    result = DataAccess<Object>.Select(tableName, storedProcedure, useAppConfig, parameters);
                    break;
                default:
                    break;
            }

            if (!result.TuvoExito) MessageBox.Show(result.Mensaje, result.TituloMensaje, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return result;
        }
    }
}
