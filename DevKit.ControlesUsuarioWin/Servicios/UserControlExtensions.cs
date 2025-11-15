namespace DevKit.ControlesUsuarioWin.Servicios;

/// <summary>Proporciona métodos de extensión para manipular controles de Windows Forms (ComboBoxEdit, CheckedComboBoxEdit, GridControl y BaseEdit).</summary>
public static class UserControlExtensions
{
    /// <summary>Proporciona métodos de extensión para manipular controles de Windows Forms (ComboBoxEdit, CheckedComboBoxEdit, GridControl y BaseEdit).</summary>
    extension(ComboBoxEdit comboBox)
    {
        /// <summary>Obtiene el valor principal del elemento seleccionado en un ComboBoxEdit.</summary>
        public T Principal<T>()
        {
            return comboBox.SelectedItem is IPrincipal<T> selectedItem
                ? selectedItem.PrincipalID
                : default;
        }

        /// <summary>Obtiene el item seleccionado en un ComboBoxEdit.</summary>
        public T GetItem<T>()
        {
            return (T)comboBox.SelectedItem;
        }

        /// <summary>Obtiene la lista de items en un ComboBoxEdit.</summary>
        public IReadOnlyList<T> GetItems<T>()
        {
            return comboBox.Properties.Items.Cast<T>().ToList();
        }

        /// <summary>Selecciona un item en un ComboBoxEdit por su PrincipalID.</summary>
        public void SetItem<T>(T principalID)
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
        public void SetItems<T>(IEnumerable<T> enumerable)
        {
            comboBox.Properties.Items.AddRange(enumerable.ToList());
        }
    }

    /// <summary>Establece el origen de datos de un GridControl y refresca las vistas.</summary>
    extension(GridControl gridControl)
    {
        /// <summary>Establece el origen de datos de un GridControl y refresca las vistas.</summary>
        public void SetItems<T>(T source)
        {
            gridControl.DataSource = source;
            gridControl.Views.ForEach(view => view.RefreshData());
        }

        /// <summary>Obtiene el origen de datos actual de un GridControl.</summary>
        public T GetItems<T>()
        {
            return (T)gridControl.DataSource;
        }
    }

    /// <summary>Agrega items a un CheckedComboBoxEdit.</summary>
    extension(CheckedComboBoxEdit checkedComboBox)
    {
        /// <summary>Agrega items a un CheckedComboBoxEdit.</summary>
        public void SetItems<T>(List<T> collection)
        {
            collection.ForEach(item => checkedComboBox.Properties.Items.Add(item));
        }

        /// <summary>Obtiene los items marcados en un CheckedComboBoxEdit.</summary>
        public IReadOnlyList<T> GetItems<T>()
        {
            return checkedComboBox.Properties.Items
                .Where(item => item.CheckState == CheckState.Checked)
                .Select(item => (T)item.Value).ToList();
        }

        /// <summary>Desmarca todos los items en un CheckedComboBoxEdit.</summary>
        public void UnCheckAllItems()
        {
            checkedComboBox.Properties.GetItems().ForEach(item => item.CheckState = CheckState.Unchecked);
        }
    }

    /// <summary>Habilita o deshabilita controles dentro de una colección de controles.</summary>
    extension(Control.ControlCollection controlCollection)
    {
        /// <summary>Habilita o deshabilita controles dentro de una colección de controles.</summary>
        public void EnableControls(bool habiliar)
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
    }

    /// <summary>Obtiene el contenido de texto de un control BaseEdit.</summary>
    extension(BaseEdit baseEdit)
    {
        /// <summary>Obtiene el contenido de texto de un control BaseEdit.</summary>
        public string GetText() => baseEdit.Text;

        /// <summary>Establece el contenido de texto en un control BaseEdit.</summary>
        public void SetText(string content) => baseEdit.Text = content;
    }
}