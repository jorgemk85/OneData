using DataManagement.Models.Test;
using System.Collections.Generic;
using System.Data;

namespace DataManagement.Tools.Test
{
    internal class TestTools
    {
        internal static string XmlCollectionTestModel { get; } = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfTestModel xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <TestModel>\r\n    <Ip>192.168.0.1</Ip>\r\n    <Transaccion>Sin transacciones</Transaccion>\r\n    <TablaAfectada>logs</TablaAfectada>\r\n    <Parametros>Sin parametros</Parametros>\r\n  </TestModel>\r\n</ArrayOfTestModel>";
        internal static string XmlTestModel { get; } = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<TestModel xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <Ip>192.168.0.1</Ip>\r\n  <Transaccion>Sin transacciones</Transaccion>\r\n  <TablaAfectada>logs</TablaAfectada>\r\n  <Parametros>Sin parametros</Parametros>\r\n</TestModel>";
        internal static string JsonListTestModel { get; } = "[{\"Ip\":\"192.168.0.1\",\"Transaccion\":\"Sin transacciones\",\"TablaAfectada\":\"logs\",\"Parametros\":\"Sin parametros\"}]";
        internal static string JsonTestModel { get; } = "{\"Ip\":\"192.168.0.1\",\"Transaccion\":\"Sin transacciones\",\"TablaAfectada\":\"logs\",\"Parametros\":\"Sin parametros\"}";
        internal static DataTable DataTableTestModel { get; } = DataSerializer.ConvertObjectOfTypeToDataTable(CreateNewTestModel());
        internal static List<TestModel> ListTestModel { get; } = DataSerializer.ConvertDataTableToListOfType<TestModel>(DataTableTestModel);
        internal static TestModel TestModel { get; } = CreateNewTestModel();

        internal static TestModel CreateNewTestModel()
        {
            TestModel newLog = new TestModel()
            {
                Ip = "192.168.0.1",
                Parametros = "Sin parametros",
                TablaAfectada = "logs",
                Transaccion = "Sin transacciones"
            };

            return newLog;
        }
    }
}
