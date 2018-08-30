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
            TestTools.GetBlogModel(true).Insert(TestTools.GetBlogModel(true));
            List<Blog> list = TestTools.GetBlogModel(false).SelectAll();
            Result result = TestTools.GetBlogModel(false).SelectResult(new Parameter(nameof(Blog.Id), list[0].Id));
            Blog.Delete(TestTools.GetBlogModel(false));

            Assert.IsTrue(result.IsFromCache);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreNotEqual(result.Data.Rows.Count, 0);
        }

        [Test]
        public void SelectInt_DataFromCache_ReturnsTrue()
        {
            TestTools.GetLogTestIntModel(true).Insert(TestTools.GetLogTestIntModel(true));
            List<LogTestInt> list = TestTools.GetLogTestIntModel(false).SelectAll();
            Result result = TestTools.GetLogTestIntModel(false).SelectResult(new Parameter(nameof(LogTestInt.Id), list[0].Id));
            LogTestInt.Delete(TestTools.GetLogTestIntModel(false));

            Assert.IsTrue(result.IsFromCache);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreNotEqual(result.Data.Rows.Count, 0);
        }
    }
}
