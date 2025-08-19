namespace DevKit.ControlesUsuarioWin.Interfaces;

/// <summary>Define métodos para obtener información de archivos y directorios.</summary>
public interface IFileInfo
{
    /// <summary>Ruta seleccionada actualmente.</summary>
    string SelectedPath { get; set; }

    /// <summary>Obtiene la ruta del directorio de la ruta seleccionada.</summary>
    string GetPathDirectory();

    /// <summary>Obtiene la ruta de un archivo con filtros opcionales.</summary>
    /// <param name="filter">Filtros de archivo en formato de cadena.</param>
    /// <returns>Ruta del archivo seleccionado.</returns>
    string GetPathFile(string filter = "Excel 2007, 2010, 2013|*.xlsx|Todos los archivos (*.*)|*.*");

    /// <summary>Obtiene una lista de archivos con una extensión específica en un directorio.</summary>
    Task<List<FileInfo>> GetListFileInfo(string selectedPath, string extension);
}