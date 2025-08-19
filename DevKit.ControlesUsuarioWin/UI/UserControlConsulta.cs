namespace DevKit.ControlesUsuarioWin.UI;

/// <summary>
/// Control de usuario personalizado que extiende ButtonEdit para realizar consultas.
/// Permite realizar búsquedas con un botón integrado y manejar la validación de datos.
/// </summary>
public partial class UserControlConsulta : ButtonEdit, IReadOnly
{
    #region Propiedades
    /// <summary>
    /// Obtiene o establece si se debe ignorar la propiedad ReadOnly.
    /// </summary>
    public bool IgnoreReadOnly { get; set; }
    /// <summary>
    /// Obtiene o establece la información de la consulta actual.
    /// </summary>
    private IInformacionConsulta InformacionConsulta { get; set; }
    /// <summary>
    /// Obtiene o establece la fila de datos actual.
    /// </summary>
    private DataRow Row { get; set; }
    /// <summary>
    /// Obtiene o establece el tipo de conversión de mayúsculas/minúsculas para el texto.
    /// Por defecto está configurado a Mayúsculas.
    /// </summary>
    public CharacterCasing CharacterCasing { get; set; } = CharacterCasing.Upper;


    #endregion

    /// <summary>
    /// Evento que se dispara cuando se inicia una consulta.
    /// </summary>
    public event Action Consultando;
    /// <summary>
    /// Evento que se dispara cuando se valida el contenido del control.
    /// </summary>
    public event Action Validando;

    /// <summary>
    /// Inicializa una nueva instancia del control UserControlConsulta.
    /// </summary>
    public UserControlConsulta() => InitializeComponent();

    /// <summary>
    /// Se llama cuando se crea el control.
    /// Configura el botón de búsqueda y otras propiedades iniciales.
    /// </summary>
    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (Properties.Buttons.Count == 0)
        {
            Properties.Buttons.Add(new EditorButton(ButtonPredefines.Search));
            Properties.Buttons[0].Shortcut = new KeyShortcut(Keys.F3);
            Properties.Buttons[0].ToolTip = "Tecla F3 para hacer busquedas.";
            Properties.Buttons[0].ImageOptions.SvgImage = Resources.search;
            Properties.Buttons[0].ImageOptions.SvgImageSize = new Size(16, 15);
            Properties.Buttons[0].Kind = ButtonPredefines.Glyph;
        }
        Properties.Buttons[0].Click += UserControlConsulta_Consultando;
        Properties.CharacterCasing = CharacterCasing;
        Properties.ValidateOnEnterKey = true;
        EnterMoveNextControl = true;
    }
    /// <summary>
    /// Maneja el evento de teclado cuando se presiona una tecla en el control.
    /// </summary>
    /// <param name="e">Datos del evento de teclado.</param>
    protected override void OnEditorKeyDown(KeyEventArgs e)
    {
        base.OnEditorKeyDown(e);

        if (e.KeyCode == Keys.Enter)
        {
            if (ReadOnly == false)
            {
                if (string.IsNullOrEmpty(Text))
                {
                    InformacionConsulta.SetItem(0, "", "");
                }

                Validando?.Invoke();
            }
        }
    }
    /// <summary>
    /// Establece la información de consulta para este control.
    /// </summary>
    /// <param name="informacionConsulta">Objeto que implementa IInformacionConsulta con la información de la consulta.</param>
    public void SetInformacionConsulta(IInformacionConsulta informacionConsulta) => InformacionConsulta = informacionConsulta;
    /// <summary>
    /// Establece la información de la consulta con los valores especificados.
    /// </summary>
    /// <param name="principalID">ID principal del ítem.</param>
    /// <param name="codigo">Código del ítem.</param>
    /// <param name="descripcion">Descripción del ítem.</param>
    public void SetInformacionConsulta(int principalID, string codigo, string descripcion) => InformacionConsulta.SetItem(principalID, codigo, descripcion);

    /// <summary>
    /// Establece la fila de datos actual.
    /// </summary>
    /// <param name="row">Fila de datos a establecer.</param>
    /// <returns>La fila de datos establecida.</returns>
    internal DataRow SetRow(DataRow row) => Row = row;
    /// <summary>
    /// Obtiene la fila de datos actual.
    /// </summary>
    /// <returns>La fila de datos actual o null si no hay ninguna.</returns>
    public DataRow GetRow() => Row;
    /// <summary>
    /// Obtiene la información de consulta actual.
    /// </summary>
    /// <returns>Objeto que implementa IInformacionConsulta con la información actual.</returns>
    public IInformacionConsulta GetIInformacionConsulta() => InformacionConsulta;
    /// <summary>
    /// Obtiene el ID principal de la consulta actual.
    /// </summary>
    /// <returns>El ID principal o 0 si no hay información de consulta.</returns>
    public int GetPrincipal() => InformacionConsulta?.PrincipalID ?? 0;

    /// <summary>
    /// Manejador del evento de consulta.
    /// Se dispara cuando se hace clic en el botón de búsqueda.
    /// </summary>
    /// <param name="sender">Origen del evento.</param>
    /// <param name="e">Datos del evento.</param>
    public void UserControlConsulta_Consultando(object sender, EventArgs e)
    {
        if (ReadOnly == false)
        {
            Consultando?.Invoke();
        }
    }
}