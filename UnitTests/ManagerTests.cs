using DataManagement.Standard.DAO;
using NUnit.Framework;
using System.Diagnostics;

namespace DataManagement.Standard.UnitTests
{
    [TestFixture]
    class ManagerTests
    {
        [Test]
        public void GetConfigurationSettings_ConfigurationFound_ReturnNoError()
        {
            Assert.DoesNotThrow(() => Manager.GetConfigurationSettings());
        }

        [Test]
        public void GetPrefixesAndSuffixes_ConfigurationFound_ReturnNoError()
        {
            Assert.DoesNotThrow(() => Manager.GetPrefixesAndSuffixes());
        }

        [Test]
        public void SetIfDebug_ConfigurationFound_ReturnTrue()
        {
            Manager.SetIfDebug();

            Assert.IsTrue(Manager.IsDebug);
        }
    }
}
