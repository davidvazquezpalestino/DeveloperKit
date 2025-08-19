namespace DevKit.ControlesUsuarioWin.UI;

/// <summary>
/// Control de usuario personalizado que extiende ComboBoxEdit con funcionalidad adicional.
/// Incluye soporte para solo lectura, conversión de mayúsculas/minúsculas y navegación mejorada.
/// </summary>
public partial class UserControlComboBoxEdit : ComboBoxEdit, IReadOnly
{
    /// <summary>
    /// Inicializa una nueva instancia del control UserControlComboBoxEdit.
    /// </summary>
    public UserControlComboBoxEdit() => InitializeComponent();

    #region Propiedades
    /// <summary>
    /// Obtiene o establece si se debe ignorar la propiedad ReadOnly.
    /// Útil para sobrescribir el comportamiento de solo lectura en ciertos escenarios.
    /// </summary>
    public bool IgnoreReadOnly { get; set; }
    /// <summary>
    /// Obtiene o establece el tipo de conversión de mayúsculas/minúsculas para el texto.
    /// Por defecto está configurado a Mayúsculas.
    /// </summary>
    public CharacterCasing CharacterCasing { get; set; } = CharacterCasing.Upper;
    /// <summary>
    /// Obtiene o establece la fuente del control.
    /// Por defecto usa Segoe UI de 10pt.
    /// </summary>
    public override Font Font { get; set; } = new Font(@"Segoe UI", 10F);
    #endregion

    /// <summary>
    /// Se llama cuando se crea el control.
    /// Configura propiedades iniciales como la fuente, conversión de mayúsculas/minúsculas
    /// y comportamiento de teclado.
    /// </summary>
    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        EnterMoveNextControl = true;
        Properties.Appearance.Font = Font;
        Properties.CharacterCasing = CharacterCasing;
        Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
        KeyUp += (_, e) =>
        {
            if (e.KeyCode is Keys.Space or Keys.Down)
            {
                ShowPopup();
            }
        };
    }

}