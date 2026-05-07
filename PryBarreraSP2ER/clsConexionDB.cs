using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace PryBarreraSP2ER
{
    internal class clsConexionDB : IDisposable
    {
        private readonly string _connectionString;
        private OleDbConnection _connection;

        public clsConexionDB(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("La cadena de conexión no puede ser nula o vacía.", nameof(connectionString));

            _connectionString = connectionString;
        }

        public static string BuildConnectionString(string dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
                throw new ArgumentException("La ruta no puede ser nula o vacía.", nameof(dbPath));

            string ext = System.IO.Path.GetExtension(dbPath).ToLowerInvariant();
            switch (ext)
            {
                case ".accdb":
                    return $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
                case ".mdb":
                    return $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={dbPath};Persist Security Info=False;";
                default:
                    throw new NotSupportedException($"Extensión de archivo no soportada: {ext}");
            }
        }

        public string GetConnectionString() => _connectionString;

        private OleDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new OleDbConnection(_connectionString);
                    _connection.Open();
                }
                return _connection;
            }
        }

        public DataTable ExecuteQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("El SQL no puede ser nulo o vacío.", nameof(sql));

            DataTable dt = new DataTable();
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(sql, Connection))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                try
                {
                    if (_connection.State != ConnectionState.Closed)
                        _connection.Close();
                }
                catch { }
                finally
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }
    }
}