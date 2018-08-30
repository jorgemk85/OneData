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
        Guid newObjId;

        [OneTimeSetUp]
        public void PerformSetupForTesting_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            newObjId = TestTools.GetBlogModel(true).Id.GetValueOrDefault();

            Assert.DoesNotThrow(() => Manager<Blog, Guid>.Insert(TestTools.GetBlogModel(false)));
        }

        [Test, Order(1)]
        public void Update_FullAutomation_DoesNotThrow()
        {
            TestTools.GetBlogModel(false).Name = "Parametros Editados";

            Assert.DoesNotThrow(() => Manager<Blog, Guid>.Update(TestTools.GetBlogModel(false)));
        }

        [Test, Order(2)]
        public void Select_FullAutomation_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Manager<Blog, Guid>.Select(null, new Parameter(nameof(Blog.Id), newObjId)));
        }

        [Test, Order(3)]
        public void SelectAll_FullAutomation_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Manager<Blog, Guid>.SelectAll());
        }

        [Test, Order(4)]
        public void Delete_FullAutomation_DoesNotThrow()
        {
            TestTools.GetBlogModel(false).Id = newObjId;

            Assert.DoesNotThrow(() => Manager<Blog, Guid>.Delete(TestTools.GetBlogModel(false)));
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
