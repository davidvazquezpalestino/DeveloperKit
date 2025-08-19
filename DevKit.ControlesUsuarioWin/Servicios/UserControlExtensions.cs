namespace DevKit.ControlesUsuarioWin.Servicios;

/// <summary>Proporciona métodos de extensión para manipular controles de Windows Forms (ComboBoxEdit, CheckedComboBoxEdit, GridControl y BaseEdit).</summary>
public static class UserControlExtensions
{
    /// <summary>Obtiene el valor principal del elemento seleccionado en un ComboBoxEdit.</summary>
    public static T Principal<T>(this ComboBoxEdit comboBox)
    {
        return comboBox.SelectedItem is IPrincipal<T> selectedItem
            ? selectedItem.PrincipalID
            : default;
    }
    /// <summary>Obtiene el item seleccionado en un ComboBoxEdit.</summary>
    public static T GetItem<T>(this ComboBoxEdit comboBox)
    {
        return (T)comboBox.SelectedItem;
    }
    /// <summary>Obtiene la lista de items en un ComboBoxEdit.</summary>
    public static IReadOnlyList<T> GetItems<T>(this ComboBoxEdit comboBox)
    {
        return comboBox.Properties.Items.Cast<T>().ToList();
    }
    /// <summary>Selecciona un item en un ComboBoxEdit por su PrincipalID.</summary>
    public static void SetItem<T>(this ComboBoxEdit comboBox, T principalID)
    {
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
    public static void SetItems<T>(this ComboBoxEdit comboBox, IEnumerable<T> enumerable)
    {
        comboBox.Properties.Items.AddRange(enumerable.ToList());
    }

    /// <summary>Establece el origen de datos de un GridControl y refresca las vistas.</summary>
    public static void SetItems<T>(this GridControl gridControl, T source)
    {
        gridControl.DataSource = source;
        gridControl.Views.ForEach(view => view.RefreshData());
    }
    /// <summary>Obtiene el origen de datos actual de un GridControl.</summary>
    public static T GetItems<T>(this GridControl gridControl)
    {
        return (T)gridControl.DataSource;
    }
    /// <summary>Agrega items a un CheckedComboBoxEdit.</summary>
    public static void SetItems<T>(this CheckedComboBoxEdit checkedComboBox, List<T> collection)
    {
        collection.ForEach(item => checkedComboBox.Properties.Items.Add(item));
    }
    /// <summary>Obtiene los items marcados en un CheckedComboBoxEdit.</summary>
    public static IReadOnlyList<T> GetItems<T>(this CheckedComboBoxEdit checkedComboBox)
    {
        return checkedComboBox.Properties.Items
            .Where(item => item.CheckState == CheckState.Checked)
            .Select(item => (T)item.Value).ToList();
    }
    /// <summary>Desmarca todos los items en un CheckedComboBoxEdit.</summary>
    public static void UnCheckAllItems(this CheckedComboBoxEdit checkedComboBox)
    {
        checkedComboBox.Properties.GetItems().ForEach(item => item.CheckState = CheckState.Unchecked);
    }
    /// <summary>Habilita o deshabilita controles dentro de una colección de controles.</summary>
    public static void EnableControls(this Control.ControlCollection controlCollection, bool habiliar)
    {
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
    public static string GetText(this BaseEdit baseEdit) => baseEdit.Text;
    /// <summary>Establece el contenido de texto en un control BaseEdit.</summary>
    public static void SetText(this BaseEdit baseEdit, string content) => baseEdit.Text = content;
}