using DataManagement.DAO;
using DataManagement.Extensions;
using DataManagement.Models;
using DataManagement.Models.Test;
using DataManagement.Tools.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DataManagement.IntegrationTests.MsSql
{
    [TestFixture]
    class ManagerTests
    {
        [OneTimeSetUp]
        public void PerformSetupForTesting_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(false);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);
        }

        [Test]
        public void Select_DataFromCache_ReturnsTrue()
        {
            List<LogTestGuid> list = new List<LogTestGuid>().SelectAll<LogTestGuid, Guid>();
            Result result = Manager<LogTestGuid, Guid>.Select(null, new Parameter(nameof(LogTestGuid.Id), list[0].Id));

            Assert.IsTrue(result.IsFromCache);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreNotEqual(result.Data.Rows.Count, 0);
        }

    }
}
