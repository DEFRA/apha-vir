using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Web.Utilities;

namespace Apha.VIR.Web.UnitTests.Utilities
{
    public class AVNumberUtilTests
    {
        [Fact]
        public void IsAVNumberValid_ValidAVNumber_ReturnsTrue()
        {
            string avNumber = "AV123456-12";
            bool result = AVNumberUtil.IsAVNumberValid(avNumber);
            Assert.True(result);
        }

        [Fact]
        public void IsAVNumberValid_ValidPDNumber_ReturnsTrue()
        {
            string pdNumber = "PD1234-12";
            bool result = AVNumberUtil.IsAVNumberValid(pdNumber);
            Assert.True(result);
        }

        [Fact]
        public void IsAVNumberValid_ValidSINumber_ReturnsTrue()
        {
            string siNumber = "SI123456-12";
            bool result = AVNumberUtil.IsAVNumberValid(siNumber);
            Assert.True(result);
        }

        [Fact]
        public void IsAVNumberValid_ValidBNNumber_ReturnsTrue()
        {
            string bnNumber = "BN123456-12";
            bool result = AVNumberUtil.IsAVNumberValid(bnNumber);
            Assert.True(result);
        }

        [Fact]
        public void IsAVNumberValid_InvalidFormat_ReturnsFalse()
        {
            string invalidNumber = "AV12345-123";
            bool result = AVNumberUtil.IsAVNumberValid(invalidNumber);
            Assert.False(result);
        }

        [Fact]
        public void IsAVNumberValid_MissingHyphen_ReturnsFalse()
        {
            string invalidNumber = "AV12345612";
            bool result = AVNumberUtil.IsAVNumberValid(invalidNumber);
            Assert.False(result);
        }

        [Fact]
        public void IsAVNumberValid_EmptyString_ReturnsFalse()
        {
            string emptyString = "";
            bool result = AVNumberUtil.IsAVNumberValid(emptyString);
            Assert.False(result);
        }

        [Fact]
        public void IsAVNumberValid_WhitespaceOnly_ReturnsFalse()
        {
            string whitespace = " ";
            bool result = AVNumberUtil.IsAVNumberValid(whitespace);
            Assert.False(result);
        }

        [Fact]
        public void IsAVNumberValid_LowercaseInput_ReturnsTrue()
        {
            string lowercaseNumber = "av123456-12";
            bool result = AVNumberUtil.IsAVNumberValid(lowercaseNumber);
            Assert.True(result);
        }
    }
}
