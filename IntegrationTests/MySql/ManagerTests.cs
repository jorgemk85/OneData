using DataManagement.DAO;
using DataManagement.Extensions;
using DataManagement.Models;
using DataManagement.Models.Test;
using DataManagement.Tools.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DataManagement.IntegrationTests.MySql
{
    [TestFixture]
    class ManagerTests
    {
        [OneTimeSetUp]
        public void PerformSetupForTesting_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MySQL);
            TestTools.SetConfigurationForConstantConsolidation(false);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);
        }

        [Test]
        public void SelectGuid_DataFromCache_ReturnsTrue()
        {
            LogTestGuid.Insert(TestTools.GetLogTestGuidModel(true));
            List<LogTestGuid> list = LogTestGuid.SelectAll();
            Result result = LogTestGuid.SelectResult(new Parameter(nameof(LogTestGuid.Id), list[0].Id));
            LogTestGuid.Delete(TestTools.GetLogTestGuidModel(false));

            Assert.IsTrue(result.IsFromCache);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreNotEqual(result.Data.Rows.Count, 0);
        }

        [Test]
        public void SelectInt_DataFromCache_ReturnsTrue()
        {
            LogTestInt.Insert(TestTools.GetLogTestIntModel(true));
            List<LogTestInt> list = LogTestInt.SelectAll();
            Result result = LogTestInt.SelectResult(new Parameter(nameof(LogTestInt.Id), list[0].Id));
            LogTestInt.Delete(TestTools.GetLogTestIntModel(false));

            Assert.IsTrue(result.IsFromCache);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreNotEqual(result.Data.Rows.Count, 0);
        }
    }
}
