using DataManagement.DAO;
using DataManagement.Models;
using DataManagement.Models.Test;
using DataManagement.Tools.Test;
using NUnit.Framework;
using System;

namespace DataManagement.IntegrationTests.MsSql
{
    [TestFixture]
    class ManagerAutomatedTestsInt
    {
        [OneTimeSetUp]
        public void PerformSetupForTesting_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            Assert.DoesNotThrow(() => Manager<LogTestInt>.Insert(TestTools.GetLogTestIntModel(false), null));
        }

        [Test, Order(1)]
        public void Update_FullAutomation_DoesNotThrow()
        {
            TestTools.GetLogTestIntModel(false).Parametros = "Parametros Editados";

            Assert.DoesNotThrow(() => Manager<LogTestInt>.Update(TestTools.GetLogTestIntModel(false), null));
        }

        [Test, Order(3)]
        public void SelectAll_FullAutomation_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Manager<LogTestInt>.SelectAll(null));
        }

        [Test, Order(4)]
        public void Delete_FullAutomation_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Manager<LogTestInt>.Delete(TestTools.GetLogTestIntModel(true), null));
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
