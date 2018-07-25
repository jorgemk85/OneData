using DataAccess.BO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace DataAccess.DAO
{
    public class QueryEvaluation
    {
        MySqlOperation mySQLProcedures = new MySqlOperation();
        MsSqlOperation msSQLProcedures = new MsSqlOperation();
        ConnectionTypes connectionType;

        public enum ConnectionTypes
        {
            MySQL,
            MSSQL
        }

        public QueryEvaluation()
        {
            connectionType = (ConnectionTypes)Enum.Parse(typeof(ConnectionTypes), ConfigurationManager.AppSettings["ConnectionType"].ToString());
        }

        public enum TransactionTypes
        {
            Select,
            SelectAll,
            Delete,
            Insert,
            Update,
            SelectOther
        }

        public Result Evaluate<T>(T obj, TransactionTypes transactionType, Result cache, bool isPartialCache, bool forceQueryDataBase, bool useAppConfig) where T : new()
        {
            string tableName = (obj as Main).DataBaseTableName;
            Result resultado = new Result();
            bool isCached = cache == null ? false : true;
            bool requiresResult = false;

            switch (transactionType)
            {
                case TransactionTypes.Select:
                    EvaluateSelect(obj, out resultado, isCached, tableName, cache, isPartialCache, forceQueryDataBase, useAppConfig);
                    break;
                case TransactionTypes.SelectAll:
                    EvaluateSelectAll(obj, out resultado, isCached, tableName, cache, forceQueryDataBase, useAppConfig);
                    break;
                case TransactionTypes.Delete:
                    requiresResult = true;
                    break;
                case TransactionTypes.Insert:
                    requiresResult = true;
                    break;
                case TransactionTypes.Update:
                    requiresResult = true;
                    break;
                case TransactionTypes.SelectOther:
                    if (connectionType == ConnectionTypes.MySQL)
                    {
                        resultado = mySQLProcedures.ExecuteProcedure(obj, tableName, transactionType, useAppConfig, connectionType);
                    }
                    else
                    {
                        resultado = msSQLProcedures.ExecuteProcedure(obj, tableName, transactionType, useAppConfig, connectionType);
                    }
                    break;
                default:
                    break;
            }

            if (requiresResult)
            {
                if (connectionType == ConnectionTypes.MySQL)
                {
                    resultado = mySQLProcedures.ExecuteProcedure(obj, tableName, transactionType, useAppConfig, connectionType);
                }
                else
                {
                    resultado = msSQLProcedures.ExecuteProcedure(obj, tableName, transactionType, useAppConfig, connectionType);
                }
                if (transactionType != TransactionTypes.SelectOther)
                {
                    if (isCached && resultado.TuvoExito) DeleteInCache(obj, cache);
                }
            }

            return resultado;
        }

        private void EvaluateSelect<T>(T obj, out Result resultado, bool isCached, string tableName, Result cache, bool isPartialCache, bool forceQueryDataBase, bool useAppConfig) where T : new()
        {
            if (forceQueryDataBase)
            {
                if (connectionType == ConnectionTypes.MySQL)
                {
                    resultado = mySQLProcedures.ExecuteProcedure(obj, tableName, TransactionTypes.Select, useAppConfig, connectionType);
                }
                else
                {
                    resultado = msSQLProcedures.ExecuteProcedure(obj, tableName, TransactionTypes.Select, useAppConfig, connectionType);
                }
            }
            else
            {
                if (connectionType == ConnectionTypes.MySQL)
                {
                    resultado = isCached == true ? SelectInCache(obj, cache) : mySQLProcedures.ExecuteProcedure(obj, tableName, TransactionTypes.Select, useAppConfig, connectionType);
                }
                else
                {
                    resultado = isCached == true ? SelectInCache(obj, cache) : msSQLProcedures.ExecuteProcedure(obj, tableName, TransactionTypes.Select, useAppConfig, connectionType);
                }

                resultado.IsFromCache = isCached == true ? true : false;
                if (isCached && isPartialCache && resultado.TuvoExito && resultado.Data.Rows.Count == 0)
                {
                    if (connectionType == ConnectionTypes.MySQL)
                    {
                        resultado = mySQLProcedures.ExecuteProcedure(obj, tableName, TransactionTypes.Select, useAppConfig, connectionType);
                    }
                    else
                    {
                        resultado = msSQLProcedures.ExecuteProcedure(obj, tableName, TransactionTypes.Select, useAppConfig, connectionType);
                    }
                }
            }
        }

        private void EvaluateSelectAll<T>(T obj, out Result resultado, bool isCached, string tableName, Result cache, bool forceQueryDataBase, bool useAppConfig)
        {
            if (forceQueryDataBase)
            {
                if (connectionType == ConnectionTypes.MySQL)
                {
                    resultado = mySQLProcedures.ExecuteProcedure(obj, tableName, TransactionTypes.SelectAll, useAppConfig, connectionType);
                }
                else
                {
                    resultado = msSQLProcedures.ExecuteProcedure(obj, tableName, TransactionTypes.SelectAll, useAppConfig, connectionType);
                }
            }
            else
            {
                if (connectionType == ConnectionTypes.MySQL)
                {
                    resultado = isCached == true ? cache : mySQLProcedures.ExecuteProcedure(obj, tableName, TransactionTypes.SelectAll, useAppConfig, connectionType);
                }
                else
                {
                    resultado = isCached == true ? cache : msSQLProcedures.ExecuteProcedure(obj, tableName, TransactionTypes.SelectAll, useAppConfig, connectionType);
                }
                resultado.IsFromCache = isCached == true ? true : false;
            }
        }

        private Result SelectInCache<T>(T obj, Result cache) where T : new()
        {
            int valueIndex = 0;
            List<object> values = new List<object>();
            string predicate = string.Empty;

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (Attribute.GetCustomAttribute(prop, typeof(UnlinkedProperty)) == null)
                {
                    if (prop.GetValue(obj) != null)
                    {
                        predicate += prop.Name + "== @" + valueIndex + " and ";
                        values.Add(prop.GetValue(obj));
                        valueIndex++;
                    }
                }
            }

            if (string.IsNullOrEmpty(predicate))
            {
                return new Result(exito: false, titulo: "Error en los parametros.", mensaje: "Al parecer no se establecieron paramentros para la busqueda o no se obtuvieron resultados previos a la consulta. Por favor asegurece de por lo menos colocar uno valido y volver a intentar.");
            }
            else
            {
                predicate = predicate.Substring(0, predicate.Length - 5);
                return new Result(exito: true, data: Tools.ConvertListToDataTableOfType(Tools.ConvertDataTableToListOfType<T>(cache.Data).Where(predicate, values.ToArray()).ToList()));
            }
        }

        private DataRow SetRowData<T>(DataRow row, T obj, bool isInsert)
        {
            object value = null;
            Type type;

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (row.Table.Columns.Contains(prop.Name))
                {
                    value = prop.GetValue(obj);
                    // La base de datos no acepta nulls, entonces verifica si es para asignarle un valor
                    if (value == null)
                    {
                        type = Nullable.GetUnderlyingType(prop.PropertyType) != null ? type = Nullable.GetUnderlyingType(prop.PropertyType) : type = prop.PropertyType;

                        switch (type.Name)
                        {
                            case "String":
                                value = string.Empty;
                                break;
                            default:
                                value = Activator.CreateInstance(type);
                                break;
                        }
                    }
                    row[prop.Name] = value;
                }
            }
            return row;
        }

        private void UpdateInCache<T>(T obj, Result cache)
        {
            SetRowData<T>(cache.Data.Rows.Find((obj as Main).Id), obj, false).AcceptChanges();
        }

        private void InsertInCache<T>(T obj, Result cache)
        {
            (obj as Main).FechaCreacion = DateTime.Now;
            (obj as Main).FechaModificacion = DateTime.Now;

            cache.Data.Rows.Add(SetRowData(cache.Data.NewRow(), obj, true));
            cache.Data.AcceptChanges();
        }

        public void AlterCache(DataRow row, Result cache)
        {
            DataRow cacheRow = cache.Data.Rows.Find(row[row.Table.PrimaryKey[0]]);
            string columnName = string.Empty;

            if (cacheRow == null)
            {
                // NO existe la fila: la agrega.
                cache.Data.Rows.Add(row.ItemArray);
                cache.Data.AcceptChanges();
            }
            else
            {
                // SI existe la fila: la actualiza.
                for (int i = 0; i < cacheRow.ItemArray.Length; i++)
                {
                    columnName = cacheRow.Table.Columns[i].ColumnName;
                    cacheRow[columnName] = row[columnName];
                }
            }
            cache.Data.AcceptChanges();
        }

        private void DeleteInCache<T>(T obj, Result cache)
        {
            for (int i = 0; i < cache.Data.Rows.Count; i++)
            {
                DataRow row = cache.Data.Rows[i];
                if (row[row.Table.PrimaryKey[0]].Equals((obj as Main).Id.GetValueOrDefault()))
                {
                    row.Delete();
                    cache.Data.AcceptChanges();
                    break;
                }
            }
        }
    }
}
