namespace DevKit.MessageBoxWin.Servicios;
/// <summary>
/// Implementación de IMessageBox que muestra mensajes usando MessageBox de Windows Forms.
/// </summary>
public class WindowsMessageBox(ILogger logger) : IMessageBox<DialogResult>
{
    /// <summary>
    /// Muestra un mensaje informativo con botón Aceptar.
    /// </summary>
    public void ShowMessage(string content)
    {
        MessageBox.Show(content, " Soluciones Tecnológicas para su Negocio", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
    }

    /// <summary>
    /// Muestra un mensaje con botones Aceptar/Cancelar.
    /// </summary>
    public DialogResult ShowOkCancel(string content)
    {
        return MessageBox.Show(content, " Soluciones Tecnológicas para su Negocio", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
    }

    /// <summary>
    /// Muestra una pregunta con botones Sí/No.
    /// </summary>
    public DialogResult ShowQuestion(string content)
    {
        return MessageBox.Show(content, " Soluciones Tecnológicas para su Negocio", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
    }
    /// <summary>
    /// Muestra una advertencia con botón Aceptar.
    /// </summary>
    public void ShowWarning(string content)
    {
        MessageBox.Show(content, " Soluciones Tecnológicas para su Negocio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    /// <summary>
    /// Muestra un mensaje de error con botón Aceptar.
    /// </summary>
    public void ShowError(string content)
    {
        logger.Error(content);
        MessageBox.Show(content, "Soluciones Tecnológicas para su Negocio", MessageBoxButtons.OK, MessageBoxIcon.Hand);
    }

    /// <summary>
    /// Muestra el mensaje de una excepción como error.
    /// </summary>
    public void ShowError(Exception exception)
    {
        logger.Error(exception, exception.Message);
        MessageBox.Show(exception.Message, "Soluciones Tecnológicas para su Negocio", MessageBoxButtons.OK, MessageBoxIcon.Hand);
    }
}