using DevExpress.XtraEditors.Mask;

namespace DevKit.ControlesUsuarioWin.UI;

/// <summary>
/// Control de usuario personalizado que extiende DateEdit para la selección de fechas.
/// Incluye soporte para navegación con teclado y formato de fecha mejorado.
/// </summary>
public partial class UserControlDateEdit : DateEdit, IReadOnly
{
    /// <summary>
    /// Inicializa una nueva instancia del control UserControlDateEdit.
    /// </summary>
    public UserControlDateEdit() => InitializeComponent();

    /// <summary>
    /// Obtiene o establece si se debe ignorar la propiedad ReadOnly.
    /// Útil para sobrescribir el comportamiento de solo lectura en ciertos escenarios.
    /// </summary>
    public bool IgnoreReadOnly { get; set; }

    /// <summary>
    /// Se llama cuando se crea el control.
    /// Configura las propiedades iniciales para el control de selección de fecha.
    /// </summary>
    protected override void OnCreateControl()
    {
        EnterMoveNextControl = true;
        Properties.Mask.MaskType = MaskType.DateTimeAdvancingCaret;
        Properties.Mask.SaveLiteral = false;
        Properties.TextEditStyle = TextEditStyles.Standard;
    }
}