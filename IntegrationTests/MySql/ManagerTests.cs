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
            TestTools.GetLogTestGuidModel(true).Insert<LogTestGuid, Guid>();
            List<LogTestGuid> list = new List<LogTestGuid>().SelectAll<LogTestGuid, Guid>();
            Result result = Manager<LogTestGuid, Guid>.Select(null, new Parameter(nameof(LogTestGuid.Id), list[0].Id));
            TestTools.GetLogTestGuidModel(false).Delete<LogTestGuid, Guid>();

            Assert.IsTrue(result.IsFromCache);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreNotEqual(result.Data.Rows.Count, 0);
        }

        [Test]
        public void SelectInt_DataFromCache_ReturnsTrue()
        {
            TestTools.GetLogTestIntModel(true).Insert<LogTestInt, int>();
            List<LogTestInt> list = new List<LogTestInt>().SelectAll<LogTestInt, int>();
            Result result = Manager<LogTestInt, int>.Select(null, new Parameter(nameof(LogTestInt.Id), list[0].Id));
            TestTools.GetLogTestIntModel(false).Delete<LogTestInt, int>();

            Assert.IsTrue(result.IsFromCache);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreNotEqual(result.Data.Rows.Count, 0);
        }
    }
}
