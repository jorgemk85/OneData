using DataManagement.DAO;
using DataManagement.Models;
using DataManagement.Models.Test;
using DataManagement.Tools.Test;
using NUnit.Framework;
using System;

namespace DataManagement.IntegrationTests.MySql
{
    [TestFixture]
    class ManagerAutomatedTestsInt
    {
        [OneTimeSetUp]
        public void PerformSetupForTesting_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MySQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            Assert.DoesNotThrow(() => Manager<LogTestInt, int>.Insert(TestTools.GetLogTestIntModel(false)));
        }

        [Test, Order(1)]
        public void Update_FullAutomation_DoesNotThrow()
        {
            TestTools.GetLogTestGuidModel(false).Parametros = "Parametros Editados";

            Assert.DoesNotThrow(() => Manager<LogTestInt, int>.Update(TestTools.GetLogTestIntModel(false)));
        }

        [Test, Order(2)]
        public void Select_FullAutomation_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Manager<LogTestInt, int>.Select(null, new Parameter(nameof(LogTestInt.Id), 1)));
        }

        [Test, Order(3)]
        public void SelectAll_FullAutomation_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Manager<LogTestInt, int>.SelectAll());
        }

        [Test, Order(4)]
        public void Delete_FullAutomation_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Manager<LogTestInt, int>.Delete(TestTools.GetLogTestIntModel(true)));
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
