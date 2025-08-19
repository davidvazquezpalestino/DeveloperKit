namespace DevKit.CrystalToolkit.Servicios;

public partial class Report
{
    /// <summary>Exporta el informe al disco en el formato especificado.</summary>
    /// <param name="formatType">Formato de exportación del informe.</param>
    /// <param name="filePath">Ruta completa del archivo de destino.</param>
    /// <param name="parameter">Parámetros opcionales para el informe.</param>
    public void ExportToDisk(FormatType formatType, string filePath, IDictionary<string, object> parameter = null)
    {
        if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        }

        if (parameter is not null)
        {
            foreach (KeyValuePair<string, object> parametro in parameter)
            {
                ReportDocument.SetParameterValue(parametro.Key, parametro.Value);
            }
        }
        switch (formatType)
        {
            case FormatType.MicrosoftWord:
                ReportDocument.ExportToDisk(ExportFormatType.WordForWindows, filePath);
                break;
            case FormatType.MicrosoftExcel:
                ReportDocument.ExportToDisk(ExportFormatType.Excel, filePath);
                break;
            case FormatType.Pdf:
                ReportDocument.ExportToDisk(ExportFormatType.PortableDocFormat, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(formatType), formatType, null);
        }
    }

    /// <summary>Exporta el informe a un flujo de memoria en formato PDF.</summary>
    /// <param name="parameter">Parámetros opcionales para el informe.</param>
    /// <returns>Flujo de memoria con el informe exportado.</returns>
    public Stream ExportToDisk(IDictionary<string, object> parameter = null)
    {
        if (parameter is not null)
        {
            foreach (KeyValuePair<string, object> parametro in parameter)
            {
                ReportDocument.SetParameterValue(parametro.Key, parametro.Value);
            }
        }

        return ReportDocument.ExportToStream(ExportFormatType.PortableDocFormat);
    }

    /// <summary>Exporta el informe al disco de forma asíncrona en el formato especificado.</summary>
    /// <param name="formatType">Formato de exportación del informe.</param>
    /// <param name="filePath">Ruta completa del archivo de destino.</param>
    /// <param name="parameter">Parámetros opcionales para el informe.</param>
    /// <returns>Tarea que representa la operación asíncrona.</returns>
    public Task ExportToDiskAsync(FormatType formatType, string filePath, IDictionary<string, object> parameter = null)
    {
        ExportToDisk(formatType, filePath, parameter);
        return Task.CompletedTask;
    }

    /// <summary>Exporta el informe a un flujo de memoria de forma asíncrona en formato PDF.</summary>
    /// <param name="parameter">Parámetros opcionales para el informe.</param>
    /// <returns>Tarea que devuelve un flujo de memoria con el informe exportado.</returns>
    public Task<Stream> ExportToDiskAsync(IDictionary<string, object> parameter = null)
    {
        Stream stream = ExportToDisk(parameter);
        return Task.FromResult(stream);
    }
}