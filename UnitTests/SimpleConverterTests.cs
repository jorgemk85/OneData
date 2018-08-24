using DataManagement.Standard.Exceptions;
using DataManagement.Standard.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.Standard.UnitTests
{
    [TestFixture]
    class SimpleConverterTests
    {
        #region Integers
        [Test]
        public void ConvertStringToType_CorrectInt_ReturnsInt()
        {
            Assert.IsInstanceOf<int>(SimpleConverter.ConvertStringToType("1", typeof(int)));
        }

        [Test]
        public void ConvertStringToType_CorrectNullableInt_ReturnsNullableInt()
        {
            Assert.IsInstanceOf<int?>(SimpleConverter.ConvertStringToType("1", typeof(int?)));
        }

        [Test]
        public void ConvertStringToType_CorrectNullableInt_ReturnsInt()
        {
            Assert.IsInstanceOf<int>(SimpleConverter.ConvertStringToType("1", typeof(int?)));
        }

        [Test]
        public void ConvertStringToType_IncorrectInt_ReturnsError()
        {
            Assert.Throws<ConvertionFailedException>(() => SimpleConverter.ConvertStringToType("s", typeof(int?)));
        }
        #endregion

        #region Decimals
        [Test]
        public void ConvertStringToType_CorrectDecimal_ReturnsDecimal()
        {
            Assert.IsInstanceOf<decimal>(SimpleConverter.ConvertStringToType("1.23", typeof(decimal)));
        }

        [Test]
        public void ConvertStringToType_CorrectNullableDecimal_ReturnsNullableDecimal()
        {
            Assert.IsInstanceOf<decimal?>(SimpleConverter.ConvertStringToType("1", typeof(decimal?)));
        }

        [Test]
        public void ConvertStringToType_CorrectNullableDecimal_ReturnsDecimal()
        {
            Assert.IsInstanceOf<decimal>(SimpleConverter.ConvertStringToType("106465.1515", typeof(decimal?)));
        }

        [Test]
        public void ConvertStringToType_IncorrectDecimal_ReturnsError()
        {
            Assert.Throws<ConvertionFailedException>(() => SimpleConverter.ConvertStringToType("s", typeof(decimal?)));
        }
        #endregion

        #region Doubles
        [Test]
        public void ConvertStringToType_CorrectDouble_ReturnsDouble()
        {
            Assert.IsInstanceOf<double>(SimpleConverter.ConvertStringToType("1.23", typeof(double)));
        }

        [Test]
        public void ConvertStringToType_CorrectNullableDouble_ReturnsNullableDouble()
        {
            Assert.IsInstanceOf<double?>(SimpleConverter.ConvertStringToType("1", typeof(double?)));
        }

        [Test]
        public void ConvertStringToType_CorrectNullableDouble_ReturnsDouble()
        {
            Assert.IsInstanceOf<double>(SimpleConverter.ConvertStringToType("106465.1515", typeof(double?)));
        }

        [Test]
        public void ConvertStringToType_IncorrectDouble_ReturnsError()
        {
            Assert.Throws<ConvertionFailedException>(() => SimpleConverter.ConvertStringToType("s", typeof(double?)));
        }
        #endregion

        #region Floats
        [Test]
        public void ConvertStringToType_CorrectFloat_ReturnsFloat()
        {
            Assert.IsInstanceOf<float>(SimpleConverter.ConvertStringToType("1.23", typeof(float)));
        }

        [Test]
        public void ConvertStringToType_CorrectNullableFloat_ReturnsNullableFloat()
        {
            Assert.IsInstanceOf<float?>(SimpleConverter.ConvertStringToType("1", typeof(float?)));
        }

        [Test]
        public void ConvertStringToType_CorrectNullableFloat_ReturnsFloat()
        {
            Assert.IsInstanceOf<float>(SimpleConverter.ConvertStringToType("106465.1515", typeof(float?)));
        }

        [Test]
        public void ConvertStringToType_IncorrectFloat_ReturnsError()
        {
            Assert.Throws<ConvertionFailedException>(() => SimpleConverter.ConvertStringToType("s", typeof(float?)));
        }
        #endregion

        #region Guids
        [Test]
        public void ConvertStringToType_CorrectGuid_ReturnsGuid()
        {
            Assert.IsInstanceOf<Guid>(SimpleConverter.ConvertStringToType("221993d8-d6ce-4261-98e5-5f7ccd82c4c6", typeof(Guid)));
        }

        [Test]
        public void ConvertStringToType_CorrectNullableGuid_ReturnsNullableGuid()
        {
            Assert.IsInstanceOf<Guid?>(SimpleConverter.ConvertStringToType("221993d8-d6ce-4261-98e5-5f7ccd82c4c6", typeof(Guid?)));
        }

        [Test]
        public void ConvertStringToType_CorrectNullableGuid_ReturnsGuid()
        {
            Assert.IsInstanceOf<Guid>(SimpleConverter.ConvertStringToType("221993d8-d6ce-4261-98e5-5f7ccd82c4c6", typeof(Guid?)));
        }

        [Test]
        public void ConvertStringToType_IncorrectGuid_ReturnsError()
        {
            Assert.Throws<ConvertionFailedException>(() => SimpleConverter.ConvertStringToType("s", typeof(Guid?)));
        }
        #endregion

        #region DateTimes
        [Test]
        public void ConvertStringToType_CorrectDate_ReturnsDateTime()
        {
            Assert.IsInstanceOf<DateTime>(SimpleConverter.ConvertStringToType("08/09/1985", typeof(DateTime)));
        }

        [Test]
        public void ConvertStringToType_CorrectNullableDateTime_ReturnsNullableDateTime()
        {
            Assert.IsInstanceOf<DateTime?>(SimpleConverter.ConvertStringToType("08/09/1985 05:06:50 AM", typeof(DateTime?)));
        }

        [Test]
        public void ConvertStringToType_CorrectNullableDateTime_ReturnsDateTime()
        {
            Assert.IsInstanceOf<DateTime>(SimpleConverter.ConvertStringToType("08/09/1985 16:34:01", typeof(DateTime?)));
        }

        [Test]
        public void ConvertStringToType_IncorrectDateTime_ReturnsError()
        {
            Assert.Throws<ConvertionFailedException>(() => SimpleConverter.ConvertStringToType("s", typeof(DateTime?)));
        }
        #endregion

    }
}
