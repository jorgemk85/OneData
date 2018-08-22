using DataManagement.Models.Test;
using DataManagement.Tools;
using DataManagement.Tools.Test;
using NUnit.Framework;
using System;
using System.IO;

namespace DataManagement.UnitTests
{
    [TestFixture]
    class ExcelSerializerTests
    {
        [Test, Order(1)]
        public void SerializeListOfTypeToExcel_FolderExist_ReturnsNoError()
        {
            Directory.CreateDirectory(TestTools.TestDirectory);

            Assert.DoesNotThrow(() => ExcelSerializer.SerializeListOfTypeToExcel(TestTools.ListTestModel, TestTools.TestDirectory + TestTools.ExcelFileName));
        }

        [Test]
        public void SerializeListOfTypeToExcel_FolderDoesNotExist_ReturnsError()
        {
            Assert.Throws<InvalidOperationException>(() => ExcelSerializer.SerializeListOfTypeToExcel(TestTools.ListTestModel, TestTools.RandomDirectory + TestTools.ExcelRandomFileName));
        }

        [Test, Order(2)]
        public void DeserializeExcelToListOfType_FileExist_ReturnsNoError()
        {
            Assert.DoesNotThrow(() => ExcelSerializer.DeserializeExcelToListOfType<TestModel>("Pagina 1", TestTools.TestDirectory + TestTools.ExcelFileName));
        }

        [Test]
        public void DeserializeExcelToListOfType_FileDoesNotExist_ReturnsError()
        {
            Assert.Throws<FileNotFoundException>(() => ExcelSerializer.DeserializeExcelToListOfType<TestModel>("Pagina 1", TestTools.RandomDirectory + TestTools.ExcelRandomFileName));
        }
    }
}
