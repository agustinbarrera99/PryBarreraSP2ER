using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using pryBarreraBaseDeDatos;

namespace PryBarreraSP2ER
{
    public partial class frmPrincipal : Form
    {
        private clsConexionDB _conexion;
        private clsMigracion _migracion;

        public frmPrincipal()
        {
            InitializeComponent();
        }

        private void BtnIniciarMigracion_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
            btnIniciarMigracion.Enabled = false;

            try
            {
                string rutaBaseDatos = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Distribuidora.accdb");
                string rutaCategorias = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categorias.txt");
                string rutaArticulos = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Articulos.txt");

                MostrarLog("========== INICIO DE MIGRACIÓN ==========");
                MostrarLog($"Ruta Base de Datos: {rutaBaseDatos}");
                MostrarLog($"Ruta Categorías: {rutaCategorias}");
                MostrarLog($"Ruta Artículos: {rutaArticulos}");
                MostrarLog("");

                // Eliminar base de datos existente para empezar limpio
                if (File.Exists(rutaBaseDatos))
                {
                    MostrarLog("Eliminando base de datos anterior...");
                    try
                    {
                        File.Delete(rutaBaseDatos);
                        System.Threading.Thread.Sleep(500);
                        MostrarLog("✓ Base de datos anterior eliminada.");
                    }
                    catch (Exception ex)
                    {
                        MostrarLog($"⚠ Advertencia al eliminar BD anterior: {ex.Message}");
                    }
                }

                // Crear base de datos nueva
                MostrarLog("Creando base de datos Distribuidora.accdb...");
                CrearBaseDatos(rutaBaseDatos);

                _conexion = new clsConexionDB(rutaBaseDatos);
                _migracion = new clsMigracion(_conexion);

                // Crear estructura
                MostrarLog("Creando estructura de tablas...");
                if (_migracion.CrearEstructuraBaseDatos())
                {
                    MostrarLog("✓ Estructura de tablas creada exitosamente.");
                }
                else
                {
                    MostrarLog("✗ Error al crear estructura.");
                }
                MostrarLog(_migracion.Log);

                // Migrar categorías
                MostrarLog("Migrando categorías...");
                _migracion = new clsMigracion(_conexion);
                _migracion.MigrarCategorias(rutaCategorias);
                MostrarLog(_migracion.Log);

                // Migrar artículos
                MostrarLog("Migrando artículos...");
                _migracion = new clsMigracion(_conexion);
                _migracion.MigrarArticulos(rutaArticulos);
                MostrarLog(_migracion.Log);

                MostrarLog("========== MIGRACIÓN COMPLETADA ==========");
            }
            catch (Exception ex)
            {
                MostrarLog($"✗ Error: {ex.Message}");
                MessageBox.Show($"Error durante la migración:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _conexion?.Dispose();
                btnIniciarMigracion.Enabled = true;
            }
        }

        private void MostrarLog(string mensaje)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => MostrarLog(mensaje)));
                return;
            }

            txtLog.AppendText(mensaje + Environment.NewLine);
            txtLog.ScrollToCaret();
        }

        private void CrearBaseDatos(string rutaBaseDatos)
        {
            try
            {
                // Crear archivo .accdb usando ADOX Catalog
                string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={rutaBaseDatos};";

                object[] oParams = new object[1];
                oParams[0] = connectionString;

                object oCatalog = Activator.CreateInstance(Type.GetTypeFromProgID("ADOX.Catalog"));
                oCatalog.GetType().InvokeMember("Create", System.Reflection.BindingFlags.InvokeMethod, null, oCatalog, oParams);

                MostrarLog("✓ Base de datos creada exitosamente.");
            }
            catch (Exception ex)
            {
                MostrarLog($"✗ Error creando base de datos: {ex.Message}");
                throw;
            }
        }
    }
}
