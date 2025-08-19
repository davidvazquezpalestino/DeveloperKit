namespace DevKit.ControlesUsuarioWin.UI;

/// <summary>
/// Control de usuario personalizado que extiende SpinEdit para la entrada de valores numéricos.
/// Incluye soporte para navegación con teclado y formato de fuente personalizado.
/// </summary>
public partial class UserControlSpinEdit : SpinEdit, IReadOnly
{
    /// <summary>
    /// Inicializa una nueva instancia del control UserControlSpinEdit.
    /// </summary>
    public UserControlSpinEdit() => InitializeComponent();
    /// <summary>
    /// Obtiene o establece la fuente del control.
    /// Por defecto usa Segoe UI de 10pt.
    /// </summary>
    public override Font Font { get; set; } = new Font(@"Segoe UI", 10F);

    /// <summary>
    /// Obtiene o establece si se debe ignorar la propiedad ReadOnly.
    /// Permite sobrescribir el comportamiento de solo lectura en ciertos escenarios.
    /// </summary>
    public bool IgnoreReadOnly { get; set; }

    /// <summary>
    /// Se llama cuando se crea el control.
    /// Configura las propiedades iniciales para el control de entrada numérica.
    /// </summary>
    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        EnterMoveNextControl = true;
        Properties.Appearance.Font = Font;
        Properties.ValidateOnEnterKey = true;
    }
}