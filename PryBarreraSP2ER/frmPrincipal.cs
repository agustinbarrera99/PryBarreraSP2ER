using System;
using System.IO;
using System.Windows.Forms;

namespace PryBarreraSP2ER
{
    public partial class frmPrincipal : Form
    {
        public frmPrincipal()
        {
            InitializeComponent();
        }

        private void BtnIniciarMigracion_Click(object sender, EventArgs e)
        {
            // ── El usuario elige dónde guardar la BD ─────────────
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Guardar base de datos";
            dialogo.Filter = "Base de datos Access (*.accdb)|*.accdb";
            dialogo.FileName = "Distribuidora";
            dialogo.DefaultExt = "accdb";

            if (dialogo.ShowDialog() != DialogResult.OK)
                return; // el usuario canceló

            string rutaDB = dialogo.FileName;

            // ─────────────────────────────────────────────────────
            txtLog.Clear();
            btnIniciarMigracion.Enabled = false;

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string rutaCategorias = Path.Combine(baseDir, "TextFiles", "Categorias.txt");
                string rutaArticulos = Path.Combine(baseDir, "TextFiles", "Articulos.txt");

                if (File.Exists(rutaDB))
                {
                    File.Delete(rutaDB);
                    MostrarLog("Base de datos anterior eliminada.");
                }

                CrearBaseDatosAccdb(rutaDB);
                MostrarLog($"Base de datos creada en: {rutaDB}");

                string connStr = clsConexionDB.BuildConnectionString(rutaDB);

                using (clsConexionDB conexion = new clsConexionDB(connStr))
                {
                    clsMigracion migracion = new clsMigracion(conexion);

                    migracion.CrearEstructuraBaseDatos();
                    MostrarLog(migracion.Log);
                    migracion.LimpiarLog();

                    migracion.MigrarCategorias(rutaCategorias);
                    MostrarLog(migracion.Log);
                    migracion.LimpiarLog();

                    migracion.MigrarArticulos(rutaArticulos);
                    MostrarLog(migracion.Log);
                }

                MostrarLog("Migración finalizada.");
            }
            catch (Exception ex)
            {
                MostrarLog($"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnIniciarMigracion.Enabled = true;
            }
        }

        private void CrearBaseDatosAccdb(string rutaDB)
        {
            string carpeta = Path.GetDirectoryName(rutaDB);
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            Type catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
            object catalogo = Activator.CreateInstance(catalogType);

            string connStr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={rutaDB};";
            catalogType.InvokeMember(
                "Create",
                System.Reflection.BindingFlags.InvokeMethod,
                null,
                catalogo,
                new object[] { connStr }
            );

            System.Runtime.InteropServices.Marshal.ReleaseComObject(catalogo);
        }

        private void MostrarLog(string mensaje)
        {
            txtLog.AppendText(mensaje + Environment.NewLine);
            txtLog.ScrollToCaret();
        }
    }
}