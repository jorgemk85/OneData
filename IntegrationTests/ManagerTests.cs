using DataManagement.DAO;
using DataManagement.Models;
using DataManagement.Tools.Test;
using NUnit.Framework;
using System;

namespace DataManagement.IntegrationTests
{
    [TestFixture]
    class ManagerTests
    {
        Guid newLogId;

        [Test, Order(1)]
        public void Insert_ObjectWithValues_ReturnsNoError()
        {
            newLogId = TestTools.LogModel.Id.GetValueOrDefault();

            Assert.DoesNotThrow(() => Manager<Log>.Insert(TestTools.LogModel));
        }

        [Test, Order(2)]
        public void Update_ObjectWithValues_ReturnsNoError()
        {
            TestTools.LogModel.Parametros = "Parametros Editados";
            Assert.DoesNotThrow(() => Manager<Log>.Update(TestTools.LogModel));
        }

        [Test, Order(3)]
        public void Select_ObjectWithValues_ReturnsNoError()
        {
            Assert.DoesNotThrow(() => Manager<Log>.Select(null, new Parameter(nameof(Log.Id), newLogId)));
        }

        [Test, Order(4)]
        public void SelectAll_ObjectWithValues_ReturnsNoError()
        {
            Assert.DoesNotThrow(() => Manager<Log>.SelectAll());
        }

        [Test, Order(5)]
        public void Delete_ObjectWithValues_ReturnsNoError()
        {
            Assert.DoesNotThrow(() => Manager<Log>.Delete(TestTools.LogModel));
        }
    }
}
