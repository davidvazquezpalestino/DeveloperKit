namespace DevKit.Rfc
{
    /// <summary>
    /// Calculador de CURP para personas físicas.
    /// Implementa el algoritmo oficial del gobierno mexicano.
    /// </summary>
    public class CurpCalculator
    {
        private static readonly string[] Vocales = { "A", "E", "I", "O", "U" };

        /// <summary>
        /// Calcula la CURP para una persona física.
        /// </summary>
        /// <param name="nombre">Nombre de pila.</param>
        /// <param name="apellidoPaterno">Apellido paterno.</param>
        /// <param name="apellidoMaterno">Apellido materno.</param>
        /// <param name="fechaNacimiento">Fecha de nacimiento.</param>
        /// <param name="sexo">Sexo (H o M).</param>
        /// <param name="entidadFederativa">Código de entidad federativa (2 caracteres).</param>
        /// <returns>CURP calculada.</returns>
        public ValueObjects.Curp CalcularCurp(
            string nombre,
            string apellidoPaterno,
            string apellidoMaterno,
            DateTime fechaNacimiento,
            string sexo,
            string entidadFederativa)
        {
            ValidarParametros(nombre, apellidoPaterno, fechaNacimiento, sexo, entidadFederativa);

            DatosNormalizados datosNormalizados = NormalizarDatos(nombre, apellidoPaterno, apellidoMaterno);
            string curpBase = ConstruirCurpBase(datosNormalizados, sexo, entidadFederativa);
            string curpCompleta = AgregarConsonantesYDigito(curpBase, datosNormalizados, fechaNacimiento);

            return ValueObjects.Curp.Crear(curpCompleta);
        }

        private void ValidarParametros(
            string nombre,
            string apellidoPaterno,
            DateTime fechaNacimiento,
            string sexo,
            string entidadFederativa)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es requerido.", nameof(nombre));

            if (string.IsNullOrWhiteSpace(apellidoPaterno))
                throw new ArgumentException("El apellido paterno es requerido.", nameof(apellidoPaterno));

            if (fechaNacimiento == default)
                throw new ArgumentException("La fecha de nacimiento es requerida.", nameof(fechaNacimiento));

            if (string.IsNullOrWhiteSpace(sexo) || (sexo != "H" && sexo != "M"))
                throw new ArgumentException("El sexo debe ser 'H' o 'M'.", nameof(sexo));

            if (string.IsNullOrWhiteSpace(entidadFederativa) || entidadFederativa.Length != 2)
                throw new ArgumentException("La entidad federativa debe tener 2 caracteres.", nameof(entidadFederativa));
        }

        private DatosNormalizados NormalizarDatos(string nombre, string apellidoPaterno, string apellidoMaterno)
        {
            string nombreUpper = EliminarAcentos(nombre.ToUpper());
            string paternoUpper = EliminarAcentos(apellidoPaterno.ToUpper());
            string maternoUpper = EliminarAcentos(apellidoMaterno.ToUpper());

            string nombreFiltrado = FiltrarPalabrasComunes(nombreUpper);
            string paternoFiltrado = FiltrarPalabrasComunes(paternoUpper);
            string maternoFiltrado = FiltrarPalabrasComunes(maternoUpper);

            string nombreCompuesto = FiltrarNombresCompuestos(nombreFiltrado);
            string paternoCompuesto = FiltrarNombresCompuestos(paternoFiltrado);
            string maternoCompuesto = FiltrarNombresCompuestos(maternoFiltrado);

            return new DatosNormalizados
            {
                Nombre = nombreCompuesto,
                ApellidoPaterno = paternoCompuesto,
                ApellidoMaterno = maternoCompuesto,
                NombreOriginal = nombreUpper,
                PaternoOriginal = paternoUpper,
                MaternoOriginal = maternoUpper
            };
        }

        private string ConstruirCurpBase(DatosNormalizados datos, string sexo, string entidadFederativa)
        {
            StringBuilder curp = new StringBuilder();

            // Primera letra del apellido paterno
            curp.Append(datos.ApellidoPaterno[0]);

            // Primera vocal interna del apellido paterno
            curp.Append(ObtenerVocalInterna(datos.ApellidoPaterno));

            // Primera letra del apellido materno o X si no existe
            curp.Append(string.IsNullOrEmpty(datos.ApellidoMaterno) ? "X" : datos.ApellidoMaterno[0].ToString());

            // Primera letra del nombre
            curp.Append(datos.Nombre[0]);

            // Fecha de nacimiento
            curp.Append(ObtenerFechaNacimientoCurp(DateTime.Now)); // Se reemplazará en el método principal

            // Sexo y entidad federativa
            curp.Append(sexo);
            curp.Append(entidadFederativa);

            // Quitar palabras prohibidas
            string curpBase = curp.ToString();
            return QuitarPalabrasProhibidas(curpBase);
        }

        private string AgregarConsonantesYDigito(string curpBase, DatosNormalizados datos, DateTime fechaNacimiento)
        {
            StringBuilder curp = new System.Text.StringBuilder(curpBase);

            // Reemplazar fecha temporal con la real
            string fechaStr = ObtenerFechaNacimientoCurp(fechaNacimiento);
            curp.Remove(4, 6).Insert(4, fechaStr);

            // Agregar consonantes internas
            curp.Append(ObtenerConsonanteInterna(datos.ApellidoPaterno));
            curp.Append(ObtenerConsonanteInterna(datos.ApellidoMaterno));
            curp.Append(ObtenerConsonanteInterna(datos.Nombre));

            // Agregar dígito verificador
            string curpConDigito = curp.ToString();
            string digitoVerificador = ObtenerDigitoVerificadorCurp(fechaNacimiento.Year, curpConDigito);
            curp.Append(digitoVerificador);

            return curp.ToString();
        }

        private string EliminarAcentos(string cadena)
        {
            return Regex.Replace(cadena, "Á", "A")
                        .Replace("É", "E")
                        .Replace("Í", "I")
                        .Replace("Ó", "O")
                        .Replace("Ú", "U")
                        .Replace("Ñ", "X");
        }

        private string FiltrarPalabrasComunes(string nombre)
        {
            string[] palabrasComunes = new[]
            {
                ",", "de ", "del ", "la ", "los ", "las ", "y ", "mc ", "mac ", "von ", "van ",
                "DE ", "DEL ", "LA ", "LOS ", "LAS ", "Y ", "MC ", "MAC ", "VON ", "VAN ",
                "MA.", "MA. ", "ma ", "MA "
            };

            foreach (string palabra in palabrasComunes)
            {
                nombre = nombre.Replace(palabra, "");
            }

            string[] nombresComunes = new[]
            {
                "JOSE ", "MARIA ", "J ", "MA ", "JOSÉ ", "MARÍA ", "J. ", "MA.",
                "MARÍA LOS", "MARIA LOS", "M ", "M. ", "M.", "J."
            };

            foreach (string nombreComun in nombresComunes)
            {
                string[] nombres = nombre.Split(' ');
                if (nombre != nombreComun && nombres.Length != 1 && !string.IsNullOrEmpty(nombres[1]))
                {
                    nombre = nombre.Replace(nombreComun, "");
                }
            }

            return nombre.Replace(" ", "");
        }

        private string FiltrarNombresCompuestos(string nombre)
        {
            if (nombre.Length > 1)
            {
                switch (nombre.Substring(0, 2))
                {
                    case "CH":
                        return nombre.Replace("CH", "C");
                    case "LL":
                        return nombre.Replace("LL", "L");
                    case "TR":
                        return nombre.Replace("TR", "T");
                }
            }
            return nombre;
        }

        private string ObtenerVocalInterna(string cadena)
        {
            if (string.IsNullOrEmpty(cadena))
                return "X";

            for (int i = 1; i < cadena.Length; i++)
            {
                if (Array.Exists(Vocales, vocal => vocal == cadena[i].ToString()))
                {
                    return cadena[i].ToString();
                }
            }
            return "X";
        }

        private string ObtenerConsonanteInterna(string cadena)
        {
            if (string.IsNullOrEmpty(cadena))
                return "X";

            for (int i = 1; i < cadena.Length; i++)
            {
                if (!Array.Exists(Vocales, vocal => vocal == cadena[i].ToString()))
                {
                    return cadena[i].ToString();
                }
            }
            return "X";
        }

        private string QuitarPalabrasProhibidas(string curp)
        {
            string palabrasAltisonantes = ObtenerPalabrasAltisonantes();
            Regex regex = new Regex(curp);
            Match match = regex.Match(palabrasAltisonantes);

            return match.Success
                ? curp.Substring(0, 1) + "X" + curp.Substring(2, 2)
                : curp;
        }

        private string ObtenerPalabrasAltisonantes()
        {
            string palabras = "BACA*BAKA*BUEI*BUEY*CACA*CACO*CAGA*CAGO*CAKA*CAKO*COGE*COGI*COJA*COJE*COJI*COJO*COLA*CULO*";
            palabras += "FALO*FETO*GETA*GUEI*GUEY*JETA*JOTO*";
            palabras += "KOGE*KOJO*KAKA*KULO*MAME*MAMO*MEAR*";
            palabras += "MEAS*MEON*MION*COJE*COJI*COJO*CULO*";
            palabras += "KACA*KACO*KAGA*KAGO*KAKO*KOGI*KOJA*KOJE*KOJI*KOLA*LILO*LOCA*LOCO*LOKA*LOKO*";
            palabras += "MIAR*MOKO*MULO*NACO*NACA*PIPI*PITO*POPO*ROBA*ROBE*ROBO*SENO*TETA*VACA*VAGA*VAGO*VAKA*VUEI*VUEY*WUEI*WUEY*";
            palabras += "MOCO*MULA*PEDA*PEDO*PENE*PUTA*PUTO*";
            palabras += "QULO*RATA*RUIN*";
            return palabras;
        }

        private string ObtenerFechaNacimientoCurp(DateTime fecha)
        {
            string anio = fecha.Year.ToString().Substring(2, 2);
            string mes = fecha.Month.ToString().Length == 1 ? "0" + fecha.Month : fecha.Month.ToString();
            string dia = fecha.Day.ToString().Length == 1 ? "0" + fecha.Day : fecha.Day.ToString();
            return anio + mes + dia;
        }

        private string ObtenerDigitoVerificadorCurp(int anio, string curp)
        {
            int contador = 18;
            int sumatoria = 0;

            for (int i = 0; i <= 15; i++)
            {
                string caracter = curp.Substring(i, 1);
                int valor = ObtenerValorCaracter(caracter) * contador;
                sumatoria += valor;
                contador--;
            }

            int numVer = sumatoria % 10;
            numVer = Math.Abs(10 - numVer);

            if (numVer == 10)
                numVer = 0;

            return anio < 2000 ? "0" + numVer : "A" + numVer;
        }

        private int ObtenerValorCaracter(string caracter)
        {
            return caracter switch
            {
                "0" => 0,
                "1" => 1,
                "2" => 2,
                "3" => 3,
                "4" => 4,
                "5" => 5,
                "6" => 6,
                "7" => 7,
                "8" => 8,
                "9" => 9,
                "A" => 10,
                "B" => 11,
                "C" => 12,
                "D" => 13,
                "E" => 14,
                "F" => 15,
                "G" => 16,
                "H" => 17,
                "I" => 18,
                "J" => 19,
                "K" => 20,
                "L" => 21,
                "M" => 22,
                "N" => 23,
                "Ñ" => 24,
                "O" => 25,
                "P" => 26,
                "Q" => 27,
                "R" => 28,
                "S" => 29,
                "T" => 30,
                "U" => 31,
                "V" => 32,
                "W" => 33,
                "X" => 34,
                "Y" => 35,
                "Z" => 36,
                _ => 0
            };
        }

        /// <summary>
        /// Datos normalizados para el cálculo de CURP.
        /// </summary>
        private class DatosNormalizados
        {
            /// <summary>
            /// Nombre normalizado de la persona.
            /// </summary>
            public string Nombre { get; set; }
            /// <summary>
            /// Apellido paterno normalizado.
            /// </summary>
            public string ApellidoPaterno { get; set; }
            /// <summary>
            /// Apellido materno normalizado.
            /// </summary>
            public string ApellidoMaterno { get; set; }
            /// <summary>
            /// Nombre original sin normalizar.
            /// </summary>
            public string NombreOriginal { get; set; }
            /// <summary>
            /// Apellido paterno original sin normalizar.
            /// </summary>
            public string PaternoOriginal { get; set; }
            /// <summary>
            /// Apellido materno original sin normalizar.
            /// </summary>
            public string MaternoOriginal { get; set; }
        }
    }
}
