using DevKit.ControlesUsuarioWin.Interfaces;

namespace DevKit.ControlesUsuarioWin.Servicios;

/// <summary>Implementación de IFileInfo para manejar operaciones de archivos y directorios.</summary>
public class CoreFileInfo : IFileInfo
{
    /// <summary>Obtiene o establece la ruta seleccionada actualmente.</summary>
    public string SelectedPath { get; set; }

    /// <summary>Muestra un diálogo para seleccionar un directorio.</summary>
    /// <returns>Ruta del directorio seleccionado o cadena vacía si se cancela.</returns>
    public string GetPathDirectory()
    {
        using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog { SelectedPath = SelectedPath })
        {
            return folderBrowserDialog.ShowDialog() == DialogResult.OK
                ? folderBrowserDialog.SelectedPath
                : string.Empty;
        }
    }
    /// <summary>Muestra un diálogo para seleccionar un archivo.</summary>
    /// <param name="filter">Filtro de archivos a mostrar en el diálogo.</param>
    /// <returns>Ruta del archivo seleccionado o cadena vacía si se cancela.</returns>
    public string GetPathFile(string filter = "Excel 2007, 2010, 2013|*.xlsx|Todos los archivos (*.*)|*.*")
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog { Title = @"Seleccionar Archivo", Filter = filter })
        {
            return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : string.Empty;
        }
    }
    /// <summary>Obtiene una lista de archivos con una extensión específica en un directorio.</summary>
    /// <param name="selectedPath">Ruta del directorio a buscar.</param>
    /// <param name="extension">Extensión de archivo a filtrar (ej: ".txt").</param>
    /// <returns>Lista de objetos FileInfo que coinciden con la extensión.</returns>
    public Task<List<FileInfo>> GetListFileInfo(string selectedPath, string extension)
    {
        using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog { SelectedPath = selectedPath })
        {
            if (Directory.Exists(folderBrowserDialog.SelectedPath) == false)
            {
                return Task.FromResult(new List<FileInfo>());
            }

            return Task.FromResult(Directory.GetFiles(folderBrowserDialog.SelectedPath)
                .Select(fileInfo => new FileInfo(fileInfo))
                .Where(fileInfo => string.Equals(fileInfo.Extension, extension, StringComparison.InvariantCultureIgnoreCase))
                .Select(fileInfo => fileInfo).ToList());
        }
    }
}