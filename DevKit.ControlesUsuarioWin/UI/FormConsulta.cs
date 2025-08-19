namespace DevKit.ControlesUsuarioWin.UI;

/// <summary>
/// Formulario de consulta que muestra datos en una cuadrícula con capacidades de filtrado.
/// Permite seleccionar un registro de la cuadrícula para su posterior procesamiento.
/// </summary>
public partial class FormConsulta : Form
{
    /// <summary>
    /// Obtiene o establece la información de la consulta seleccionada.
    /// </summary>
    private IInformacionConsulta InformacionConsulta { get; set; }
    /// <summary>
    /// Obtiene o establece la fila de datos seleccionada.
    /// </summary>
    private DataRow Row { get; set; }
    /// <summary>
    /// Obtiene o establece la tabla de datos que se muestra en la cuadrícula.
    /// </summary>
    private DataTable Tabla { get; set; }
    /// <summary>
    /// Obtiene o establece el filtro inicial para la ventana de consulta.
    /// </summary>
    public string FiltroVentana { get; set; }

    /// <summary>
    /// Inicializa una nueva instancia del formulario de consulta.
    /// Configura los manejadores de eventos y la información de consulta inicial.
    /// </summary>
    public FormConsulta()
    {
        InitializeComponent();
        Load += PrVentanaConsulta_Load;
        GridViewConsulta.DoubleClick += gridViewConsulta_DoubleClick;

        KeyPreview = true;

        KeyUp += PrVentanaConsulta_KeyUp;


        txtFiltro.Properties.KeyUp += (_, _) => ExecuteFilter(txtFiltro.Text.ToUpperInvariant());

        InformacionConsulta = new InformacionConsulta();
        InformacionConsulta.SetItem(0, "", "");
    }
    /// <summary>
    /// Establece la tabla de datos que se mostrará en la cuadrícula.
    /// </summary>
    /// <param name="table">Tabla de datos a mostrar.</param>
    public void SetDataTable(DataTable table) => Tabla = table;
    /// <summary>
    /// Maneja los eventos de teclado del formulario.
    /// - Escape: Cierra el formulario.
    /// - Enter: Selecciona el registro actual.
    /// </summary>
    private void PrVentanaConsulta_KeyUp(object sender, KeyEventArgs args)
    {
        switch (args.KeyCode)
        {
            case Keys.Escape:
                Close();
                break;
            case Keys.Enter:
                SelectedRecord();
                break;
        }
    }
    /// <summary>
    /// Maneja el evento de doble clic en la cuadrícula para seleccionar un registro.
    /// </summary>
    private void gridViewConsulta_DoubleClick(object sender, EventArgs eventArgs) => SelectedRecord();

    /// <summary>
    /// Se ejecuta cuando se carga el formulario.
    /// Configura el filtro inicial y el origen de datos de la cuadrícula.
    /// </summary>
    private void PrVentanaConsulta_Load(object sender, EventArgs eventArgs)
    {
        CenterToParent();
        txtFiltro.Text = FiltroVentana;
        ExecuteFilter(FiltroVentana);

        txtFiltro.Focus();
        if (Tabla.Rows.Count > 0)
        {
            if (Tabla.Rows.Count == 1)
            {
                Row = Tabla.Rows[0];
                SelectRecord(Row);
                Close();
                DialogResult = DialogResult.OK;
            }
            else
            {
                EstablecerOrigenDatos(Tabla);
            }
        }
    }
    /// <summary>
    /// Establece el origen de datos de la cuadrícula y configura las columnas.
    /// </summary>
    /// <param name="table">Tabla de datos a mostrar en la cuadrícula.</param>
    private void EstablecerOrigenDatos(DataTable table)
    {
        GridConsulta.DataSource = table;
        GridViewConsulta.Columns[0].Visible = false;
        GridViewConsulta.OptionsCustomization.AllowColumnResizing = true;
        GridViewConsulta.BestFitColumns(true);
    }
    /// <summary>
    /// Maneja la selección de un registro en la cuadrícula.
    /// Obtiene la fila seleccionada y cierra el formulario con resultado OK.
    /// </summary>
    private void SelectedRecord()
    {
        DataRowView rowView = (DataRowView)GridViewConsulta.GetFocusedRow();
        if (rowView is not null)
        {
            Row = rowView.Row;
            SelectRecord(Row);
            Close();
            DialogResult = DialogResult.OK;
        }
    }
    /// <summary>
    /// Procesa la fila seleccionada y actualiza la información de consulta.
    /// </summary>
    /// <param name="row">Fila de datos seleccionada.</param>
    private void SelectRecord(DataRow row)
    {
        List<string> columnas = row.Table
                                    .Columns
                                    .Cast<DataColumn>()
                                    .Take(3)
                                    .Select(dataColumn => dataColumn.ColumnName)
                                    .ToList();

        int id = (int)row[columnas[0]];
        string codigo = row[columnas[1]].ToString();
        string descripcion = row[columnas[2]].ToString();

        InformacionConsulta = new InformacionConsulta();
        InformacionConsulta.SetItem(id, codigo, descripcion);
    }
    /// <summary>
    /// Aplica un filtro a la cuadrícula basado en el texto proporcionado.
    /// Busca coincidencias en todas las columnas visibles.
    /// </summary>
    /// <param name="palabra">Texto a buscar en las columnas.</param>
    public void ExecuteFilter(string palabra)
    {
        GridViewConsulta.ActiveFilterString = "";
        GridViewConsulta.ActiveFilterEnabled = true;

        string condicionBusqueda = string.Empty;
        foreach (GridColumn columna in GridViewConsulta.Columns)
        {
            columna.FilterMode = ColumnFilterMode.DisplayText;
            if (string.IsNullOrEmpty(condicionBusqueda) == false)
            {
                condicionBusqueda += " OR ";
            }

            condicionBusqueda += $"[{columna.FieldName}] LIKE '%{palabra}%' ";
        }

        GridViewConsulta.ActiveFilterString = condicionBusqueda;
    }

    /// <summary>
    /// Obtiene la información de la consulta seleccionada.
    /// </summary>
    /// <returns>Objeto que implementa IInformacionConsulta con los datos seleccionados.</returns>
    public IInformacionConsulta GetInformacionConsulta() => InformacionConsulta;
    /// <summary>
    /// Obtiene la fila de datos seleccionada.
    /// </summary>
    /// <returns>Fila de datos seleccionada o null si no hay selección.</returns>
    public DataRow GetRow() => Row;
    /// <summary>
    /// Maneja el evento de cierre del formulario.
    /// Libera los recursos de la tabla de datos.
    /// </summary>
    private void FormConsulta_FormClosed(object sender, FormClosedEventArgs e) => Tabla = null;
}