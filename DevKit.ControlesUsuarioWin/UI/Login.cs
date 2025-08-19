namespace DevKit.ControlesUsuarioWin.UI;

/// <summary>
/// Control de usuario que proporciona una interfaz para gestionar conexiones a bases de datos.
/// Permite agregar, eliminar y seleccionar conexiones guardadas.
/// </summary>
public partial class Login : XtraUserControl, IReadOnly
{
    /// <summary>
    /// Lista que almacena las conexiones configuradas.
    /// </summary>
    public List<Conexion> Connections = new List<Conexion>();
    /// <summary>
    /// Evento que se dispara cuando se selecciona una conexión en el combo.
    /// </summary>
    public event Action<Conexion> SelectedIndex;

    /// <summary>
    /// Obtiene o establece si el control está en modo de solo lectura.
    /// </summary>
    public bool ReadOnly { get; set; }
    /// <summary>
    /// Indica si se debe ignorar la propiedad ReadOnly.
    /// </summary>
    public bool IgnoreReadOnly { get; set; }

    /// <summary>
    /// Inicializa una nueva instancia del control de inicio de sesión.
    /// Configura los manejadores de eventos y la interfaz de usuario.
    /// </summary>
    public Login()
    {
        Conexion coreConexion = new Conexion();
        InitializeComponent();


        Load += (_, _) => { InitializeConnections(); };

        comboConexiones.SelectedValueChanged += (_, _) =>
        {
            coreConexion = (Conexion)comboConexiones.SelectedItem;

            if (coreConexion != null)
            {
                txtServidor.Text = coreConexion.Servidor;
                txtUsuario.Text = coreConexion.Usuario;
                txtContraseña.Text = coreConexion.Contrasenia;
                txtBaseDatos.Text = coreConexion.BaseDatos;
                chkPrincipal.Checked = coreConexion.Principal;

                SelectedIndex?.Invoke(coreConexion);
            }
        };

        btnAgregar.Click += (_, _) =>
        {
            coreConexion = new Conexion
            {
                Servidor = txtServidor.Text,
                BaseDatos = txtBaseDatos.Text,
                Usuario = txtUsuario.Text,
                Contrasenia = txtContraseña.Text,
                Principal = chkPrincipal.Checked
            };
            int conexionId = coreConexion.IdConexion;

            if (coreConexion.Principal)
            {
                Connections.ForEach(conexion => conexion.Principal = false);
            }

            if (string.IsNullOrEmpty(coreConexion.Servidor) == false && string.IsNullOrEmpty(coreConexion.BaseDatos) == false)
            {
                int index = Connections.FindIndex(conexion => conexion.IdConexion == conexionId + 1);

                if (index > -1)
                {
                    coreConexion.IdConexion = conexionId;
                }
                else
                {
                    coreConexion.IdConexion = Connections.Count;
                }

                Connections.Add(coreConexion);

                File.WriteAllText(DirectorioSistema, JsonSerializer.Serialize(Connections));

                comboConexiones.Properties.Items.Clear();
                comboConexiones.Properties.Items.AddRange(Connections.OrderBy(p => p.Servidor).ToList());


                comboConexiones.SelectedItem = index > 0 ? Connections[index] : Connections.LastOrDefault();
            }
        };

        btnEliminar.Click += async (_, _) =>
        {
            if (comboConexiones.SelectedItem is Conexion conexion)
            {
                txtServidor.Text = conexion.Servidor;
                txtBaseDatos.Text = conexion.BaseDatos;
                txtUsuario.Text = conexion.Usuario;
                txtContraseña.Text = conexion.Contrasenia;

                Connections.Remove(conexion);
                await Task.Run(() => File.WriteAllText(DirectorioSistema, JsonSerializer.Serialize(Connections)));

                comboConexiones.Properties.Items.Clear();
                comboConexiones.Properties.Items.AddRange(Connections);
                comboConexiones.SelectedItem = Connections.LastOrDefault();
                Clear();
            }
        };
        comboConexiones.Focus();
        btnNuevo.Click += (_, _) => Clear();
    }

    /// <summary>
    /// Obtiene la ruta del archivo donde se guardan las conexiones.
    /// </summary>
    public string DirectorioSistema => $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "DirectorioSistema.json")}";

    /// <summary>
    /// Inicializa las conexiones cargándolas desde el archivo de configuración.
    /// Si el archivo no existe, inicializa una lista vacía.
    /// </summary>
    public void InitializeConnections()
    {
        if (File.Exists(DirectorioSistema))
        {
            string json = File.ReadAllText(DirectorioSistema);
            Connections = JsonSerializer.Deserialize<List<Conexion>>(json);
        }

        if (Connections != null)
        {
            foreach (Conexion item in Connections.OrderBy(conexion => conexion.Servidor))
                comboConexiones.Properties.Items.Add(item);
        }

        if (Connections != null)
        {
            comboConexiones.SelectedItem = Connections.FirstOrDefault(conexion => conexion.Principal);
        }
    }


    /// <summary>
    /// Limpia los campos del formulario y restablece los valores predeterminados.
    /// </summary>
    public void Clear()
    {
        Conexion conexion = new Conexion();
        txtServidor.Text = conexion.Servidor;
        txtUsuario.Text = conexion.Usuario;
        txtContraseña.Text = conexion.Contrasenia;
        txtBaseDatos.Text = conexion.BaseDatos;
        chkPrincipal.Checked = false;
        comboConexiones.SelectedIndex = -1;

        txtServidor.Focus();
    }
}