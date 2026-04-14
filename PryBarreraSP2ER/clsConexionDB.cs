using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace pryBarreraBaseDeDatos
{
    internal class clsConexionDB : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _dbPath;
        private OleDbConnection _connection;

        public clsConexionDB(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("Ruta de base de datos no puede ser nula o vacía.", nameof(databasePath));
            }

            if (!File.Exists(databasePath))
            {
                throw new FileNotFoundException("No se encontró el archivo de base de datos.", databasePath);
            }

            _dbPath = databasePath;
            _connectionString = BuildConnectionString(databasePath);
        }

        private static string BuildConnectionString(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            switch (ext)
            {
                case ".accdb":
                    return $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Persist Security Info=False;";
                case ".mdb":
                    return $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={path};Persist Security Info=False;";
                default:
                    throw new NotSupportedException($"Extensión de archivo no soportada: {ext}");
            }
        }

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

        public string GetConnectionString()
        {
            return _connectionString;
        }

        public static List<string> GetDatabaseFiles(string folderPath)
        {
            var list = new List<string>();
            if (!Directory.Exists(folderPath)) return list;

            var files = Directory.GetFiles(folderPath, "*.accdb");
            files = ConcatArrays(files, Directory.GetFiles(folderPath, "*.mdb"));

            foreach (var f in files)
            {
                list.Add(Path.GetFileName(f));
            }

            return list;
        }

        private static T[] ConcatArrays<T>(T[] a, T[] b)
        {
            var result = new T[a.Length + b.Length];
            Array.Copy(a, 0, result, 0, a.Length);
            Array.Copy(b, 0, result, a.Length, b.Length);
            return result;
        }

        public List<string> GetTableNames()
        {
            var tables = new List<string>();

            // Obtener esquema de tablas de usuario
            var schema = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            if (schema == null) return tables;

            foreach (DataRow row in schema.Rows)
            {
                var tableType = row["TABLE_TYPE"] as string;
                if (!string.Equals(tableType, "TABLE", StringComparison.OrdinalIgnoreCase)) continue;

                var tableName = row["TABLE_NAME"] as string;
                // Ignorar tablas del sistema (por ejemplo: MSys*)
                if (string.IsNullOrWhiteSpace(tableName)) continue;
                if (tableName.StartsWith("MSys", StringComparison.OrdinalIgnoreCase)) continue;

                tables.Add(tableName);
            }

            return tables;
        }

        public DataTable ExecuteQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("SQL no puede ser nulo o vacío.", nameof(sql));

            var dt = new DataTable();
            using (var adapter = new OleDbDataAdapter(sql, Connection))
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
                    {
                        _connection.Close();
                    }
                }
                catch
                {
                }
                finally
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }
    }
}
