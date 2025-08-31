using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DevKit.ExecutionEngine.SqlServer.Implementations;
using Microsoft.Data.SqlClient;
using Xunit;

namespace DevKit.ExecutionEngine.SqlServer.Tests
{
    public class SQLServerProviderBulkCopyTests : IDisposable
    {
        private readonly SQLServerProvider _provider;
        private readonly string _connectionString;
        private readonly string _testTable = "TestBulkCopy";

        public SQLServerProviderBulkCopyTests()
        {
            // Use a test database connection string
            _connectionString = "YourTestConnectionStringHere";
            _provider = new SQLServerProvider(_connectionString);
            
            // Create test table
            CreateTestTable();
        }

        public void Dispose()
        {
            // Clean up test table
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand($"IF OBJECT_ID('{_testTable}', 'U') IS NOT NULL DROP TABLE {_testTable}", connection);
            command.ExecuteNonQuery();
        }

        private void CreateTestTable()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var createTableSql = $
                $"""
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{_testTable}')
                BEGIN
                    CREATE TABLE {_testTable} (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        Value INT NOT NULL,
                        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
                    )
                END
                """;
            
            using var command = new SqlCommand(createTableSql, connection);
            command.ExecuteNonQuery();
        }

        private DataTable CreateTestData(int count)
        {
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Value", typeof(int));

            for (int i = 0; i < count; i++)
            {
                table.Rows.Add($"Test-{i}", i);
            }

            return table;
        }

        [Fact]
        public async Task ExecuteBulkInsertAsync_WithDataTable_ShouldInsertAllRows()
        {
            // Arrange
            var testData = CreateTestData(100);

            // Act
            await _provider.ExecuteBulkInsertAsync(testData, _testTable);

            // Assert
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand($"SELECT COUNT(*) FROM {_testTable}", connection);
            var count = (int)await command.ExecuteScalarAsync();
            
            Assert.Equal(100, count);
        }

        [Fact]
        public async Task ExecuteBulkInsertAsync_WithConfiguration_ShouldUseConfiguration()
        {
            // Arrange
            var testData = CreateTestData(150);
            var config = new BulkCopyConfiguration
            {
                DestinationTableName = _testTable,
                BatchSize = 50,
                BulkCopyTimeout = 60
            };

            // Act
            await _provider.ExecuteBulkInsertAsync(testData, config);

            // Assert
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand($"SELECT COUNT(*) FROM {_testTable}", connection);
            var count = (int)await command.ExecuteScalarAsync();
            
            Assert.Equal(150, count);
        }

        [Fact]
        public async Task ExecuteBulkInsertAsync_WithEntities_ShouldInsertAllEntities()
        {
            // Arrange
            var entities = new List<TestEntity>();
            for (int i = 0; i < 75; i++)
            {
                entities.Add(new TestEntity { Name = $"Entity-{i}", Value = i });
            }

            // Act
            await _provider.ExecuteBulkInsertAsync(entities, builder => 
                builder.WithTable(_testTable)
                       .WithBatchSize(25));

            // Assert
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand($"SELECT COUNT(*) FROM {_testTable}", connection);
            var count = (int)await command.ExecuteScalarAsync();
            
            Assert.Equal(75, count);
        }

        [Fact]
        public void ExecuteBulkInsert_Sync_ShouldInsertAllRows()
        {
            // Arrange
            var testData = CreateTestData(50);

            // Act
            _provider.ExecuteBulkInsert(testData, _testTable);

            // Assert
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand($"SELECT COUNT(*) FROM {_testTable}", connection);
            var count = (int)command.ExecuteScalar();
            
            Assert.Equal(50, count);
        }

        private class TestEntity
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
    }
}
