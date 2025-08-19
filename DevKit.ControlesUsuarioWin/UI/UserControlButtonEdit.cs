namespace DevKit.ControlesUsuarioWin.UI;

/// <summary>
/// Control de usuario personalizado que extiende ButtonEdit con funcionalidad adicional.
/// Incluye soporte para conversión de mayúsculas/minúsculas y configuración de fuente.
/// </summary>
public partial class UserControlButtonEdit : ButtonEdit
{
    /// <summary>
    /// Inicializa una nueva instancia del control UserControlButtonEdit.
    /// </summary>
    public UserControlButtonEdit() => InitializeComponent();
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
    /// <summary>
    /// Se llama cuando se crea el control.
    /// Configura propiedades iniciales como la fuente y la conversión de mayúsculas/minúsculas.
    /// </summary>
    protected override void OnCreateControl()
    {
        base.OnCreateControl();

        Properties.Appearance.Font = Font;
        Properties.CharacterCasing = CharacterCasing;
        Properties.ValidateOnEnterKey = true;
        EnterMoveNextControl = true;

    }
}