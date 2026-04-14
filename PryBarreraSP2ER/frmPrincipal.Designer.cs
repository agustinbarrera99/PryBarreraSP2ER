namespace PryBarreraSP2ER
{
    partial class frmPrincipal
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnIniciarMigracion = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            
            // btnIniciarMigracion
            this.btnIniciarMigracion.Location = new System.Drawing.Point(560, 15);
            this.btnIniciarMigracion.Name = "btnIniciarMigracion";
            this.btnIniciarMigracion.Size = new System.Drawing.Size(79, 23);
            this.btnIniciarMigracion.TabIndex = 0;
            this.btnIniciarMigracion.Text = "Iniciar Migración";
            this.btnIniciarMigracion.UseVisualStyleBackColor = true;
            this.btnIniciarMigracion.Click += new System.EventHandler(this.BtnIniciarMigracion_Click);
            
            // txtLog
            this.txtLog.Location = new System.Drawing.Point(12, 43);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(627, 395);
            this.txtLog.TabIndex = 1;
            
            // lblTitulo
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Location = new System.Drawing.Point(12, 20);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(66, 13);
            this.lblTitulo.TabIndex = 2;
            this.lblTitulo.Text = "Información:";
            
            // frmPrincipal
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(655, 450);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnIniciarMigracion);
            this.Name = "frmPrincipal";
            this.Text = "Migración de Datos - GoodHard";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnIniciarMigracion;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblTitulo;
    }
}
