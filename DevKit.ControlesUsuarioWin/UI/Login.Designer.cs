namespace DevKit.ControlesUsuarioWin.UI
{
    partial class Login
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            chkPrincipal = new CheckBox();
            comboConexiones = new UserControlComboBoxEdit();
            btnNuevo = new Button();
            btnEliminar = new Button();
            btnAgregar = new Button();
            txtBaseDatos = new UserControlTextEdit();
            txtContraseña = new UserControlTextEdit();
            txtUsuario = new UserControlTextEdit();
            txtServidor = new UserControlTextEdit();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label5 = new Label();
            label1 = new Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)comboConexiones.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtBaseDatos.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtContraseña.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtUsuario.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtServidor.Properties).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(chkPrincipal);
            panel1.Controls.Add(comboConexiones);
            panel1.Controls.Add(btnNuevo);
            panel1.Controls.Add(btnEliminar);
            panel1.Controls.Add(btnAgregar);
            panel1.Controls.Add(txtBaseDatos);
            panel1.Controls.Add(txtContraseña);
            panel1.Controls.Add(txtUsuario);
            panel1.Controls.Add(txtServidor);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(1);
            panel1.Name = "panel1";
            panel1.Size = new Size(805, 143);
            panel1.TabIndex = 0;
            // 
            // chkPrincipal
            // 
            chkPrincipal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkPrincipal.AutoSize = true;
            chkPrincipal.Font = new Font("Segoe UI Variable Display", 8F, FontStyle.Regular, GraphicsUnit.Point);
            chkPrincipal.Location = new Point(640, 7);
            chkPrincipal.Margin = new Padding(1);
            chkPrincipal.Name = "chkPrincipal";
            chkPrincipal.Size = new Size(68, 19);
            chkPrincipal.TabIndex = 7;
            chkPrincipal.Text = "Principal";
            chkPrincipal.UseVisualStyleBackColor = true;
            // 
            // comboConexiones
            // 
            comboConexiones.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboConexiones.CharacterCasing = CharacterCasing.Normal;
            comboConexiones.EnterMoveNextControl = true;
            comboConexiones.IgnoreReadOnly = false;
            comboConexiones.Location = new Point(104, 4);
            comboConexiones.Margin = new Padding(4, 4, 4, 4);
            comboConexiones.Name = "comboConexiones";
            comboConexiones.Properties.Appearance.BackColor = Color.White;
            comboConexiones.Properties.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            comboConexiones.Properties.Appearance.Options.UseBackColor = true;
            comboConexiones.Properties.Appearance.Options.UseFont = true;
            comboConexiones.Properties.AppearanceDropDown.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            comboConexiones.Properties.AppearanceDropDown.Options.UseFont = true;
            
            comboConexiones.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            comboConexiones.Properties.Buttons.AddRange(new EditorButton[] { new EditorButton(ButtonPredefines.Combo) });
            comboConexiones.Properties.DropDownRows = 20;
            comboConexiones.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            comboConexiones.Size = new Size(511, 26);
            comboConexiones.TabIndex = 0;
            // 
            // btnNuevo
            // 
            btnNuevo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNuevo.Font = new Font("Segoe UI Variable Display", 8F, FontStyle.Regular, GraphicsUnit.Point);
            btnNuevo.Location = new Point(771, 3);
            btnNuevo.Margin = new Padding(1);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(29, 27);
            btnNuevo.TabIndex = 6;
            btnNuevo.UseVisualStyleBackColor = true;
            // 
            // btnEliminar
            // 
            btnEliminar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEliminar.Font = new Font("Segoe UI Variable Display", 8F, FontStyle.Regular, GraphicsUnit.Point);
            btnEliminar.Location = new Point(742, 3);
            btnEliminar.Margin = new Padding(1);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(29, 27);
            btnEliminar.TabIndex = 6;
            btnEliminar.Text = "-";
            btnEliminar.UseVisualStyleBackColor = true;
            // 
            // btnAgregar
            // 
            btnAgregar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAgregar.Font = new Font("Segoe UI Variable Display", 8F, FontStyle.Regular, GraphicsUnit.Point);
            btnAgregar.Location = new Point(713, 3);
            btnAgregar.Margin = new Padding(1);
            btnAgregar.Name = "btnAgregar";
            btnAgregar.Size = new Size(29, 27);
            btnAgregar.TabIndex = 5;
            btnAgregar.Text = "+";
            btnAgregar.UseVisualStyleBackColor = true;
            // 
            // txtBaseDatos
            // 
            txtBaseDatos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBaseDatos.CharacterCasing = CharacterCasing.Normal;
            txtBaseDatos.EnterMoveNextControl = true;
            txtBaseDatos.IgnoreReadOnly = false;
            txtBaseDatos.Location = new Point(102, 114);
            txtBaseDatos.Margin = new Padding(1);
            txtBaseDatos.Name = "txtBaseDatos";
            txtBaseDatos.Properties.Appearance.BackColor = Color.White;
            txtBaseDatos.Properties.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            txtBaseDatos.Properties.Appearance.Options.UseBackColor = true;
            txtBaseDatos.Properties.Appearance.Options.UseFont = true;

            txtBaseDatos.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            txtBaseDatos.Properties.ValidateOnEnterKey = true;
            txtBaseDatos.Size = new Size(698, 26);
            txtBaseDatos.TabIndex = 4;
            // 
            // txtContraseña
            // 
            txtContraseña.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtContraseña.CharacterCasing = CharacterCasing.Normal;
            txtContraseña.EnterMoveNextControl = true;
            txtContraseña.IgnoreReadOnly = false;
            txtContraseña.Location = new Point(102, 86);
            txtContraseña.Margin = new Padding(1);
            txtContraseña.Name = "txtContraseña";
            txtContraseña.Properties.Appearance.BackColor = Color.White;
            txtContraseña.Properties.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            txtContraseña.Properties.Appearance.Options.UseBackColor = true;
            txtContraseña.Properties.Appearance.Options.UseFont = true;

            txtContraseña.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            txtContraseña.Properties.ValidateOnEnterKey = true;
            txtContraseña.Size = new Size(698, 26);
            txtContraseña.TabIndex = 3;
            // 
            // txtUsuario
            // 
            txtUsuario.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtUsuario.CharacterCasing = CharacterCasing.Normal;
            txtUsuario.EnterMoveNextControl = true;
            txtUsuario.IgnoreReadOnly = false;
            txtUsuario.Location = new Point(104, 58);
            txtUsuario.Margin = new Padding(1);
            txtUsuario.Name = "txtUsuario";
            txtUsuario.Properties.Appearance.BackColor = Color.White;
            txtUsuario.Properties.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            txtUsuario.Properties.Appearance.Options.UseBackColor = true;
            txtUsuario.Properties.Appearance.Options.UseFont = true;

            txtUsuario.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            txtUsuario.Properties.ValidateOnEnterKey = true;
            txtUsuario.Size = new Size(698, 26);
            txtUsuario.TabIndex = 2;
            // 
            // txtServidor
            // 
            txtServidor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtServidor.CharacterCasing = CharacterCasing.Normal;
            txtServidor.EnterMoveNextControl = true;
            txtServidor.IgnoreReadOnly = false;
            txtServidor.Location = new Point(104, 30);
            txtServidor.Margin = new Padding(1);
            txtServidor.Name = "txtServidor";
            txtServidor.Properties.Appearance.BackColor = Color.White;
            txtServidor.Properties.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            txtServidor.Properties.Appearance.Options.UseBackColor = true;
            txtServidor.Properties.Appearance.Options.UseFont = true;

            txtServidor.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            txtServidor.Properties.ValidateOnEnterKey = true;
            txtServidor.Size = new Size(698, 26);
            txtServidor.TabIndex = 1;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Variable Display", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            label4.Location = new Point(21, 118);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(71, 17);
            label4.TabIndex = 0;
            label4.Text = "Base Datos";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Variable Display", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            label3.Location = new Point(18, 90);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(74, 17);
            label3.TabIndex = 0;
            label3.Text = "Contraseña";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Variable Display", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            label2.Location = new Point(40, 62);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(52, 17);
            label2.TabIndex = 0;
            label2.Text = "Usuario";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Variable Display", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            label5.Location = new Point(16, 9);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(76, 17);
            label5.TabIndex = 0;
            label5.Text = "Conexiones";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Variable Display", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(35, 34);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(57, 17);
            label1.TabIndex = 0;
            label1.Text = "Servidor";
            // 
            // Login
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel1);
            Margin = new Padding(4, 4, 4, 4);
            Name = "Login";
            Size = new Size(805, 143);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)comboConexiones.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtBaseDatos.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtContraseña.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtUsuario.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtServidor.Properties).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private UserControlTextEdit txtBaseDatos;
        private UserControlTextEdit txtContraseña;
        private UserControlTextEdit txtUsuario;
        private UserControlTextEdit txtServidor;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private Label label5;
        private Button btnEliminar;
        private Button btnAgregar;
        private UserControlComboBoxEdit comboConexiones;
        private CheckBox chkPrincipal;
        private Button btnNuevo;
    }
}
