using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.DataAccess.Utilities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.DataAccess.UnitTests.Utilities
{
    public class SqlReaderHelperTests
    {
        [Fact]
        public void GetNullableInt_NonNullValue_ReturnsInteger()
        {
            // Arrange
            var reader = Substitute.For<DbDataReader>();
            const string columnName = "ColumnName";
            const int expectedValue = 42;
            reader.GetOrdinal(columnName).Returns(0);
            reader.IsDBNull(0).Returns(false);
            reader.GetInt32(0).Returns(expectedValue);

            // Act
            var result = SqlReaderHelper.GetNullableInt(reader, columnName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedValue, result.Value);
        }

        [Fact]
        public void GetNullableInt_NullValue_ReturnsNull()
        {
            // Arrange
            var reader = Substitute.For<DbDataReader>();
            const string columnName = "ColumnName";
            reader.GetOrdinal(columnName).Returns(0);
            reader.IsDBNull(0).Returns(true);

            // Act
            var result = SqlReaderHelper.GetNullableInt(reader, columnName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetNullableInt_ColumnNotExist_ThrowsException()
        {
            // Arrange
            var reader = Substitute.For<DbDataReader>();
            const string columnName = "NonExistentColumn";
            reader.GetOrdinal(columnName).Throws(new IndexOutOfRangeException());

            // Act & Assert
            Assert.Throws<IndexOutOfRangeException>(() => SqlReaderHelper.GetNullableInt(reader, columnName));
        }

        [Fact]
        public void GetNullableGuid_NonNullValue_ReturnsGuid()
        {
            var reader = Substitute.For<DbDataReader>();
            var expected = Guid.NewGuid();
            reader.GetOrdinal("ColumnName").Returns(0);
            reader.IsDBNull(0).Returns(false);
            reader.GetGuid(0).Returns(expected);

            var result = SqlReaderHelper.GetNullableGuid(reader, "ColumnName");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetNullableGuid_NullValue_ReturnsNull()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("ColumnName").Returns(0);
            reader.IsDBNull(0).Returns(true);

            var result = SqlReaderHelper.GetNullableGuid(reader, "ColumnName");

            Assert.Null(result);
        }

        [Fact]
        public void GetNullableGuid_InvalidColumn_Throws()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("BadColumn").Throws(new IndexOutOfRangeException());

            Assert.Throws<IndexOutOfRangeException>(() => SqlReaderHelper.GetNullableGuid(reader, "BadColumn"));
        }

        [Fact]
        public void GetNullableDateTime_NonNullValue_ReturnsDateTime()
        {
            var reader = Substitute.For<DbDataReader>();
            var expected = DateTime.UtcNow;
            reader.GetOrdinal("ColumnName").Returns(0);
            reader.IsDBNull(0).Returns(false);
            reader.GetDateTime(0).Returns(expected);

            var result = SqlReaderHelper.GetNullableDateTime(reader, "ColumnName");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetNullableDateTime_NullValue_ReturnsNull()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("ColumnName").Returns(0);
            reader.IsDBNull(0).Returns(true);

            var result = SqlReaderHelper.GetNullableDateTime(reader, "ColumnName");

            Assert.Null(result);
        }

        [Fact]
        public void GetNullableDateTime_InvalidColumn_Throws()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("BadColumn").Throws(new IndexOutOfRangeException());

            Assert.Throws<IndexOutOfRangeException>(() => SqlReaderHelper.GetNullableDateTime(reader, "BadColumn"));
        }

        [Fact]
        public void GetNullableString_NonNullValue_ReturnsString()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("ColumnName").Returns(0);
            reader.IsDBNull(0).Returns(false);
            reader.GetString(0).Returns("TestValue");

            var result = SqlReaderHelper.GetNullableString(reader, "ColumnName");

            Assert.Equal("TestValue", result);
        }

        [Fact]
        public void GetNullableString_NullValue_ReturnsNull()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("ColumnName").Returns(0);
            reader.IsDBNull(0).Returns(true);

            var result = SqlReaderHelper.GetNullableString(reader, "ColumnName");

            Assert.Null(result);
        }

        [Fact]
        public void GetNullableString_InvalidColumn_Throws()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("BadColumn").Throws(new IndexOutOfRangeException());

            Assert.Throws<IndexOutOfRangeException>(() => SqlReaderHelper.GetNullableString(reader, "BadColumn"));
        }

        [Fact]
        public void GetNullableBool_NonNullValue_ReturnsBool()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("ColumnName").Returns(0);
            reader.IsDBNull(0).Returns(false);
            reader.GetBoolean(0).Returns(true);

            var result = SqlReaderHelper.GetNullableBool(reader, "ColumnName");

            Assert.True(result);
        }

        [Fact]
        public void GetNullableBool_NullValue_ReturnsNull()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("ColumnName").Returns(0);
            reader.IsDBNull(0).Returns(true);

            var result = SqlReaderHelper.GetNullableBool(reader, "ColumnName");

            Assert.Null(result);
        }

        [Fact]
        public void GetNullableBool_InvalidColumn_Throws()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("BadColumn").Throws(new IndexOutOfRangeException());

            Assert.Throws<IndexOutOfRangeException>(() => SqlReaderHelper.GetNullableBool(reader, "BadColumn"));
        }

        [Fact]
        public void GetNullableDecimal_NonNullValue_ReturnsDecimal()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("ColumnName").Returns(0);
            reader.IsDBNull(0).Returns(false);
            reader.GetDecimal(0).Returns(123.45m);

            var result = SqlReaderHelper.GetNullableDecimal(reader, "ColumnName");

            Assert.Equal(123.45m, result);
        }

        [Fact]
        public void GetNullableDecimal_NullValue_ReturnsNull()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("ColumnName").Returns(0);
            reader.IsDBNull(0).Returns(true);

            var result = SqlReaderHelper.GetNullableDecimal(reader, "ColumnName");

            Assert.Null(result);
        }

        [Fact]
        public void GetNullableDecimal_InvalidColumn_Throws()
        {
            var reader = Substitute.For<DbDataReader>();
            reader.GetOrdinal("BadColumn").Throws(new IndexOutOfRangeException());

            Assert.Throws<IndexOutOfRangeException>(() => SqlReaderHelper.GetNullableDecimal(reader, "BadColumn"));
        }

    }
}
