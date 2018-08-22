using DataManagement.DAO;
using DataManagement.Models;
using DataManagement.Tools.Test;
using NUnit.Framework;
using System;

namespace DataManagement.IntegrationTests
{
    [TestFixture]
    class ManagerTests
    {
        Guid newLogId;

        [Test, Order(1)]
        public void Insert_FullAutomation_ReturnsNoError()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            newLogId = TestTools.GetLogModel(true).Id.GetValueOrDefault();

            Assert.DoesNotThrow(() => Manager<Log>.Insert(TestTools.GetLogModel(false)));
        }

        [Test, Order(2)]
        public void Update_FullAutomation_ReturnsNoError()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            TestTools.GetLogModel(true).Parametros = "Parametros Editados";

            Assert.DoesNotThrow(() => Manager<Log>.Update(TestTools.GetLogModel(false)));
        }

        [Test, Order(3)]
        public void Select_FullAutomation_ReturnsNoError()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            Assert.DoesNotThrow(() => Manager<Log>.Select(null, new Parameter(nameof(Log.Id), newLogId)));
        }

        [Test, Order(4)]
        public void SelectAll_FullAutomation_ReturnsNoError()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            Assert.DoesNotThrow(() => Manager<Log>.SelectAll());
        }

        [Test, Order(5)]
        public void Delete_FullAutomation_ReturnsNoError()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(true);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);

            Assert.DoesNotThrow(() => Manager<Log>.Delete(TestTools.GetLogModel(false)));
        }
    }
}
