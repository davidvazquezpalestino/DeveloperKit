namespace DevKit.ControlesUsuarioWin.UI;

/// <summary>
/// Control de usuario personalizado que extiende MemoEdit para la edición de texto multilínea.
/// Incluye soporte para conversión de mayúsculas/minúsculas y formato de fuente personalizado.
/// </summary>
public class UserControlMemoEdit : MemoEdit, IReadOnly
{
    #region Propiedades
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
    /// Obtiene o establece si se debe ignorar la propiedad ReadOnly.
    /// Útil para sobrescribir el comportamiento de solo lectura en ciertos escenarios.
    /// </summary>
    public bool IgnoreReadOnly { get; set; }

    #endregion

    /// <summary>
    /// Se llama cuando se crea el control.
    /// Configura la fuente y la conversión de mayúsculas/minúsculas según las propiedades establecidas.
    /// </summary>
    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        Properties.Appearance.Font = Font;
        Properties.CharacterCasing = CharacterCasing;
    }
}