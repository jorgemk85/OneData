using DataManagement.DAO;
using DataManagement.Models;
using DataManagement.Models.Test;
using DataManagement.Tools.Test;
using NUnit.Framework;
using System;

namespace DataManagement.IntegrationTests
{
    [SingleThreaded]
    [TestFixture]
    class ManagerTests
    {
        Guid newLogId;

        [OneTimeSetUp]
        public void PerformFullyAutomatedInsert_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            newLogId = TestTools.GetLogTestModel(true).Id.GetValueOrDefault();

            Assert.DoesNotThrow(() => Manager<LogTest>.Insert(TestTools.GetLogTestModel(false)));
        }

        [Test, Order(1)]
        public void Update_FullAutomation_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            TestTools.GetLogTestModel(true).Parametros = "Parametros Editados";

            Assert.DoesNotThrow(() => Manager<LogTest>.Update(TestTools.GetLogTestModel(false)));
        }

        [Test, Order(2)]
        public void Select_FullAutomation_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            Assert.DoesNotThrow(() => Manager<LogTest>.Select(null, new Parameter(nameof(LogTest.Id), newLogId)));
        }

        [Test, Order(3)]
        public void SelectAll_FullAutomation_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            Assert.DoesNotThrow(() => Manager<LogTest>.SelectAll());
        }

        [Test, Order(4)]
        public void Delete_FullAutomation_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            TestTools.GetLogTestModel(true).Id = newLogId;

            Assert.DoesNotThrow(() => Manager<LogTest>.Delete(TestTools.GetLogTestModel(false)));
        }
    }
}
