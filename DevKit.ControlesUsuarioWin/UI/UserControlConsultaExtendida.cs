namespace DevKit.ControlesUsuarioWin.UI;

/// <summary>
/// Control de usuario que extiende UserControlConsulta con funcionalidad adicional para búsquedas extendidas.
/// Permite mostrar un formulario de consulta personalizado para seleccionar elementos.
/// </summary>
public partial class UserControlConsultaExtendida : UserControlConsulta
{
    /// <summary>
    /// Evento que se dispara antes de realizar una consulta.
    /// Permite proporcionar los datos que se mostrarán en el formulario de consulta.
    /// </summary>
    public event Func<DataTable> PreConsultando;
    /// <summary>
    /// Obtiene o establece la alineación horizontal del texto en el control.
    /// Por defecto está centrado.
    /// </summary>
    public HorzAlignment TextAlignment { get; set; } = HorzAlignment.Center;
    /// <summary>
    /// Inicializa una nueva instancia del control UserControlConsultaExtendida.
    /// Configura el manejador de eventos para la consulta.
    /// </summary>
    public UserControlConsultaExtendida()
    {
        InitializeComponent();
        Consultando += VentanaConsulta;
    }
    /// <summary>
    /// Muestra el formulario de consulta cuando se inicia una búsqueda.
    /// Obtiene los datos a través del evento PreConsultando y muestra un formulario de selección.
    /// </summary>
    private void VentanaConsulta()
    {
        DataTable tabla = PreConsultando?.Invoke();
        if (tabla is not null)
        {
            FormConsulta formConsulta = new FormConsulta { FiltroVentana = Text };
            formConsulta.SetDataTable(tabla);
            if (formConsulta.ShowDialog(this) == DialogResult.OK)
            {
                SetRow(formConsulta.GetRow());
                SetInformacionConsulta(formConsulta.GetInformacionConsulta());
                EditValue = formConsulta.GetInformacionConsulta().Codigo;
            }
        }
    }
}