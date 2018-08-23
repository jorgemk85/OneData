using DataManagement.DAO;
using DataManagement.Enums;
using DataManagement.Models.Test;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataManagement.Tools.Test
{
    internal class TestTools
    {
        internal static Random Random { get; } = new Random();
        internal static string TestDirectory { get; } = "C:\\tests\\";
        internal static string ExcelFileName { get; } = "test.xlsx";
        internal static string TextFileName { get; } = "test.txt";
        internal static string RandomDirectory { get; } = string.Format("C:\\{0}\\", Random.Next() * 100);
        internal static string ExcelRandomFileName { get; } = string.Format("{0}.xlsx", Random.Next() * 100);
        internal static string TextRandomFileName { get; } = string.Format("{0}.txt", Random.Next() * 100);
        internal static string XmlCollectionTestModel { get; } = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfTestModel xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <TestModel>\r\n    <Ip>192.168.0.1</Ip>\r\n    <Transaccion>Sin transacciones</Transaccion>\r\n    <TablaAfectada>logs</TablaAfectada>\r\n    <Parametros>Sin parametros</Parametros>\r\n  </TestModel>\r\n</ArrayOfTestModel>";
        internal static string XmlTestModel { get; } = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<TestModel xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <Ip>192.168.0.1</Ip>\r\n  <Transaccion>Sin transacciones</Transaccion>\r\n  <TablaAfectada>logs</TablaAfectada>\r\n  <Parametros>Sin parametros</Parametros>\r\n</TestModel>";
        internal static string JsonListTestModel { get; } = "[{\"Ip\":\"192.168.0.1\",\"Transaccion\":\"Sin transacciones\",\"TablaAfectada\":\"logs\",\"Parametros\":\"Sin parametros\"}]";
        internal static string JsonTestModel { get; } = "{\"Ip\":\"192.168.0.1\",\"Transaccion\":\"Sin transacciones\",\"TablaAfectada\":\"logs\",\"Parametros\":\"Sin parametros\"}";
        internal static DataTable DataTableTestModel { get; } = ConvertObjectOfTypeToDataTable(CreateNewTestModel());
        internal static List<TestModel> ListTestModel { get; } = ConvertDataTableToListOfType<TestModel>(DataTableTestModel);
        internal static TestModel TestModel { get; } = CreateNewTestModel();

        private static LogTest CurrentLogTestModel { get; set; }

        internal static void SetConfigurationForAutoCreate(bool enable)
        {
            Manager.AutoCreateTables = enable;
            Manager.AutoCreateStoredProcedures = enable;
        }

        internal static void SetConfigurationForAutoAlter(bool enable)
        {
            Manager.AutoAlterTables = enable;
            Manager.AutoAlterStoredProcedures = enable;
        }

        internal static void SetConfigurationForLogs(bool enable)
        {
            Manager.EnableLogInDatabase = enable;
            Manager.EnableLogInFile = enable;
        }

        internal static void SetDefaultConfiguration(ConnectionTypes connectionType)
        {
            Manager.ConnectionType = connectionType;
            Manager.DefaultSchema = "operaciones";
            Manager.DefaultConnection = connectionType.ToString();
            Manager.SelectSuffix = "_Select";
            Manager.InsertSuffix = "_Insert";
            Manager.InsertListSuffix = "_InsertList";
            Manager.UpdateSuffix = "_Update";
            Manager.DeleteSuffix = "_Delete";
            Manager.SelectAllSuffix = "_SelectAll";
            Manager.StoredProcedurePrefix = "SP_";
            Manager.TablePrefix = "TB_";
        }

        internal static void SetConfigurationForConstantConsolidation(bool enable)
        {
            Manager.ConstantTableConsolidation = enable;
        }

        private static TestModel CreateNewTestModel()
        {
            TestModel newTestModel = new TestModel()
            {
                Ip = "192.168.0.1",
                Parametros = "Sin parametros",
                TablaAfectada = "logs",
                Transaccion = "Sin transacciones"
            };

            return newTestModel;
        }

        internal static LogTest GetLogTestModel(bool giveNew)
        {
            if (giveNew || CurrentLogTestModel == null)
            {
                LogTest newLogTest = new LogTest()
                {
                    Ip = "192.168.0.1",
                    Parametros = "Sin parametros",
                    TablaAfectada = "logs",
                    Transaccion = "Sin transacciones"
                };
                CurrentLogTestModel = newLogTest;
            }

            return CurrentLogTestModel;
        }

        private static DataTable ConvertObjectOfTypeToDataTable<T>(T obj)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                Type type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                dataTable.Columns.Add(prop.Name, type);
            }

            var values = new object[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                values[i] = properties[i].GetValue(obj, null);
            }

            dataTable.Rows.Add(values);

            return dataTable;
        }

        private static List<T> ConvertDataTableToListOfType<T>(DataTable dataTable) where T : new()
        {
            List<T> newList = new List<T>();

            foreach (DataRow row in dataTable.Rows)
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                T newObject = new T();
                foreach (PropertyInfo property in properties)
                {
                    if (dataTable.Columns.Contains(property.Name) && property.CanWrite)
                    {
                        property.SetValue(newObject, SimpleConverter.ConvertStringToType(row[property.Name].ToString(), property.PropertyType));
                    }
                }
                newList.Add(newObject);
            }

            return newList;
        }
    }
}
