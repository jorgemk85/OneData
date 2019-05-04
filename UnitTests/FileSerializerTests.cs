using OneData.Models.Test;
using OneData.Tools;
using OneData.Tools.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneData.UnitTests
{
    [TestFixture]
    class FileSerializerTests
    {
        [Test, Order(1)]
        public void SerializeListOfTypeToFile_FolderExist_ReturnsNoError()
        {
            Directory.CreateDirectory(TestTools.TestDirectory);

            Assert.DoesNotThrow(() => FileSerializer.SerializeIEnumerableOfTypeToFile(TestTools.ListTestModel, TestTools.TestDirectory + TestTools.TextFileName, '|'));
        }

        [Test]
        public void SerializeListOfTypeToFile_FolderDoesNotExist_ReturnsError()
        {
            Assert.Throws<DirectoryNotFoundException>(() => FileSerializer.SerializeIEnumerableOfTypeToFile(TestTools.ListTestModel, TestTools.RandomDirectory + TestTools.TextRandomFileName, '|'));
        }

        [Test, Order(2)]
        public void DeserializeFileToListOfType_FileExist_ReturnsNoError()
        {
            Assert.DoesNotThrow(() => FileSerializer.DeserializeFileToListOfType<TestModel>(TestTools.TestDirectory + TestTools.TextFileName, '|', Encoding.UTF7));
        }

        [Test]
        public void DeserializeFileToListOfType_FileDoesNotExist_ReturnsError()
        {
            Assert.Throws<DirectoryNotFoundException>(() => FileSerializer.DeserializeFileToListOfType<TestModel>(TestTools.RandomDirectory + TestTools.TextRandomFileName, '|', Encoding.UTF7));
        }
    }
}
