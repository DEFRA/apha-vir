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

        [Fact]
        public void AVNumberIsValidPotentially_ValidAVFormat_ReturnsTrue()
        {
            string av = "AV123-1";
            bool result = AVNumberUtil.AVNumberIsValidPotentially(av);
            Assert.True(result);
        }

        [Fact]
        public void AVNumberIsValidPotentially_ValidPDFormat_ReturnsTrue()
        {
            string pd = "PD123-1";
            bool result = AVNumberUtil.AVNumberIsValidPotentially(pd);
            Assert.True(result);
        }

        [Fact]
        public void AVNumberIsValidPotentially_ValidSIFormat_ReturnsTrue()
        {
            string si = "SI123-1";
            bool result = AVNumberUtil.AVNumberIsValidPotentially(si);
            Assert.True(result);
        }

        [Fact]
        public void AVNumberIsValidPotentially_ValidBNFormat_ReturnsTrue()
        {
            string bn = "BN123-1";
            bool result = AVNumberUtil.AVNumberIsValidPotentially(bn);
            Assert.True(result);
        }

        [Fact]
        public void AVNumberIsValidPotentially_InvalidFormat_ReturnsFalse()
        {
            string invalid = "XX123-1";
            bool result = AVNumberUtil.AVNumberIsValidPotentially(invalid);
            Assert.False(result);
        }

        [Fact]
        public void AVNumberFormatted_WithAVPrefix_FormatsCorrectly()
        {
            string input = "AV123-23";
            string result = AVNumberUtil.AVNumberFormatted(input);
            Assert.Equal("AV000123-23", result);
        }

        [Fact]
        public void AVNumberFormatted_WithSIPrefix_FormatsCorrectly()
        {
            string input = "SI45-5";
            string result = AVNumberUtil.AVNumberFormatted(input);
            Assert.Equal("SI000045-05", result);
        }

        [Fact]
        public void AVNumberFormatted_WithBNPrefix_FormatsCorrectly()
        {
            string input = "BN789-9";
            string result = AVNumberUtil.AVNumberFormatted(input);
            Assert.Equal("BN000789-09", result);
        }

        [Fact]
        public void AVNumberFormatted_WithPDPrefix_FormatsCorrectly()
        {
            string input = "PD12-7";
            string result = AVNumberUtil.AVNumberFormatted(input);
            Assert.Equal("PD0012-07", result);
        }

        [Fact]
        public void AVNumberFormatted_WithUnknownPrefix_ReturnsInput()
        {
            string input = "XX123-45";
            string result = AVNumberUtil.AVNumberFormatted(input);
            Assert.Equal(input.ToUpper(), result);
        }

        [Fact]
        public void AVNumberFormatted_NoDash_ReturnsInput()
        {
            string input = "AV12345";
            string result = AVNumberUtil.AVNumberFormatted(input);
            Assert.Equal("AV12345", result);
        }

        [Fact]
        public void AVNumberFormatted_InvalidNumber_ReturnsInput()
        {
            string input = "AVabc-xy"; // parsing will throw, caught in catch
            string result = AVNumberUtil.AVNumberFormatted(input);
            Assert.Equal(input.ToUpper(), result);
        }


    }
}
