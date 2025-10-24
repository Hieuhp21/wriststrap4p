using Microsoft.Data.SqlClient;
using System.Data;

namespace WEB_SHOW_WRIST_STRAP.Configs
{
    public interface IDbConnectionFactory
    {
        SqlConnection CreateConnection();  // Đổi thành SqlConnection (không phải IDbConnection)
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
