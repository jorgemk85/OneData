using OneData.DAO;
using OneData.Models;
using OneData.Models.Test;
using OneData.Tools.Test;
using NUnit.Framework;
using System;

namespace OneData.IntegrationTests.MsSql
{
    [SingleThreaded]
    [TestFixture]
    class ManagerAutomatedTests
    {
        Guid newObjId;

        [OneTimeSetUp]
        public void PerformSetupForTesting_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            newObjId = TestTools.GetBlogModel(true).Id.GetValueOrDefault();

            Assert.DoesNotThrow(() => Manager<Blog>.Insert(TestTools.GetBlogModel(false), null));
        }

        [Test, Order(1)]
        public void Update_FullAutomation_DoesNotThrow()
        {
            TestTools.GetBlogModel(false).Name = "Parametros Editados";

            Assert.DoesNotThrow(() => Manager<Blog>.Update(TestTools.GetBlogModel(false), null));
        }

        [Test, Order(3)]
        public void SelectAll_FullAutomation_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Manager<Blog>.SelectAll(null));
        }

        [Test, Order(4)]
        public void Delete_FullAutomation_DoesNotThrow()
        {
            TestTools.GetBlogModel(false).Id = newObjId;

            Assert.DoesNotThrow(() => Manager<Blog>.Delete(TestTools.GetBlogModel(false), null));
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
