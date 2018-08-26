using DataManagement.DAO;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Models.Test;
using DataManagement.Tools.Test;
using NUnit.Framework;
using System;

namespace DataManagement.IntegrationTests.MsSql
{
    [SingleThreaded]
    [TestFixture]
    class ManagerAutomatedTests
    {
        Guid newLogId;

        [OneTimeSetUp]
        public void PerformSetupForTesting_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            newLogId = TestTools.GetLogTestGuidModel(true).Id.GetValueOrDefault();

            Assert.DoesNotThrow(() => Manager<LogTestGuid, Guid>.Insert(TestTools.GetLogTestGuidModel(false)));
        }

        [Test, Order(1)]
        public void Update_FullAutomation_DoesNotThrow()
        {
            TestTools.GetLogTestGuidModel(false).Parametros = "Parametros Editados";

            Assert.DoesNotThrow(() => Manager<LogTestGuid, Guid>.Update(TestTools.GetLogTestGuidModel(false)));
        }

        [Test, Order(2)]
        public void Select_FullAutomation_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Manager<LogTestGuid, Guid>.Select(null, new Parameter(nameof(LogTestGuid.Id), newLogId)));
        }

        [Test, Order(3)]
        public void SelectAll_FullAutomation_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Manager<LogTestGuid, Guid>.SelectAll());
        }

        [Test, Order(4)]
        public void Delete_FullAutomation_DoesNotThrow()
        {
            TestTools.GetLogTestGuidModel(false).Id = newLogId;

            Assert.DoesNotThrow(() => Manager<LogTestGuid, Guid>.Delete(TestTools.GetLogTestGuidModel(false)));
        }

        [OneTimeTearDown]
        public void PerformSetFalseInEveryConfiguration()
        {
            TestTools.SetConfigurationForConstantConsolidation(false);
            TestTools.SetConfigurationForAutoCreate(false);
            TestTools.SetConfigurationForAutoAlter(false);
        }
    }
}
