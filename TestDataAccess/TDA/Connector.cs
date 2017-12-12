using DataAccess;
using DataAccess.BO;
using DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TDA
{
    public class Connector : DataAccess<Connector>, IConnectable
    {
        public Result Delete<T>(T obj) where T : new() => Execute(StoredProcedures.TransactionTypes.Delete, obj);

        public Result Insert<T>(T obj) where T : new() => Execute(StoredProcedures.TransactionTypes.Insert, obj);

        public Result Update<T>(T obj) where T : new() => Execute(StoredProcedures.TransactionTypes.Update, obj);

        public Result SelectOther(string tableName, string storedProcedure, params Parameter[] parameters)
        {
            return Execute<Object>(StoredProcedures.TransactionTypes.SelectOther, tableName: tableName, storedProcedure: storedProcedure, parameters: parameters);
        }

        public T Select<T>(params Parameter[] parameters) where T : new()
        {
            return DataAccess.BO.Tools.ConvertDataTableToObjectOfType<T>(Execute<T>(StoredProcedures.TransactionTypes.Select, parameters: parameters).Data);
        }

        public Dictionary<Guid, T> SelectDictionary<T>(params Parameter[] parameters) where T : new()
        {
            return DataAccess.BO.Tools.ConvertDataTableToDictionaryOfType<T>(Execute<T>(StoredProcedures.TransactionTypes.Select, parameters: parameters).Data);
        }

        public List<T> SelectList<T>(params Parameter[] parameters) where T : new()
        {
            return DataAccess.BO.Tools.ConvertDataTableToListOfType<T>(Execute<T>(StoredProcedures.TransactionTypes.Select, parameters: parameters).Data);
        }

        public Dictionary<Guid, T> SelectAllDictionary<T>() where T : new()
        {
            return DataAccess.BO.Tools.ConvertDataTableToDictionaryOfType<T>(Execute<T>(StoredProcedures.TransactionTypes.SelectAll).Data);
        }

        public List<T> SelectAllList<T>() where T : new()
        {
            return DataAccess.BO.Tools.ConvertDataTableToListOfType<T>(Execute<T>(StoredProcedures.TransactionTypes.SelectAll).Data);
        }

        private Result Execute<T>(StoredProcedures.TransactionTypes transactionType, T obj = default, string tableName = "", string storedProcedure = "", params Parameter[] parameters) where T : new()
        {
            Result result = null;
            switch (transactionType)
            {
                case StoredProcedures.TransactionTypes.Select:
                    result = DataAccess<T>.Select(parameters);
                    break;
                case StoredProcedures.TransactionTypes.SelectAll:
                    result = DataAccess<T>.SelectAll();
                    break;
                case StoredProcedures.TransactionTypes.Delete:
                    result = DataAccess<T>.Delete(obj);
                    break;
                case StoredProcedures.TransactionTypes.Insert:
                    result = DataAccess<T>.Insert(obj);
                    break;
                case StoredProcedures.TransactionTypes.Update:
                    result = DataAccess<T>.Update(obj);
                    break;
                case StoredProcedures.TransactionTypes.SelectOther:
                    result = DataAccess<Object>.Select(tableName, storedProcedure, parameters);
                    break;
                default:
                    break;
            }

            if (!result.TuvoExito) MessageBox.Show(result.Mensaje, result.TituloMensaje, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return result;
        }
    }
}
