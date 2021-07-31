using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace PRS.Database
{
    internal class PrsDatabase : IDatabase
    {
        //private readonly string _connectionString;

        public PrsDatabase()
        {
          //  _connectionString = connectionString;
        }

        public async Task<IEnumerable<ColumnModel>> GetColumnModelsAsync(string connectionString)
        {
            using SqlConnection connection = new();
            connection.ConnectionString = connectionString;
            await connection.OpenAsync().ConfigureAwait(false);
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS ORDER BY TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME";
            command.CommandType = CommandType.Text;
            using SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            List<ColumnModel> columns = new();

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                ColumnModel m = new();
                m.TableSchema = reader[0].ToString();
                m.TableName = reader[1].ToString();
                m.ColumnName = reader[2].ToString();
                m.OrdinalPosition = reader[3].ToString();
                m.ColumnDefault = reader[4].ToString();
                m.IsNullable = reader[5].ToString();
                m.DataType = reader[6].ToString();
                m.CharacterMaximumLength = reader[7].ToString();
                columns.Add(m);
            }

            reader.Close();
            connection.Close();

            return columns;
        }
 
        public async Task<IEnumerable<string>> GetStoredProcedureNamesAsync(string connectionString)
        {
            using SqlConnection connection = new();
            connection.ConnectionString = connectionString;
            await connection.OpenAsync().ConfigureAwait(false);
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Name FROM SYSOBJECTS WHERE type = 'P' AND category = 0 ORDER BY name";
            command.CommandType = CommandType.Text;
            using SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            List<string> names = new();

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                names.Add(reader[0].ToString());
            }

            reader.Close();
            connection.Close();

            return names;
        }

        public async Task<IEnumerable<TableModel>> GetTableModelsAsync(string connectionString)
        {
            using SqlConnection connection = new();
            connection.ConnectionString = connectionString;
            await connection.OpenAsync().ConfigureAwait(false);
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME";
            command.CommandType = CommandType.Text;
            using SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            List<TableModel> tables = new();

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                TableModel m = new();
                m.TableSchema = reader[0].ToString();
                m.TableName = reader[1].ToString();
                m.TableType = reader[2].ToString();
                tables.Add(m);
            }

            reader.Close();
            connection.Close();

            return tables;
        }
    }
}
