namespace DevKit.ControlesUsuarioWin.Servicios;

/// <summary>Proporciona métodos de extensión para manipular controles de Windows Forms (ComboBoxEdit, CheckedComboBoxEdit, GridControl y BaseEdit).</summary>
public static class UserControlExtensions
{
    /// <summary>Obtiene el valor principal del elemento seleccionado en un ComboBoxEdit.</summary>
    /// <typeparam name="T">El tipo del valor principal.</typeparam>
    /// <param name="comboBox">El <see cref="ComboBoxEdit"/> de origen.</param>
    /// <returns>El valor de PrincipalID o default(T).</returns>
    public static T Principal<T>(this ComboBoxEdit comboBox)
    {
        if (comboBox == null)
        {
            return default;
        }

        return comboBox.SelectedItem is IPrincipal<T> selectedItem
            ? selectedItem.PrincipalID
            : default;
    }

    /// <summary>Obtiene el item seleccionado en un ComboBoxEdit.</summary>
    /// <typeparam name="T">El tipo del item.</typeparam>
    /// <param name="comboBox">El <see cref="ComboBoxEdit"/> de origen.</param>
    /// <returns>El item seleccionado casteado a T.</returns>
    public static T GetItem<T>(this ComboBoxEdit comboBox)
    {
        if (comboBox == null)
        {
            return default;
        }

        return (T)comboBox.SelectedItem;
    }

    /// <summary>Obtiene la lista de items en un ComboBoxEdit.</summary>
    /// <typeparam name="T">El tipo de los items.</typeparam>
    /// <param name="comboBox">El <see cref="ComboBoxEdit"/> de origen.</param>
    /// <returns>Una lista de solo lectura con los items.</returns>
    public static IReadOnlyList<T> GetItems<T>(this ComboBoxEdit comboBox)
    {
        if (comboBox == null)
        {
            return new List<T>().AsReadOnly();
        }

        return comboBox.Properties.Items.Cast<T>().ToList();
    }

    /// <summary>Selecciona un item en un ComboBoxEdit por su PrincipalID.</summary>
    /// <typeparam name="T">El tipo del PrincipalID.</typeparam>
    /// <param name="comboBox">El <see cref="ComboBoxEdit"/> de origen.</param>
    /// <param name="principalID">El ID a buscar.</param>
    public static void SetItem<T>(this ComboBoxEdit comboBox, T principalID)
    {
        if (comboBox == null)
        {
            return;
        }

        IPrincipal<T> selectedItem = comboBox.Properties.Items
            .OfType<IPrincipal<T>>()
            .FirstOrDefault(principal => principal.PrincipalID.Equals(principalID));

        if (selectedItem == null)
        {
            comboBox.SelectedIndex = -1;
        }
        else
        {
            comboBox.SelectedItem = selectedItem;
        }
    }

    /// <summary>Agrega items a un ComboBoxEdit.</summary>
    /// <typeparam name="T">El tipo de los items.</typeparam>
    /// <param name="comboBox">El <see cref="ComboBoxEdit"/> de origen.</param>
    /// <param name="enumerable">La colección de items a agregar.</param>
    public static void SetItems<T>(this ComboBoxEdit comboBox, IEnumerable<T> enumerable)
    {
        if (comboBox == null || enumerable == null)
        {
            return;
        }

        comboBox.Properties.Items.AddRange(enumerable.ToList());
    }

    /// <summary>Establece el origen de datos de un GridControl y refresca las vistas.</summary>
    /// <typeparam name="T">El tipo del origen de datos.</typeparam>
    /// <param name="gridControl">El <see cref="GridControl"/> de origen.</param>
    /// <param name="source">El origen de datos.</param>
    public static void SetItems<T>(this GridControl gridControl, T source)
    {
        if (gridControl == null)
        {
            return;
        }

        gridControl.DataSource = source;
        gridControl.Views.ForEach(view => view.RefreshData());
    }

    /// <summary>Obtiene el origen de datos actual de un GridControl.</summary>
    /// <typeparam name="T">El tipo del origen de datos esperado.</typeparam>
    /// <param name="gridControl">El <see cref="GridControl"/> de origen.</param>
    /// <returns>El origen de datos casteado a T.</returns>
    public static T GetItems<T>(this GridControl gridControl)
    {
        if (gridControl == null)
        {
            return default;
        }

        return (T)gridControl.DataSource;
    }

    /// <summary>Agrega items a un CheckedComboBoxEdit.</summary>
    /// <typeparam name="T">El tipo de los items.</typeparam>
    /// <param name="checkedComboBox">El <see cref="CheckedComboBoxEdit"/> de origen.</param>
    /// <param name="collection">La lista de items a agregar.</param>
    public static void SetItems<T>(this CheckedComboBoxEdit checkedComboBox, List<T> collection)
    {
        if (checkedComboBox == null || collection == null)
        {
            return;
        }

        collection.ForEach(item => checkedComboBox.Properties.Items.Add(item));
    }

    /// <summary>Obtiene los items marcados en un CheckedComboBoxEdit.</summary>
    /// <typeparam name="T">El tipo de los items.</typeparam>
    /// <param name="checkedComboBox">El <see cref="CheckedComboBoxEdit"/> de origen.</param>
    /// <returns>Una lista con los items seleccionados.</returns>
    public static IReadOnlyList<T> GetItems<T>(this CheckedComboBoxEdit checkedComboBox)
    {
        if (checkedComboBox == null)
        {
            return new List<T>().AsReadOnly();
        }

        return checkedComboBox.Properties.Items
            .Where(item => item.CheckState == CheckState.Checked)
            .Select(item => (T)item.Value).ToList();
    }

    /// <summary>Desmarca todos los items en un CheckedComboBoxEdit.</summary>
    /// <param name="checkedComboBox">El <see cref="CheckedComboBoxEdit"/> de origen.</param>
    public static void UnCheckAllItems(this CheckedComboBoxEdit checkedComboBox)
    {
        if (checkedComboBox == null)
        {
            return;
        }

        checkedComboBox.Properties.GetItems().ForEach(item => item.CheckState = CheckState.Unchecked);
    }

    /// <summary>Habilita o deshabilita controles dentro de una colección de controles.</summary>
    /// <param name="controlCollection">La colección de controles.</param>
    /// <param name="habiliar">Verdadero para habilitar, falso para solo lectura.</param>
    public static void EnableControls(this Control.ControlCollection controlCollection, bool habiliar)
    {
        if (controlCollection == null)
        {
            return;
        }

        foreach (Control control1 in controlCollection)
        {
            if (control1 is IReadOnly control)
            {
                if (control.IgnoreReadOnly == false)
                {
                    control.ReadOnly = habiliar == false;
                }
            }

            if (control1.HasChildren)
            {
                control1.Controls.EnableControls(habiliar);
            }
        }
    }

    /// <summary>Obtiene el contenido de texto de un control BaseEdit.</summary>
    /// <param name="baseEdit">El control de origen.</param>
    /// <returns>El texto del control.</returns>
    public static string GetText(this BaseEdit baseEdit) => baseEdit?.Text;

    /// <summary>Establece el contenido de texto en un control BaseEdit.</summary>
    /// <param name="baseEdit">El control de origen.</param>
    /// <param name="content">El nuevo texto.</param>
    public static void SetText(this BaseEdit baseEdit, string content)
    {
        if (baseEdit != null)
        {
            baseEdit.Text = content;
        }
    }
}