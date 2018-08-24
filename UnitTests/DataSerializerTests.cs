using DataManagement.Standard.Models.Test;
using DataManagement.Standard.Tools;
using DataManagement.Standard.Tools.Test;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;

namespace DataManagement.Standard.UnitTests
{
    [TestFixture]
    class DataSerializerTests
    {
        [Test]
        public void SerializeDataTableToJsonObjectOfType_SerializationSucceeds_ReturnsEquals()
        {
            string json = DataSerializer.SerializeDataTableToJsonObjectOfType<TestModel>(TestTools.DataTableTestModel);

            Assert.AreEqual(json, TestTools.JsonTestModel);
        }

        [Test]
        public void SerializeDataTableToJsonListOfType_SerializationSucceeds_ReturnsEquals()
        {
            string json = DataSerializer.SerializeDataTableToJsonListOfType<TestModel>(TestTools.DataTableTestModel);

            Assert.AreEqual(json, TestTools.JsonListTestModel);
        }

        [Test]
        public void DeserializeJsonToListOfType_DeserializationSucceeds_ReturnsEquals()
        {
            List<TestModel> deserializedTestModel = DataSerializer.DeserializeJsonToListOfType<TestModel>(TestTools.JsonListTestModel);

            Assert.AreEqual(deserializedTestModel[0].Ip, TestTools.ListTestModel[0].Ip);
        }

        [Test]
        public void DeserializeJsonToObjectOfType_DeserializationSucceeds_ReturnsEquals()
        {
            TestModel deserializedTestModel = DataSerializer.DeserializeJsonToObjectOfType<TestModel>(TestTools.JsonTestModel);

            Assert.AreEqual(TestTools.TestModel.Ip, deserializedTestModel.Ip);
        }

        [Test]
        public void SerializeDataTableToXmlListOfType_SerializationSucceeds_ReturnsEquals()
        {
            string xml = DataSerializer.SerializeDataTableToXmlListOfType<TestModel>(TestTools.DataTableTestModel);

            Assert.AreEqual(xml, TestTools.XmlCollectionTestModel);
        }

        [Test]
        public void SerializeDataTableToXmlObjectOfType_SerializationSucceeds_ReturnsEquals()
        {
            string xml = DataSerializer.SerializeDataTableToXmlObjectOfType<TestModel>(TestTools.DataTableTestModel);

            Assert.AreEqual(xml, TestTools.XmlTestModel);
        }

        [Test]
        public void SerializeListOfTypeToXml_SerializationSucceeds_ReturnsEquals()
        {
            string xml = DataSerializer.SerializeListOfTypeToXml(TestTools.ListTestModel);

            Assert.AreEqual(xml, TestTools.XmlCollectionTestModel);
        }

        [Test]
        public void SerializeObjectOfTypeToXml_SerializationSucceeds_ReturnsEquals()
        {
            string xml = DataSerializer.SerializeObjectOfTypeToXml(TestTools.TestModel);

            Assert.AreEqual(xml, TestTools.XmlTestModel);
        }

        [Test]
        public void ConvertDataTableToObjectOfType_SerializationSucceeds_ReturnsEquals()
        {
            TestModel convertedTestModel = DataSerializer.ConvertDataTableToObjectOfType<TestModel>(TestTools.DataTableTestModel);

            Assert.AreEqual(convertedTestModel.Ip, TestTools.TestModel.Ip);
        }

        [Test]
        public void ConvertDataTableToDictionary_TKey_TValue_SerializationSucceeds_ReturnsEquals()
        {
            Dictionary<string, string> dictionaryTestModel = DataSerializer.ConvertDataTableToDictionary<string, string>(TestTools.DataTableTestModel, nameof(TestModel.Ip), nameof(TestModel.TablaAfectada));

            Assert.AreEqual(dictionaryTestModel[TestTools.TestModel.Ip], TestTools.TestModel.TablaAfectada);
        }

        [Test]
        public void ConvertDataTableToDictionary_TKey_T_SerializationSucceeds_ReturnsEquals()
        {
            Dictionary<string, TestModel> dictionaryTestModel = DataSerializer.ConvertDataTableToDictionary<string, TestModel>(TestTools.DataTableTestModel, nameof(TestModel.Ip));

            Assert.AreEqual(dictionaryTestModel[TestTools.TestModel.Ip].TablaAfectada, TestTools.TestModel.TablaAfectada);
        }

        [Test]
        public void ConvertDataTableToListOfType_T_SerializationSucceeds_ReturnsEquals()
        {
            List<TestModel> listTestModel = DataSerializer.ConvertDataTableToListOfType<TestModel>(TestTools.DataTableTestModel);

            Assert.AreEqual(listTestModel[0].TablaAfectada, TestTools.ListTestModel[0].TablaAfectada);
        }

        [Test]
        public void ConvertListToDataTableOfGenericType_T_SerializationSucceeds_ReturnsEquals()
        {
            DataTable dataTableTestModel = DataSerializer.ConvertListToDataTableOfGenericType(TestTools.ListTestModel);

            Assert.AreEqual(dataTableTestModel.Rows[0][0], TestTools.DataTableTestModel.Rows[0][0]);
        }

        [Test]
        public void ConvertObjectOfTypeToDataTable_T_SerializationSucceeds_ReturnsEquals()
        {
            DataTable dataTableTestModel = DataSerializer.ConvertObjectOfTypeToDataTable(TestTools.TestModel);

            Assert.AreEqual(dataTableTestModel.Rows[0][0], TestTools.DataTableTestModel.Rows[0][0]);
        }
    }
}
