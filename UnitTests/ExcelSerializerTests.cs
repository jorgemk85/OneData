using OneData.Models.Test;
using OneData.Tools;
using OneData.Tools.Test;
using NUnit.Framework;
using System;
using System.IO;

namespace OneData.UnitTests
{
    [TestFixture]
    class ExcelSerializerTests
    {
        //[Test, Order(1)]
        //public void SerializeListOfTypeToExcel_FolderExist_ReturnsNoError()
        //{
        //    Directory.CreateDirectory(TestTools.TestDirectory);

        //    Assert.DoesNotThrow(() => ExcelSerializer.SerializeIEnumerableOfTypeToExcel(TestTools.ListTestModel, TestTools.TestDirectory + TestTools.ExcelFileName));
        //}

        //[Test]
        //public void SerializeListOfTypeToExcel_FolderDoesNotExist_ReturnsError()
        //{
        //    Assert.Throws<InvalidOperationException>(() => ExcelSerializer.SerializeIEnumerableOfTypeToExcel(TestTools.ListTestModel, TestTools.RandomDirectory + TestTools.ExcelRandomFileName));
        //}

        //[Test, Order(2)]
        //public void DeserializeExcelToListOfType_FileExist_ReturnsNoError()
        //{
        //    Assert.DoesNotThrow(() => ExcelSerializer.DeserializeExcelToListOfType<TestModel>("Pagina 1", TestTools.TestDirectory + TestTools.ExcelFileName));
        //}

        //[Test]
        //public void DeserializeExcelToListOfType_FileDoesNotExist_ReturnsError()
        //{
        //    Assert.Throws<FileNotFoundException>(() => ExcelSerializer.DeserializeExcelToListOfType<TestModel>("Pagina 1", TestTools.RandomDirectory + TestTools.ExcelRandomFileName));
        //}
    }
}
