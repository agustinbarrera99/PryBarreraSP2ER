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
                MostrarLog("Creando base de datos Distribuidora.accdb...");

                // Eliminar base de datos existente para empezar limpio
                if (File.Exists(rutaBaseDatos))
                {
                    MostrarLog("Eliminando base de datos anterior...");
                    try
                    {
                        File.Delete(rutaBaseDatos);
                        System.Threading.Thread.Sleep(500); // Esperar a que se libere el archivo
                        MostrarLog("? Base de datos anterior eliminada.");
                    }
                    catch (Exception ex)
                    {
                        MostrarLog($"? Advertencia al eliminar BD anterior: {ex.Message}");
                    }
                }

                // Crear base de datos nueva
                MostrarLog("Creando base de datos Distribuidora.mdb...");
                CrearBaseDatos(rutaBaseDatos);

                _conexion = new clsConexionDB(rutaBaseDatos);
                _migracion = new clsMigracion(_conexion);

                // Crear estructura
                MostrarLog("Creando estructura de tablas...");
                if (_migracion.CrearEstructuraBaseDatos())
                {
                    MostrarLog("? Estructura de tablas creada exitosamente.");
                }
                else
                {
                    MostrarLog("? Error al crear estructura.");
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
                MostrarLog($"? Error: {ex.Message}");
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
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string templateName = "Distribuidora_Template.accdb";
                string templatePath = Path.Combine(baseDir, templateName);

                MostrarLog($"Plantilla esperada: {templatePath}");

                // Si no existe exactamente en la ruta esperada, buscar en el directorio de salida (subcarpetas incl.)
                if (!File.Exists(templatePath))
                {
                    MostrarLog("No encontrada en la ruta exacta. Buscando coincidencias en la carpeta de salida...");

                    var candidatos = Directory.GetFiles(baseDir, "Distribuidora_Template.*", SearchOption.AllDirectories);
                    if (candidatos.Length == 0)
                    {
                        candidatos = Directory.GetFiles(baseDir, "Distribuidora*.*", SearchOption.AllDirectories);
                    }

                    if (candidatos.Length == 0)
                    {
                        // Listar archivos para diagnóstico
                        var archivos = Directory.GetFiles(baseDir, "*", SearchOption.TopDirectoryOnly);
                        var listado = archivos.Length == 0 ? "(vacío)" : string.Join(", ", Array.ConvertAll(archivos, Path.GetFileName));
                        MostrarLog("Archivos en carpeta de salida: " + listado);

                        throw new FileNotFoundException("No se encontró la plantilla de BD en la carpeta de salida.", templatePath);
                    }

                    templatePath = candidatos[0];
                    MostrarLog("Plantilla encontrada en: " + templatePath);
                }

                File.Copy(templatePath, rutaBaseDatos, overwrite: true);
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
