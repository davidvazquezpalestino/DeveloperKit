namespace DevKit.Exporter;

/// <summary>Proporciona métodos de extensión para exportar datos a diferentes formatos.</summary>
public static class ExportFileExtensions
{
    /// <summary>
    /// Define los tipos de formato de fecha que pueden utilizarse
    /// al mostrar o convertir valores de fecha y hora.
    /// </summary>
    public enum DateFormatType
    {
        /// <summary>
        /// Formato corto de fecha. 
        /// Generalmente incluye día, mes y año en una representación compacta 
        /// (por ejemplo: "10/02/2026" o "2026-02-10").
        /// </summary>
        Short,

        /// <summary>
        /// Formato largo de fecha. 
        /// Suele incluir el nombre completo del día de la semana, el mes y el año,
        /// proporcionando una representación más descriptiva 
        /// (por ejemplo: "martes, 10 de febrero de 2026").
        /// </summary>
        Long
    }
    /// <summary>Proporciona métodos de extensión para exportar datos a diferentes formatos.</summary>
    extension(DataTable table)
    {
        /// <summary>Exporta un DataTable a un archivo Excel.</summary>
        public void ExportToMicrosoftExcel(string fileName, DateFormatType dateFormatType = DateFormatType.Short)
        {
            GuardAgainstInvalidExcelExtension(fileName);

            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                using (IWorkbook workbook = Path.GetExtension(fileName).ToLower() == ".xls"
                           ? new HSSFWorkbook()
                           : new XSSFWorkbook())
                {
                    ISheet sheet = workbook.CreateSheet(string.IsNullOrEmpty(table.TableName) ? "Sheet1" : table.TableName);
                    IRow headerRow = sheet.CreateRow(0);

                    short dateFormat = workbook.CreateDataFormat().GetFormat(GetDateFormatString(dateFormatType));

                    // Create encabezado del excel
                    CreateHeaderWhenDataTable(table, headerRow, workbook);
                    ICellStyle genericCellStyle = CreateCellDetailsStyle(workbook);
                    ICellStyle dateCellStyle = CreateDateCellStyle(workbook, dateFormat);

                    // Escribe los registros en Excel
                    for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                    {
                        IRow row = sheet.CreateRow(rowIndex + 1);
                        DataRow dataRow = table.Rows[rowIndex];

                        for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                        {
                            object cellValue = dataRow[columnIndex];
                            SetCellValue(cellValue, row, columnIndex, genericCellStyle, dateCellStyle);
                        }
                    }

                    workbook.Write(fileStream);
                }
            }
        }
    }

    /// <summary>Exporta una colección de diccionarios a un archivo Excel.</summary>
    extension(IEnumerable<Dictionary<string, object>> dictionary)
    {
        /// <summary>Exporta una colección de diccionarios a un archivo Excel.</summary>
        public void ExportToMicrosoftExcel(string fileName, DateFormatType dateFormatType = DateFormatType.Short)
        {
            GuardAgainstInvalidExcelExtension(fileName);

            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                using (IWorkbook workbook = Path.GetExtension(fileName).ToLower() == ".xls"
                           ? new HSSFWorkbook()
                           : new XSSFWorkbook())
                {
                    string sheetname = Path.GetFileNameWithoutExtension(fileName);
                    if (sheetname.Length > 30)
                    {
                        sheetname = sheetname.Substring(30);
                    }
                    ISheet sheet = workbook.CreateSheet(sheetname);

                    // Asumiendo que la primera fila del diccionario tiene las claves para los encabezados
                    IRow header = sheet.CreateRow(0);
                    ICellStyle headerCellStyle = CreateCellHeaderStyle(workbook);

                    List<Dictionary<string, object>> items = dictionary.ToList();
                    if (items.Any())
                    {
                        // Crear encabezado con las claves de los diccionarios
                        Dictionary<string, object> firstRow = items.First();
                        int columnIndex = 0;
                        foreach (string key in firstRow.Keys)
                        {
                            ICell cell = header.CreateCell(columnIndex++);
                            cell.SetCellValue(key);
                            cell.CellStyle = headerCellStyle;
                        }
                    }

                    short dateFormat = workbook.CreateDataFormat().GetFormat(GetDateFormatString(dateFormatType));

                    ICellStyle genericCellStyle = CreateCellDetailsStyle(workbook);
                    ICellStyle dateCellStyle = CreateDateCellStyle(workbook, dateFormat);

                    // Escribir los registros
                    int rowIndex = 1; // Start after the header row
                    foreach (Dictionary<string, object> item in items)
                    {
                        IRow row = sheet.CreateRow(rowIndex++);
                        int columnIndex = 0;

                        foreach (string key in item.Keys)
                        {
                            object cellValue = item[key];
                            SetCellValue(cellValue, row, columnIndex++, genericCellStyle, dateCellStyle);
                        }
                    }

                    workbook.Write(fileStream);
                }
            }
        }
    }

    /// <summary>Exporta una colección de objetos a un archivo Excel.</summary>
    extension<T>(IEnumerable<T> data)
    {
        /// <summary>Exporta una colección de objetos a un archivo Excel.</summary>
        public void ExportToMicrosoftExcel(string fileName, DateFormatType dateFormatType = DateFormatType.Short)
        {
            GuardAgainstInvalidExcelExtension(fileName);

            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook = Path.GetExtension(fileName).ToLower() == ".xls"
                    ? new HSSFWorkbook()
                    : new XSSFWorkbook();

                ISheet sheet = workbook.CreateSheet(typeof(T).Name);
                IRow headerRow = sheet.CreateRow(0);

                // Aquí obtienes las propiedades del tipo T
                PropertyInfo[] properties = typeof(T).GetProperties();

                // Crear encabezados
                for (int i = 0; i < properties.Length; i++)
                {
                    headerRow.CreateCell(i).SetCellValue(properties[i].Name);
                }

                // Escribir registros
                int rowIndex = 1;
                IEnumerable<T> items = data.ToList();
                foreach (T item in items)
                {
                    IRow row = sheet.CreateRow(rowIndex);
                    for (int columnIndex = 0; columnIndex < properties.Length; columnIndex++)
                    {
                        object value = properties[columnIndex].GetValue(item);
                        row.CreateCell(columnIndex).SetCellValue(value?.ToString() ?? string.Empty);
                    }
                    rowIndex++;
                }

                workbook.Write(fileStream);
                Console.WriteLine($"Archivo escrito en {fileName} con {items.Count()} registros");
            }

        }
    }

    /// <summary>Exporta un DataTable a un MemoryStream en formato Excel.</summary>
    extension(DataTable table)
    {
        /// <summary>Exporta un DataTable a un MemoryStream en formato Excel.</summary>
        public MemoryStream ExportToMicrosoftExcel(DateFormatType dateFormatType = DateFormatType.Short)
        {
            // Si la tabla está vacía, lanzamos una excepción (opcional)
            if (table == null || table.Rows.Count == 0)
            {
                throw new InvalidOperationException("La tabla está vacía.");
            }

            // Creamos un MemoryStream para escribir el archivo en memoria
            MemoryStream memoryStream = new MemoryStream();

            // Creamos el libro de trabajo (XLSX o XLS)
            IWorkbook workbook = Path.GetExtension("output.xlsx").ToLower() == ".xls" ? new HSSFWorkbook() : new XSSFWorkbook();

            // Creamos la hoja
            ISheet sheet = workbook.CreateSheet(string.IsNullOrEmpty(table.TableName) ? "Sheet1" : table.TableName);

            // Definimos el formato de fecha
            short dateFormat = workbook.CreateDataFormat().GetFormat(GetDateFormatString(dateFormatType));

            // Creamos el encabezado
            IRow headerRow = sheet.CreateRow(0);
            CreateHeaderWhenDataTable(table, headerRow, workbook);

            // Definimos estilos de celda
            ICellStyle genericCellStyle = CreateCellDetailsStyle(workbook);
            ICellStyle dateCellStyle = CreateDateCellStyle(workbook, dateFormat);

            // Escribimos los registros de la tabla en el archivo Excel
            for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            {
                IRow row = sheet.CreateRow(rowIndex + 1);
                DataRow dataRow = table.Rows[rowIndex];

                for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                {
                    object cellValue = dataRow[columnIndex];
                    SetCellValue(cellValue, row, columnIndex, genericCellStyle, dateCellStyle);
                }
            }

            // Escribimos el libro de trabajo en el MemoryStream
            workbook.Write(memoryStream, true);

            // Restablecemos el puntero del MemoryStream al inicio para que se pueda leer correctamente al devolverlo
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Devolvemos el MemoryStream generado con el archivo Excel
            return memoryStream;
        }
    }

    /// <summary>Exporta una colección de objetos a un MemoryStream en formato Excel.</summary>
    extension<T>(IEnumerable<T> data)
    {
        /// <summary>Exporta una colección de objetos a un MemoryStream en formato Excel.</summary>
        /// <param name="dateFormatType"></param>
        public MemoryStream ExportToMicrosoftExcel(DateFormatType dateFormatType = DateFormatType.Short)
        {
            // Crea un MemoryStream
            MemoryStream memoryStream = new MemoryStream();

            // Usamos el tipo de archivo adecuado (XLSX)
            using (IWorkbook workbook = new XSSFWorkbook())
            {
                ISheet sheet = workbook.CreateSheet(typeof(T).Name);
                IRow headerRow = sheet.CreateRow(0);
                PropertyInfo[] properties = typeof(T).GetProperties();
                short dateFormat = workbook.CreateDataFormat().GetFormat(GetDateFormatString(dateFormatType));

                // Crea el encabezado del registro
                CreateHeaderWhenIList(properties, headerRow, workbook);
                ICellStyle genericCellStyle = CreateCellDetailsStyle(workbook);
                ICellStyle dateCellStyle = CreateDateCellStyle(workbook, dateFormat);

                // Escribe los registros
                int rowIndex = 1;
                foreach (T item in data)
                {
                    IRow row = sheet.CreateRow(rowIndex);
                    for (int columnIndex = 0; columnIndex < properties.Length; columnIndex++)
                    {
                        object value = properties[columnIndex].GetValue(item);
                        SetCellValue(value, row, columnIndex, genericCellStyle, dateCellStyle);
                    }
                    rowIndex++;
                }


                // Escribe el archivo en el MemoryStream
                workbook.Write(memoryStream, true);
            }
            // Restaura el puntero del MemoryStream al principio para la lectura
            memoryStream.Seek(0, SeekOrigin.Begin);
            // Regresa el MemoryStream con los datos del archivo Excel
            return memoryStream;
        }
    }


    /// <summary>Exporta una colección de diccionarios a un MemoryStream en formato Excel.</summary>
    extension(IEnumerable<Dictionary<string, object>> dictionary)
    {
        /// <summary>Exporta una colección de diccionarios a un MemoryStream en formato Excel.</summary>
        /// <param name="dateFormatType"></param>
        public MemoryStream ExportToMicrosoftExcel(DateFormatType dateFormatType = DateFormatType.Short)
        {
            // Crear un MemoryStream en lugar de escribir a un archivo físico
            MemoryStream memoryStream = new MemoryStream();

            using (IWorkbook workbook = new XSSFWorkbook())
            {
                ISheet sheet = workbook.CreateSheet("Sheet1");

                // Asumir que la primera fila del diccionario tiene las claves para los encabezados
                IRow headerRow = sheet.CreateRow(0);
                ICellStyle headerCellStyle = CreateCellHeaderStyle(workbook);

                List<Dictionary<string, object>> items = dictionary.ToList();
                if (items.Any())
                {
                    // Crear encabezado con las claves de los diccionarios
                    Dictionary<string, object> item = items.First();
                    int columnIndex = 0;
                    foreach (string key in item.Keys)
                    {
                        ICell cell = headerRow.CreateCell(columnIndex++);
                        cell.SetCellValue(key);
                        cell.CellStyle = headerCellStyle;
                    }
                }

                short dateFormat = workbook.CreateDataFormat().GetFormat(GetDateFormatString(dateFormatType));

                ICellStyle genericCellStyle = CreateCellDetailsStyle(workbook);
                ICellStyle dateCellStyle = CreateDateCellStyle(workbook, dateFormat);

                // Escribir los registros
                int rowIndex = 1; // Start after the header row
                foreach (Dictionary<string, object> item in items)
                {
                    IRow row = sheet.CreateRow(rowIndex++);
                    int columnIndex = 0;

                    foreach (string key in item.Keys)
                    {
                        object cellValue = item[key];
                        SetCellValue(cellValue, row, columnIndex++, genericCellStyle, dateCellStyle);
                    }
                }

                // Escribimos el libro de trabajo en el MemoryStream
                workbook.Write(memoryStream, true);

                // Restablecemos el puntero del MemoryStream al inicio para que se pueda leer correctamente al devolverlo
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
        }
    }

    /// <summary>Verifica si la extensión del archivo es válida para un archivo Excel.</summary>
    private static void GuardAgainstInvalidExcelExtension(string fileName)
    {
        string extension = Path.GetExtension(fileName)?.ToLower();
        if (extension != ".xls" && extension != ".xlsx")
        {
            throw new ArgumentException("El archivo debe tener extensión .xls o .xlsx.");
        }
    }
    /// <summary>Crea el encabezado para una colección de objetos.</summary>
    private static void CreateHeaderWhenIList(PropertyInfo[] properties, IRow headerRow, IWorkbook workbook)
    {
        ICellStyle cellCellStyle = CreateCellHeaderStyle(workbook);
        for (int index = 0; index < properties.Length; index++)
        {
            ICell cell = headerRow.CreateCell(index);
            cell.SetCellValue(properties[index].Name);
            cell.CellStyle = cellCellStyle;
        }
    }
    /// <summary>Crea el encabezado para un DataTable.</summary>
    private static void CreateHeaderWhenDataTable(DataTable table, IRow headerRow, IWorkbook workbook)
    {
        ICellStyle cellCellStyle = CreateCellHeaderStyle(workbook);
        for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
        {
            DataColumn column = table.Columns[columnIndex];
            ICell cell = headerRow.CreateCell(columnIndex);

            cell.SetCellValue(column.ColumnName);
            cell.CellStyle = cellCellStyle;
        }
    }
    /// <summary>Establece el valor de una celda según su tipo de dato.</summary>
    private static void SetCellValue(object value, IRow row, int columnIndex, ICellStyle genericCellStyle, ICellStyle dateCellStyle)
    {
        ICell cell = row.CreateCell(columnIndex);

        if (value is DateTime dateTime)
        {
            if (dateTime > DateTime.MinValue)
            {
                cell.SetCellValue(dateTime);
                cell.CellStyle = dateCellStyle;
            }
            else
            {
                cell.SetCellValue(string.Empty);
                cell.CellStyle = genericCellStyle;
            }
        }
        else if (value is bool b)
        {
            cell.SetCellValue(b);
            cell.CellStyle = genericCellStyle;
        }
        else if (value is int or long or short or byte)
        {
            cell.SetCellValue(Convert.ToInt64(value));
            cell.CellStyle = genericCellStyle;
        }
        else if (value is double or float or decimal)
        {
            cell.SetCellValue(Convert.ToDouble(value));
            cell.CellStyle = genericCellStyle;
        }
        else if (value != null)
        {
            cell.SetCellValue(value.ToString());
            cell.CellStyle = genericCellStyle;
        }
        else
        {
            cell.SetCellValue(string.Empty);
            cell.CellStyle = genericCellStyle;
        }
    }
    /// <summary>Crea el estilo para las celdas de encabezado.</summary>
    private static ICellStyle CreateCellHeaderStyle(IWorkbook workbook)
    {
        ICellStyle cellStyle = workbook.CreateCellStyle();
        IFont font = workbook.CreateFont();
        font.FontName = "Calibri";
        font.FontHeightInPoints = 12;
        font.Color = IndexedColors.Blue.Index;
        font.IsBold = true;
        cellStyle.SetFont(font);

        return cellStyle;
    }
    /// <summary>Crea el estilo para las celdas de datos.</summary>
    private static ICellStyle CreateCellDetailsStyle(IWorkbook workbook)
    {
        ICellStyle cellStyle = workbook.CreateCellStyle();
        IFont font = workbook.CreateFont();
        font.FontName = "Calibri";
        font.FontHeightInPoints = 10;
        font.Color = IndexedColors.Black.Index;
        cellStyle.SetFont(font);

        return cellStyle;
    }
    /// <summary>Crea el estilo para las celdas de tipo fecha.</summary>
    private static ICellStyle CreateDateCellStyle(IWorkbook workbook, short dateFormat)
    {
        ICellStyle dateCellStyle = workbook.CreateCellStyle();
        dateCellStyle.DataFormat = dateFormat;

        IFont font = workbook.CreateFont();
        font.FontName = "Calibri";
        font.FontHeightInPoints = 10;
        dateCellStyle.SetFont(font);

        return dateCellStyle;
    }
    private static string GetDateFormatString(DateFormatType dateFormatType)
    {
        return dateFormatType switch
        {
            DateFormatType.Short => "yyyy-MM-dd",
            _ => "yyyy-MM-dd HH:mm:ss"
        };
    }
}