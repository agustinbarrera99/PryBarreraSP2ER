using System;
using System.Data.OleDb;
using System.IO;
using System.Text;

namespace PryBarreraSP2ER
{
    internal class clsMigracion
    {
        private readonly clsConexionDB _conexion;
        private readonly StringBuilder _log;

        public string Log => _log.ToString();

        public clsMigracion(clsConexionDB conexion)
        {
            _conexion = conexion ?? throw new ArgumentNullException(nameof(conexion));
            _log = new StringBuilder();
        }

        public void LimpiarLog()
        {
            _log.Clear();
        }

        public bool CrearEstructuraBaseDatos()
        {
            try
            {
                string connStr = _conexion.GetConnectionString();
                using (OleDbConnection conn = new OleDbConnection(connStr))
                {
                    conn.Open();

                    string sqlCategorias = @"
                        CREATE TABLE Categorias (
                            IdCategoria INTEGER NOT NULL PRIMARY KEY,
                            Nombre      VARCHAR(100) NOT NULL
                        )";

                    string sqlArticulos = @"
                        CREATE TABLE Articulos (
                            IdArticulo  INTEGER NOT NULL PRIMARY KEY,
                            Nombre      VARCHAR(150) NOT NULL,
                            IdCategoria INTEGER NOT NULL,
                            Precio      DOUBLE NOT NULL
                        )";

                    EjecutarDDL(conn, sqlCategorias);
                    _log.AppendLine("  → Tabla 'Categorias' creada.");

                    EjecutarDDL(conn, sqlArticulos);
                    _log.AppendLine("  → Tabla 'Articulos' creada.");
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.AppendLine($"  Error creando estructura: {ex.Message}");
                return false;
            }
        }

        public void MigrarCategorias(string rutaArchivo)
        {
            _log.AppendLine("Migrando datos de Categorías...");

            if (!File.Exists(rutaArchivo))
            {
                _log.AppendLine($"  Archivo no encontrado: {rutaArchivo}");
                return;
            }

            int insertados = 0;
            int errores = 0;
            string connStr = _conexion.GetConnectionString();

            using (OleDbConnection conn = new OleDbConnection(connStr))
            {
                conn.Open();

                foreach (string linea in File.ReadLines(rutaArchivo, Encoding.Default))
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;

                    string[] partes = linea.Split('\t');
                    if (partes.Length < 2)
                    {
                        _log.AppendLine($"  Línea con formato incorrecto: '{linea}'");
                        errores++;
                        continue;
                    }

                    if (!int.TryParse(partes[0].Trim(), out int idCategoria))
                    {
                        _log.AppendLine($"  IdCategoria inválido en línea: '{linea}'");
                        errores++;
                        continue;
                    }

                    string nombre = partes[1].Trim();

                    try
                    {
                        string sql = "INSERT INTO Categorias (IdCategoria, Nombre) VALUES (@id, @nombre)";
                        using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", idCategoria);
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.ExecuteNonQuery();
                            insertados++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.AppendLine($"  Error insertando categoría {idCategoria}: {ex.Message}");
                        errores++;
                    }
                }
            }

            _log.AppendLine($"Se incorporaron {insertados} registros nuevos.");
            if (errores > 0)
                _log.AppendLine($"  {errores} línea(s) con errores.");
        }

        public void MigrarArticulos(string rutaArchivo)
        {
            _log.AppendLine("Migrando datos de Artículos...");

            if (!File.Exists(rutaArchivo))
            {
                _log.AppendLine($"  Archivo no encontrado: {rutaArchivo}");
                return;
            }

            int insertados = 0;
            int errores = 0;
            string connStr = _conexion.GetConnectionString();

            using (OleDbConnection conn = new OleDbConnection(connStr))
            {
                conn.Open();

                foreach (string linea in File.ReadLines(rutaArchivo, Encoding.Default))
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;

                    string[] partes = linea.Split('\t');
                    if (partes.Length < 4)
                    {
                        _log.AppendLine($"  Línea con formato incorrecto: '{linea}'");
                        errores++;
                        continue;
                    }

                    if (!int.TryParse(partes[0].Trim(), out int idArticulo))
                    {
                        _log.AppendLine($"  IdArticulo inválido: '{linea}'");
                        errores++;
                        continue;
                    }

                    string nombre = partes[1].Trim();

                    if (!int.TryParse(partes[2].Trim(), out int idCategoria))
                    {
                        _log.AppendLine($"  IdCategoria inválido en artículo {idArticulo}");
                        errores++;
                        continue;
                    }

                    string precioStr = partes[3].Trim().Replace(',', '.');
                    if (!double.TryParse(precioStr,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out double precio))
                    {
                        _log.AppendLine($"  Precio inválido en artículo {idArticulo}: '{partes[3]}'");
                        errores++;
                        continue;
                    }

                    try
                    {
                        string sql = "INSERT INTO Articulos (IdArticulo, Nombre, IdCategoria, Precio) VALUES (@id, @nombre, @idCat, @precio)";
                        using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", idArticulo);
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.Parameters.AddWithValue("@idCat", idCategoria);
                            cmd.Parameters.AddWithValue("@precio", precio);
                            cmd.ExecuteNonQuery();
                            insertados++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.AppendLine($"  Error insertando artículo {idArticulo}: {ex.Message}");
                        errores++;
                    }
                }
            }

            _log.AppendLine($"Se incorporaron {insertados} registros nuevos.");
            if (errores > 0)
                _log.AppendLine($"  {errores} línea(s) con errores.");
        }

        private static void EjecutarDDL(OleDbConnection conn, string sql)
        {
            using (OleDbCommand cmd = new OleDbCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}