using DataManagement.Standard.DAO;
using DataManagement.Standard.Models;
using DataManagement.Standard.Models.Test;
using DataManagement.Standard.Tools;
using DataManagement.Standard.Tools.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DataManagement.Standard.IntegrationTests.MsSql
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
            List<LogTest> list = DataSerializer.ConvertDataTableToListOfType<LogTest>(Manager<LogTest, Guid>.SelectAll().Data);
            Result result = Manager<LogTest, Guid>.Select(null, new Parameter(nameof(LogTest.Id), list[0].Id));

            Assert.IsTrue(result.IsFromCache);
        }
    }
}
