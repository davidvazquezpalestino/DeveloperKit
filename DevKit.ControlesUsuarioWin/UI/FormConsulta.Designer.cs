using System.ComponentModel;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;

namespace DevKit.ControlesUsuarioWin.UI
{
    partial class FormConsulta
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            GridConsulta = new GridControl();
            GridViewConsulta = new GridView();
            layoutControl1 = new LayoutControl();
            txtFiltro = new UserControlTextEdit();
            Root = new LayoutControlGroup();
            layoutControlItem1 = new LayoutControlItem();
            layoutControlItem2 = new LayoutControlItem();
            ((ISupportInitialize)GridConsulta).BeginInit();
            ((ISupportInitialize)GridViewConsulta).BeginInit();
            ((ISupportInitialize)layoutControl1).BeginInit();
            layoutControl1.SuspendLayout();
            ((ISupportInitialize)txtFiltro.Properties).BeginInit();
            ((ISupportInitialize)Root).BeginInit();
            ((ISupportInitialize)layoutControlItem1).BeginInit();
            ((ISupportInitialize)layoutControlItem2).BeginInit();
            SuspendLayout();
            // 
            // GridConsulta
            // 
            GridConsulta.EmbeddedNavigator.Margin = new Padding(2);
            GridConsulta.Location = new Point(3, 31);
            GridConsulta.MainView = GridViewConsulta;
            GridConsulta.Name = "GridConsulta";
            GridConsulta.Size = new Size(1078, 527);
            GridConsulta.TabIndex = 1;
            GridConsulta.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { GridViewConsulta });
            // 
            // GridViewConsulta
            // 
            GridViewConsulta.Appearance.Row.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point);
            GridViewConsulta.Appearance.Row.Options.UseFont = true;
            GridViewConsulta.DetailHeight = 372;
            GridViewConsulta.GridControl = GridConsulta;
            GridViewConsulta.Name = "GridViewConsulta";
            GridViewConsulta.OptionsBehavior.Editable = false;
            GridViewConsulta.OptionsFind.AllowFindPanel = false;
            GridViewConsulta.OptionsFind.ClearFindOnClose = false;
            GridViewConsulta.OptionsFind.FindDelay = 100;
            GridViewConsulta.OptionsFind.FindMode = FindMode.Always;
            GridViewConsulta.OptionsSelection.ResetSelectionClickOutsideCheckboxSelector = true;
            GridViewConsulta.OptionsView.HeaderFilterButtonShowMode = FilterButtonShowMode.SmartTag;
            GridViewConsulta.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            GridViewConsulta.OptionsView.ShowFooter = true;
            GridViewConsulta.OptionsView.ShowGroupPanel = false;
            GridViewConsulta.OptionsView.ShowIndicator = false;
            // 
            // layoutControl1
            // 
            layoutControl1.Controls.Add(txtFiltro);
            layoutControl1.Controls.Add(GridConsulta);
            layoutControl1.Dock = DockStyle.Fill;
            layoutControl1.Location = new Point(0, 0);
            layoutControl1.Name = "layoutControl1";
            layoutControl1.Root = Root;
            layoutControl1.Size = new Size(1084, 561);
            layoutControl1.TabIndex = 7;
            layoutControl1.Text = "layoutControl1";
            // 
            // txtFiltro
            // 
            txtFiltro.CharacterCasing = CharacterCasing.Upper;
            txtFiltro.EnterMoveNextControl = true;
            txtFiltro.IgnoreReadOnly = false;
            txtFiltro.Location = new Point(51, 3);
            txtFiltro.Name = "txtFiltro";
            txtFiltro.Properties.Appearance.BackColor = Color.White;
            txtFiltro.Properties.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            txtFiltro.Properties.Appearance.Options.UseBackColor = true;
            txtFiltro.Properties.Appearance.Options.UseFont = true;
            txtFiltro.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            txtFiltro.Properties.CharacterCasing = CharacterCasing.Upper;
            txtFiltro.Properties.ValidateOnEnterKey = true;
            txtFiltro.Size = new Size(1030, 24);
            txtFiltro.StyleController = layoutControl1;
            txtFiltro.TabIndex = 4;
            // 
            // Root
            // 
            Root.EnableIndentsWithoutBorders = DefaultBoolean.True;
            Root.GroupBordersVisible = false;
            Root.Items.AddRange(new BaseLayoutItem[] { layoutControlItem1, layoutControlItem2 });
            Root.Name = "Root";
            Root.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1, 1, 1);
            Root.Size = new Size(1084, 561);
            Root.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            layoutControlItem1.Control = GridConsulta;
            layoutControlItem1.Location = new Point(0, 28);
            layoutControlItem1.Name = "layoutControlItem1";
            layoutControlItem1.Size = new Size(1082, 531);
            layoutControlItem1.TextSize = new Size(0, 0);
            layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            layoutControlItem2.AppearanceItemCaption.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point);
            layoutControlItem2.AppearanceItemCaption.Options.UseFont = true;
            layoutControlItem2.Control = txtFiltro;
            layoutControlItem2.Location = new Point(0, 0);
            layoutControlItem2.Name = "layoutControlItem2";
            layoutControlItem2.Size = new Size(1082, 28);
            layoutControlItem2.Text = "Buscar";
            layoutControlItem2.TextSize = new Size(36, 16);
            // 
            // FormConsulta
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1084, 561);
            Controls.Add(layoutControl1);
            Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point);
            KeyPreview = true;
            Name = "FormConsulta";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Consultas";
            WindowState = FormWindowState.Maximized;
            FormClosed += FormConsulta_FormClosed;
            ((ISupportInitialize)GridConsulta).EndInit();
            ((ISupportInitialize)GridViewConsulta).EndInit();
            ((ISupportInitialize)layoutControl1).EndInit();
            layoutControl1.ResumeLayout(false);
            ((ISupportInitialize)txtFiltro.Properties).EndInit();
            ((ISupportInitialize)Root).EndInit();
            ((ISupportInitialize)layoutControlItem1).EndInit();
            ((ISupportInitialize)layoutControlItem2).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private GridControl GridConsulta;
        private GridView GridViewConsulta;
        private LayoutControl layoutControl1;
        private LayoutControlGroup Root;
        private LayoutControlItem layoutControlItem1;
        private UserControlTextEdit txtFiltro;
        private LayoutControlItem layoutControlItem2;
    }
}