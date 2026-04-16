using pryBarreraBaseDeDatos;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        // ─────────────────────────────────────────────
        // Crear estructura de tablas en la BD
        // ─────────────────────────────────────────────
        public bool CrearEstructuraBaseDatos()
        {
            try
            {
                string connStr = _conexion.GetConnectionString();

                using (var conn = new OleDbConnection(connStr))
                {
                    conn.Open();

                    // Tabla Categorias
                    string sqlCategorias = @"
                        CREATE TABLE Categorias (
                            IdCategoria INTEGER NOT NULL PRIMARY KEY,
                            Nombre      VARCHAR(100) NOT NULL
                        )";

                    // Tabla Articulos
                    string sqlArticulos = @"
                        CREATE TABLE Articulos (
                            IdArticulo  INTEGER NOT NULL PRIMARY KEY,
                            Nombre      VARCHAR(150) NOT NULL,
                            IdCategoria INTEGER NOT NULL,
                            Precio      DOUBLE NOT NULL,
                            CONSTRAINT FK_Articulos_Categorias
                                FOREIGN KEY (IdCategoria) REFERENCES Categorias(IdCategoria)
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
                _log.AppendLine($"✗ Error creando estructura: {ex.Message}");
                return false;
            }
        }

        // ─────────────────────────────────────────────
        // Migrar Categorias.txt → tabla Categorias
        // ─────────────────────────────────────────────
        public void MigrarCategorias(string rutaArchivo)
        {
            _log.AppendLine("Migrando datos de Categorías...");

            if (!File.Exists(rutaArchivo))
            {
                _log.AppendLine($"✗ Archivo no encontrado: {rutaArchivo}");
                return;
            }

            int insertados = 0;
            int errores = 0;

            string connStr = _conexion.GetConnectionString();

            using (var conn = new OleDbConnection(connStr))
            {
                conn.Open();

                foreach (string linea in File.ReadLines(rutaArchivo, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;

                    // Formato esperado: IdCategoria,Nombre
                    string[] partes = linea.Split(',');
                    if (partes.Length < 2)
                    {
                        _log.AppendLine($"  ⚠ Línea con formato incorrecto: '{linea}'");
                        errores++;
                        continue;
                    }

                    if (!int.TryParse(partes[0].Trim(), out int idCategoria))
                    {
                        _log.AppendLine($"  ⚠ IdCategoria inválido en línea: '{linea}'");
                        errores++;
                        continue;
                    }

                    string nombre = partes[1].Trim();

                    try
                    {
                        string sql = "INSERT INTO Categorias (IdCategoria, Nombre) VALUES (@id, @nombre)";
                        using (var cmd = new OleDbCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", idCategoria);
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.ExecuteNonQuery();
                            insertados++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.AppendLine($"  ✗ Error insertando categoría {idCategoria}: {ex.Message}");
                        errores++;
                    }
                }
            }

            _log.AppendLine($"Se incorporaron {insertados} registros nuevos.");
            if (errores > 0)
                _log.AppendLine($"  ⚠ {errores} línea(s) con errores.");
        }

        // ─────────────────────────────────────────────
        // Migrar Articulos.txt → tabla Articulos
        // ─────────────────────────────────────────────
        public void MigrarArticulos(string rutaArchivo)
        {
            _log.AppendLine("Migrando datos de Artículos...");

            if (!File.Exists(rutaArchivo))
            {
                _log.AppendLine($"✗ Archivo no encontrado: {rutaArchivo}");
                return;
            }

            int insertados = 0;
            int errores = 0;

            string connStr = _conexion.GetConnectionString();

            using (var conn = new OleDbConnection(connStr))
            {
                conn.Open();

                foreach (string linea in File.ReadLines(rutaArchivo, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;

                    // Formato esperado: IdArticulo,Nombre,IdCategoria,Precio
                    string[] partes = linea.Split(',');
                    if (partes.Length < 4)
                    {
                        _log.AppendLine($"  ⚠ Línea con formato incorrecto: '{linea}'");
                        errores++;
                        continue;
                    }

                    if (!int.TryParse(partes[0].Trim(), out int idArticulo))
                    {
                        _log.AppendLine($"  ⚠ IdArticulo inválido: '{linea}'");
                        errores++;
                        continue;
                    }

                    // El nombre puede contener comas; todo lo que está entre la 2da y
                    // antepenúltima coma forma el nombre.
                    // Pero para el formato simple de 4 columnas:
                    string nombre = partes[1].Trim();

                    if (!int.TryParse(partes[2].Trim(), out int idCategoria))
                    {
                        _log.AppendLine($"  ⚠ IdCategoria inválido en artículo {idArticulo}");
                        errores++;
                        continue;
                    }

                    // El precio puede usar '.' o ',' como separador decimal
                    string precioStr = partes[3].Trim().Replace(',', '.');
                    if (!double.TryParse(precioStr,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out double precio))
                    {
                        _log.AppendLine($"  ⚠ Precio inválido en artículo {idArticulo}: '{partes[3]}'");
                        errores++;
                        continue;
                    }

                    try
                    {
                        string sql = @"INSERT INTO Articulos (IdArticulo, Nombre, IdCategoria, Precio)
                                       VALUES (@id, @nombre, @idCat, @precio)";
                        using (var cmd = new OleDbCommand(sql, conn))
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
                        _log.AppendLine($"  ✗ Error insertando artículo {idArticulo}: {ex.Message}");
                        errores++;
                    }
                }
            }

            _log.AppendLine($"Se incorporaron {insertados} registros nuevos.");
            if (errores > 0)
                _log.AppendLine($"  ⚠ {errores} línea(s) con errores.");
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────
        private static void EjecutarDDL(OleDbConnection conn, string sql)
        {
            using (var cmd = new OleDbCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}