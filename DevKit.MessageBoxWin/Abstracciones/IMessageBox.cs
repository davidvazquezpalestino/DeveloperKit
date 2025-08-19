namespace DevKit.MessageBoxWin.Abstracciones;

/// <summary>
/// Interfaz que define métodos para mostrar diferentes tipos de mensajes y diálogos.
/// </summary>
/// <typeparam name="TDialogResult">El tipo de resultado que se devuelve al mostrar un diálogo.</typeparam>
public interface IMessageBox<out TDialogResult>
{
    /// <summary>
    /// Muestra un mensaje con el contenido especificado.
    /// </summary>
    /// <param name="content">El contenido del mensaje.</param>
    /// <returns>El resultado del diálogo.</returns>
    void ShowMessage(string content);

    /// <summary>
    /// Muestra un mensaje de OK/Cancelar con el contenido especificado.
    /// </summary>
    /// <param name="content">El contenido del mensaje.</param>
    /// <returns>El resultado del diálogo.</returns>
    TDialogResult ShowOkCancel(string content);

    /// <summary>
    /// Muestra una pregunta con el contenido especificado.
    /// </summary>
    /// <param name="content">El contenido de la pregunta.</param>
    /// <returns>El resultado del diálogo.</returns>
    TDialogResult ShowQuestion(string content);

    /// <summary>
    /// Muestra un mensaje de advertencia con el contenido especificado.
    /// </summary>
    /// <param name="content">El contenido del mensaje de advertencia.</param>
    /// <returns>El resultado del diálogo.</returns>
    void ShowWarning(string content);

    /// <summary>
    /// Muestra un mensaje de error con el contenido especificado.
    /// </summary>
    /// <param name="content">El contenido del mensaje de error.</param>
    /// <returns>El resultado del diálogo.</returns>
    void ShowError(string content);

    /// <summary>
    /// Muestra un mensaje de error con la excepción especificada.
    /// </summary>
    /// <param name="exception">La excepción a mostrar en el mensaje de error.</param>
    /// <returns>El resultado del diálogo.</returns>
    void ShowError(Exception exception);
}