namespace DevKit.ControlesUsuarioWin.UI;

/// <summary>
/// Control de usuario personalizado que extiende TextEdit con funcionalidad adicional.
/// Implementa IReadOnly para soportar el modo de solo lectura.
/// </summary>
public partial class UserControlTextEdit : TextEdit, IReadOnly
{
    /// <summary>
    /// Inicializa una nueva instancia del control UserControlTextEdit.
    /// Configura el manejo de eventos de teclado para la tecla Enter.
    /// </summary>
    public UserControlTextEdit()
    {
        InitializeComponent();

        KeyUp += (_, eventArgs) =>
        {
            if (eventArgs.KeyCode == Keys.Enter)
            {
                Validando?.Invoke(this);
            }
        };
    }

    #region Eventos

    /// <summary>
    /// Evento que se dispara cuando se presiona la tecla Enter en el control.
    /// </summary>
    public event Action<object> Validando;

    #endregion

    /// <summary>
    /// Se llama cuando se crea el control.
    /// Configura propiedades iniciales como la conversión de mayúsculas/minúsculas y el comportamiento de la tecla Enter.
    /// </summary>
    protected override void OnCreateControl()
    {
        EnterMoveNextControl = true;
        Properties.CharacterCasing = CharacterCasing;
        Properties.ValidateOnEnterKey = true;
        TabStop = !Properties.ReadOnly;
        base.OnCreateControl();
    }

    #region Propiedades

    /// <summary>
    /// Obtiene o establece el tipo de conversión de mayúsculas/minúsculas para el texto.
    /// Por defecto está configurado a Mayúsculas.
    /// </summary>
    public CharacterCasing CharacterCasing { get; set; } = CharacterCasing.Upper;
    /// <summary>
    /// Obtiene o establece un valor que indica si se debe ignorar la propiedad ReadOnly.
    /// Útil cuando se necesita sobrescribir el comportamiento de solo lectura en ciertos escenarios.
    /// </summary>
    public bool IgnoreReadOnly { get; set; }

    #endregion
}