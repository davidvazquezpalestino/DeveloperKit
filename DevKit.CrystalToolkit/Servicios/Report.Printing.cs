namespace DevKit.CrystalToolkit.Servicios;

public partial class Report
{
    /// <summary>Imprime el informe con los parámetros especificados.</summary>
    /// <param name="parameter">Parámetros opcionales para el informe.</param>
    /// <param name="copies">Número de copias a imprimir (por defecto 1).</param>
    /// <param name="printerName">Nombre de la impresora a utilizar (opcional).</param>
    public void Print(IDictionary<string, object> parameter = null, int copies = 1, string printerName = null)
    {
        if (parameter is not null)
        {
            foreach (KeyValuePair<string, object> parametro in parameter)
            {
                ReportDocument.SetParameterValue(parametro.Key, parametro.Value);
            }
        }

        int pages = ReportDocument.FormatEngine.GetLastPageNumber(new ReportPageRequestContext());
        ReportDocument.PrintToPrinter(copies, false, 1, pages);

    }

    /// <summary>Imprime el informe de forma asíncrona con los parámetros especificados.</summary>
    /// <param name="parameter">Parámetros opcionales para el informe.</param>
    /// <param name="copies">Número de copias a imprimir (por defecto 1).</param>
    /// <param name="printerName">Nombre de la impresora a utilizar (opcional).</param>
    /// <returns>Tarea que representa la operación asíncrona.</returns>
    public Task PrintAsync(IDictionary<string, object> parameter = null, int copies = 1, string printerName = null)
    {
        if (parameter is not null)
        {
            foreach (KeyValuePair<string, object> parametro in parameter)
            {
                ReportDocument.SetParameterValue(parametro.Key, parametro.Value);
            }
        }

        int pages = ReportDocument.FormatEngine.GetLastPageNumber(new ReportPageRequestContext());
        ReportDocument.PrintToPrinter(copies, false, 1, pages);


        return Task.CompletedTask;
    }
}