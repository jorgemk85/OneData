using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Models;
using DataManagement.Models.Test;
using DataManagement.Tools;
using DataManagement.Tools.Test;
using NUnit.Framework;
using System;

namespace DataManagement.UnitTests
{
    [TestFixture]
    class ConsolidationToolsTests
    {
        [Test]
        public void PerformNullValidation_ObjectHasNulls_ReturnError()
        {
            TestModel newTestModel = new TestModel();

            Assert.Throws<FoundNullException>(() => ConsolidationTools.PerformNullValidation(newTestModel, true));
        }

        [Test]
        public void PerformNullValidation_ObjectHasNoNulls_ReturnTrue()
        {
            Assert.IsTrue(ConsolidationTools.PerformNullValidation(TestTools.TestModel, true));
        }

        [Test]
        public void SetValuesIntoObjectOfType_AllValuesSet_ReturnNoError()
        {
            Assert.DoesNotThrow(() => ConsolidationTools.SetValuesIntoObjectOfType(TestTools.TestModel, new { Id = Guid.Parse("967939da-9fea-476e-9229-9f8ccb8abfd6"), Ip = "192.168.0.1", Parametros = "Sin parametros", TablaAfectada = "logs", Transaccion = "Sin transacciones" }));
        }

        [Test]
        public void SetValuesIntoObjectOfType_AllValuesSet_ReturnAllValuesSet()
        {
            TestModel newTestModel = new TestModel();

            ConsolidationTools.SetValuesIntoObjectOfType(newTestModel, TestTools.TestModel);
            Assert.AreEqual(TestTools.TestModel.Ip, newTestModel.Ip);
        }

        [Test]
        public void GetValueFromConfiguration_GetsValue_ReturnError()
        {
            Assert.Throws<ConfigurationNotFoundException>(() => ConsolidationTools.GetValueFromConfiguration("", ConfigurationTypes.AppSetting));
        }

        [Test]
        public void GetValueFromConfiguration_GetsValue_ReturnNoError()
        {
            Assert.DoesNotThrow(() => ConsolidationTools.GetValueFromConfiguration("DefaultConnection", ConfigurationTypes.AppSetting));
        }

        [Test]
        public void GetValueFromConfiguration_GetsBoolean_ReturnTrue()
        {
            bool? value = null;
            Assert.DoesNotThrow(() => value = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoCreateTables", ConfigurationTypes.AppSetting)));
            Assert.IsInstanceOf(typeof(bool), value.GetValueOrDefault());
        }
    }
}
