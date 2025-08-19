namespace DevKit.ControlesUsuarioWin.UI;

/// <summary>
/// Control de usuario personalizado que extiende PictureEdit para la visualización y edición de imágenes.
/// Implementa la interfaz IReadOnly para soportar el modo de solo lectura.
/// </summary>
public partial class UserControlPictureEdit : PictureEdit, IReadOnly
{
    /// <summary>
    /// Inicializa una nueva instancia del control UserControlPictureEdit.
    /// </summary>
    public UserControlPictureEdit()
    {
        InitializeComponent();
    }

    #region IReadOnly

    /// <summary>
    /// Obtiene o establece si se debe ignorar la propiedad ReadOnly.
    /// Permite sobrescribir el comportamiento de solo lectura en ciertos escenarios.
    /// </summary>
    public bool IgnoreReadOnly { get; set; }

    #endregion
}