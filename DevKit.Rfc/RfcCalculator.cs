namespace DevKit.Rfc
{
    /// <summary>
    /// Calculador de RFC (Registro Federal de Contribuyentes).
    /// Implementa el algoritmo oficial del SAT para personas físicas y morales.
    /// </summary>
    public class RfcCalculator
    {
        private readonly bool _generarHomoclave;

        /// <summary>
        /// Inicializa una nueva instancia del calculador de RFC.
        /// </summary>
        /// <param name="generarHomoclave">Indica si se debe generar homoclave.</param>
        public RfcCalculator(bool generarHomoclave)
        {
            _generarHomoclave = generarHomoclave;
        }

        /// <summary>
        /// Calcula el RFC para una persona física.
        /// </summary>
        /// <param name="nombre">Nombre de pila.</param>
        /// <param name="apellidoPaterno">Apellido paterno.</param>
        /// <param name="apellidoMaterno">Apellido materno.</param>
        /// <param name="fechaNacimiento">Fecha de nacimiento.</param>
        /// <returns>RFC calculado.</returns>
        public ValueObjects.Rfc CalcularRfcPersonaFisica(
            string nombre,
            string apellidoPaterno,
            string apellidoMaterno,
            DateTime fechaNacimiento)
        {
            ValidarParametrosPersonaFisica(nombre, apellidoPaterno, fechaNacimiento);

            DatosPersonaFisica datosNormalizados = NormalizarDatosPersonaFisica(nombre, apellidoPaterno, apellidoMaterno);
            string rfcBase = ConstruirRfcBasePersonaFisica(datosNormalizados);
            string rfcCompleto = CompletarRfcPersonaFisica(rfcBase, datosNormalizados, fechaNacimiento);

            return ValueObjects.Rfc.Crear(rfcCompleto);
        }

        /// <summary>
        /// Calcula el RFC para una persona moral.
        /// </summary>
        /// <param name="razonSocial">Razón social.</param>
        /// <param name="fechaConstitucion">Fecha de constitución.</param>
        /// <returns>RFC calculado.</returns>
        public ValueObjects.Rfc CalcularRfcPersonaMoral(string razonSocial, DateTime fechaConstitucion)
        {
            ValidarParametrosPersonaMoral(razonSocial, fechaConstitucion);

            string razonSocialNormalizada = NormalizarRazonSocial(razonSocial);
            string rfcBase = ConstruirRfcBasePersonaMoral(razonSocialNormalizada);
            string rfcCompleto = CompletarRfcPersonaMoral(rfcBase, razonSocialNormalizada, fechaConstitucion);

            return ValueObjects.Rfc.Crear(rfcCompleto);
        }

        private void ValidarParametrosPersonaFisica(string nombre, string apellidoPaterno, DateTime fechaNacimiento)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es requerido.", nameof(nombre));

            if (string.IsNullOrWhiteSpace(apellidoPaterno))
                throw new ArgumentException("El apellido paterno es requerido.", nameof(apellidoPaterno));

            if (fechaNacimiento == default)
                throw new ArgumentException("La fecha de nacimiento es requerida.", nameof(fechaNacimiento));
        }

        private void ValidarParametrosPersonaMoral(string razonSocial, DateTime fechaConstitucion)
        {
            if (string.IsNullOrWhiteSpace(razonSocial))
                throw new ArgumentException("La razón social es requerida.", nameof(razonSocial));

            if (fechaConstitucion == default)
                throw new ArgumentException("La fecha de constitución es requerida.", nameof(fechaConstitucion));
        }

        /// <summary>
        /// Normaliza los datos de una persona física para el cálculo de RFC.
        /// </summary>
        /// <param name="nombre">Nombre de pila.</param>
        /// <param name="apellidoPaterno">Apellido paterno.</param>
        /// <param name="apellidoMaterno">Apellido materno.</param>
        /// <returns>Datos normalizados para cálculo.</returns>
        public DatosPersonaFisica NormalizarDatosPersonaFisica(string nombre, string apellidoPaterno, string apellidoMaterno)
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

            return new DatosPersonaFisica
            {
                Nombre = nombreCompuesto,
                ApellidoPaterno = paternoCompuesto,
                ApellidoMaterno = maternoCompuesto,
                NombreOriginal = nombreUpper,
                PaternoOriginal = paternoUpper,
                MaternoOriginal = maternoUpper
            };
        }

        private string NormalizarRazonSocial(string razonSocial)
        {
            string razonSocialUpper = EliminarAcentos(razonSocial.ToUpper());
            string razonSocialFiltrada = FiltrarPalabrasComunes(razonSocialUpper);
            string razonSocialCompuesta = FiltrarNombresCompuestos(razonSocialFiltrada);
            return razonSocialCompuesta;
        }

        /// <summary>
        /// Construye la base del RFC para personas físicas (sin fecha ni homoclave).
        /// </summary>
        /// <param name="datos">Datos normalizados de la persona.</param>
        /// <returns>Base del RFC (4 caracteres).</returns>
        public string ConstruirRfcBasePersonaFisica(DatosPersonaFisica datos)
        {
            StringBuilder rfc = new System.Text.StringBuilder();

            // Primera letra del apellido paterno
            rfc.Append(datos.ApellidoPaterno[0]);

            // Primera vocal interna del apellido paterno
            rfc.Append(ObtenerVocalInterna(datos.ApellidoPaterno));

            // Primera letra del apellido materno o X si no existe
            rfc.Append(string.IsNullOrEmpty(datos.ApellidoMaterno) ? "X" : datos.ApellidoMaterno[0].ToString());

            // Primera letra del nombre
            rfc.Append(datos.Nombre[0]);

            // Quitar palabras prohibidas
            string rfcBase = rfc.ToString();
            return QuitarPalabrasProhibidas(rfcBase);
        }

        private string ConstruirRfcBasePersonaMoral(string razonSocial)
        {
            // Tomar las primeras 3 letras de la razón social
            string rfcBase = razonSocial.Length >= 3 ? razonSocial.Substring(0, 3) : razonSocial.PadRight(3, 'X');
            return QuitarPalabrasProhibidas(rfcBase);
        }

        private string CompletarRfcPersonaFisica(string rfcBase, DatosPersonaFisica datos, DateTime fechaNacimiento)
        {
            StringBuilder rfc = new System.Text.StringBuilder(rfcBase);

            // Agregar fecha de nacimiento
            rfc.Append(ObtenerFechaRfc(fechaNacimiento));

            if (_generarHomoclave)
            {
                // Agregar homoclave
                string homoclave = ObtenerHomonimia(datos.PaternoOriginal, datos.MaternoOriginal, datos.NombreOriginal);
                rfc.Append(homoclave);

                // Agregar dígito verificador
                string rfcConDigito = rfc.ToString();
                string digitoVerificador = ObtenerDigitoVerificadorRfc(rfcConDigito);
                rfc.Append(digitoVerificador);
            }
            else
            {
                rfc.Append("000");
            }

            return rfc.ToString();
        }

        private string CompletarRfcPersonaMoral(string rfcBase, string razonSocial, DateTime fechaConstitucion)
        {
            StringBuilder rfc = new System.Text.StringBuilder(rfcBase);

            // Agregar fecha de constitución
            rfc.Append(ObtenerFechaRfc(fechaConstitucion));

            if (_generarHomoclave)
            {
                // Agregar homoclave
                string homoclave = ObtenerHomonimiaPersonaMoral(razonSocial);
                rfc.Append(homoclave);

                // Agregar dígito verificador
                string rfcConDigito = rfc.ToString();
                string digitoVerificador = ObtenerDigitoVerificadorRfc(rfcConDigito);
                rfc.Append(digitoVerificador);
            }
            else
            {
                rfc.Append("000");
            }

            return rfc.ToString();
        }

        /// <summary>
        /// Calcula la homoclave para RFC de personas físicas.
        /// </summary>
        /// <param name="paterno">Apellido paterno original.</param>
        /// <param name="materno">Apellido materno original.</param>
        /// <param name="nombre">Nombre original.</param>
        /// <returns>Homoclave de 2 caracteres.</returns>
        public string ObtenerHomonimia(string paterno, string materno, string nombre)
        {
            string nombreCompleto = paterno.Trim() + " " + materno.Trim() + " " + nombre.Trim();
            string numero = "0";
            string letra = "";
            string numero1 = "";
            string numero2 = "";
            int numeroSuma = 0;
            for (int i = 0; i < nombreCompleto.Length; i++)
            {
                letra = nombreCompleto.Substring(i, 1);
                switch (letra)
                {
                    case "&":
                        numero = numero + "10";
                        break;
                    case "A":
                        numero = numero + "11";
                        break;
                    case "B":
                        numero = numero + "12";
                        break;
                    case "C":
                        numero = numero + "13";
                        break;
                    case "D":
                        numero = numero + "14";
                        break;
                    case "E":
                        numero = numero + "15";
                        break;
                    case "F":
                        numero = numero + "16";
                        break;
                    case "G":
                        numero = numero + "17";
                        break;
                    case "H":
                        numero = numero + "18";
                        break;
                    case "I":
                        numero = numero + "19";
                        break;
                    case "J":
                        numero = numero + "21";
                        break;
                    case "K":
                        numero = numero + "22";
                        break;
                    case "L":
                        numero = numero + "23";
                        break;
                    case "M":
                        numero = numero + "24";
                        break;
                    case "N":
                        numero = numero + "25";
                        break;
                    case "O":
                        numero = numero + "26";
                        break;
                    case "P":
                        numero = numero + "27";
                        break;
                    case "Q":
                        numero = numero + "28";
                        break;
                    case "R":
                        numero = numero + "29";
                        break;
                    case "S":
                        numero = numero + "32";
                        break;
                    case "T":
                        numero = numero + "33";
                        break;
                    case "U":
                        numero = numero + "34";
                        break;
                    case "V":
                        numero = numero + "35";
                        break;
                    case "W":
                        numero = numero + "36";
                        break;
                    case "X":
                        numero = numero + "37";
                        break;
                    case "Y":
                        numero = numero + "38";
                        break;
                    case "Z":
                        numero = numero + "39";
                        break;
                    case " ":
                        numero = numero + "00";
                        break;
                }
            }

            // Calcular sumatoria según el algoritmo original
            for (int i = 0; i < numero.Length; i++)
            {
                numero1 = ((i + 1) == numero.Length) ? "0" : numero.Substring(i, 2);
                numero2 = (i == numero.Length - 1) ? "0" : numero.Substring(i + 1, 1);
                numeroSuma = numeroSuma + (int.Parse(numero1) * int.Parse(numero2));
            }

            int numero3 = numeroSuma % 1000;
            int numero4 = numero3 / 34;
            string numero5 = numero4.ToString().Split('.')[0];
            int numero6 = numero3 % 34;
            string homonimio = "";

            // Asignar valores según el algoritmo original
            switch (numero5)
            {
                case "0": homonimio = "1"; break;
                case "1": homonimio = "2"; break;
                case "2": homonimio = "3"; break;
                case "3": homonimio = "4"; break;
                case "4": homonimio = "5"; break;
                case "5": homonimio = "6"; break;
                case "6": homonimio = "7"; break;
                case "7": homonimio = "8"; break;
                case "8": homonimio = "9"; break;
                case "9": homonimio = "A"; break;
                case "10": homonimio = "B"; break;
                case "11": homonimio = "C"; break;
                case "12": homonimio = "D"; break;
                case "13": homonimio = "E"; break;
                case "14": homonimio = "F"; break;
                case "15": homonimio = "G"; break;
                case "16": homonimio = "H"; break;
                case "17": homonimio = "I"; break;
                case "18": homonimio = "J"; break;
                case "19": homonimio = "K"; break;
                case "20": homonimio = "L"; break;
                case "21": homonimio = "M"; break;
                case "22": homonimio = "N"; break;
                case "23": homonimio = "P"; break;
                case "24": homonimio = "Q"; break;
                case "25": homonimio = "R"; break;
                case "26": homonimio = "S"; break;
                case "27": homonimio = "T"; break;
                case "28": homonimio = "U"; break;
                case "29": homonimio = "V"; break;
                case "30": homonimio = "W"; break;
                case "31": homonimio = "X"; break;
                case "32": homonimio = "Y"; break;
                case "33": homonimio = "Z"; break;
            }

            switch (numero6.ToString())
            {
                case "0": homonimio = homonimio + "1"; break;
                case "1": homonimio = homonimio + "2"; break;
                case "2": homonimio = homonimio + "3"; break;
                case "3": homonimio = homonimio + "4"; break;
                case "4": homonimio = homonimio + "5"; break;
                case "5": homonimio = homonimio + "6"; break;
                case "6": homonimio = homonimio + "7"; break;
                case "7": homonimio = homonimio + "8"; break;
                case "8": homonimio = homonimio + "9"; break;
                case "9": homonimio = homonimio + "A"; break;
                case "10": homonimio = homonimio + "B"; break;
                case "11": homonimio = homonimio + "C"; break;
                case "12": homonimio = homonimio + "D"; break;
                case "13": homonimio = homonimio + "E"; break;
                case "14": homonimio = homonimio + "F"; break;
                case "15": homonimio = homonimio + "G"; break;
                case "16": homonimio = homonimio + "H"; break;
                case "17": homonimio = homonimio + "I"; break;
                case "18": homonimio = homonimio + "J"; break;
                case "19": homonimio = homonimio + "K"; break;
                case "20": homonimio = homonimio + "L"; break;
                case "21": homonimio = homonimio + "M"; break;
                case "22": homonimio = homonimio + "N"; break;
                case "23": homonimio = homonimio + "P"; break;
                case "24": homonimio = homonimio + "Q"; break;
                case "25": homonimio = homonimio + "R"; break;
                case "26": homonimio = homonimio + "S"; break;
                case "27": homonimio = homonimio + "T"; break;
                case "28": homonimio = homonimio + "U"; break;
                case "29": homonimio = homonimio + "V"; break;
                case "30": homonimio = homonimio + "W"; break;
                case "31": homonimio = homonimio + "X"; break;
                case "32": homonimio = homonimio + "Y"; break;
                case "33": homonimio = homonimio + "Z"; break;
            }
            return homonimio;
        }

        private string ObtenerHomonimiaPersonaMoral(string razonSocial)
        {
            string numero = "0";
            int numeroSuma = 0;

            // Asignar valores numéricos a cada carácter
            for (int i = 0; i < razonSocial.Length; i++)
            {
                string letra = razonSocial.Substring(i, 1).ToUpper();
                numero += ObtenerValorCaracterHomonimia(letra);
            }

            // Calcular sumatoria
            for (int i = 0; i < numero.Length - 1; i++)
            {
                string numero1 = i == numero.Length - 1 ? "0" : numero.Substring(i, 2);
                string numero2 = i == numero.Length - 2 ? "0" : numero.Substring(i + 1, 1);
                numeroSuma += int.Parse(numero1) * int.Parse(numero2);
            }

            // Calcular homonimia
            int resultado = numeroSuma % 1000;
            int cociente = resultado / 34;
            int residuo = resultado % 34;

            return ConvertirDigitoHomonimia(cociente) + ConvertirDigitoHomonimia(residuo);
        }

        private string ObtenerValorCaracterHomonimia(string caracter)
        {
            return caracter switch
            {
                " " => "00",
                "&" => "10",
                "0" => "00",
                "1" => "01",
                "2" => "02",
                "3" => "03",
                "4" => "04",
                "5" => "05",
                "6" => "06",
                "7" => "07",
                "8" => "08",
                "9" => "09",
                "A" => "11",
                "B" => "12",
                "C" => "13",
                "D" => "14",
                "E" => "15",
                "F" => "16",
                "G" => "17",
                "H" => "18",
                "I" => "19",
                "J" => "21",
                "K" => "22",
                "L" => "23",
                "M" => "24",
                "N" => "25",
                "Ñ" => "40",
                "O" => "26",
                "P" => "27",
                "Q" => "28",
                "R" => "29",
                "S" => "32",
                "T" => "33",
                "U" => "34",
                "V" => "35",
                "W" => "36",
                "X" => "37",
                "Y" => "38",
                "Z" => "39",
                _ => "00"
            };
        }

        private string ConvertirDigitoHomonimia(int digito)
        {
            return digito switch
            {
                0 => "1",
                1 => "2",
                2 => "3",
                3 => "4",
                4 => "5",
                5 => "6",
                6 => "7",
                7 => "8",
                8 => "9",
                9 => "A",
                10 => "B",
                11 => "C",
                12 => "D",
                13 => "E",
                14 => "F",
                15 => "G",
                16 => "H",
                17 => "I",
                18 => "J",
                19 => "K",
                20 => "L",
                21 => "M",
                22 => "N",
                23 => "P",
                24 => "Q",
                25 => "R",
                26 => "S",
                27 => "T",
                28 => "U",
                29 => "V",
                30 => "W",
                31 => "X",
                32 => "Y",
                33 => "Z",
                _ => "1"
            };
        }

        /// <summary>
        /// Calcula el dígito verificador para RFC.
        /// </summary>
        /// <param name="rfc">RFC sin dígito verificador.</param>
        /// <returns>Dígito verificador.</returns>
        public string ObtenerDigitoVerificadorRfc(string rfc)
        {
            List<string> rfcsuma = new System.Collections.Generic.List<string>();
            int nv = 0;
            int y = 0;

            for (int i = 0; i < rfc.Length; i++)
            {
                string letra = rfc.Substring(i, 1);

                switch (letra)
                {
                    case "0": rfcsuma.Add("00"); break;
                    case "1": rfcsuma.Add("01"); break;
                    case "2": rfcsuma.Add("02"); break;
                    case "3": rfcsuma.Add("03"); break;
                    case "4": rfcsuma.Add("04"); break;
                    case "5": rfcsuma.Add("05"); break;
                    case "6": rfcsuma.Add("06"); break;
                    case "7": rfcsuma.Add("07"); break;
                    case "8": rfcsuma.Add("08"); break;
                    case "9": rfcsuma.Add("09"); break;
                    case "A": rfcsuma.Add("10"); break;
                    case "B": rfcsuma.Add("11"); break;
                    case "C": rfcsuma.Add("12"); break;
                    case "D": rfcsuma.Add("13"); break;
                    case "E": rfcsuma.Add("14"); break;
                    case "F": rfcsuma.Add("15"); break;
                    case "G": rfcsuma.Add("16"); break;
                    case "H": rfcsuma.Add("17"); break;
                    case "I": rfcsuma.Add("18"); break;
                    case "J": rfcsuma.Add("19"); break;
                    case "K": rfcsuma.Add("20"); break;
                    case "L": rfcsuma.Add("21"); break;
                    case "M": rfcsuma.Add("22"); break;
                    case "N": rfcsuma.Add("23"); break;
                    case "Ñ": rfcsuma.Add("24"); break;
                    case "O": rfcsuma.Add("25"); break;
                    case "P": rfcsuma.Add("26"); break;
                    case "Q": rfcsuma.Add("27"); break;
                    case "R": rfcsuma.Add("28"); break;
                    case "S": rfcsuma.Add("29"); break;
                    case "T": rfcsuma.Add("30"); break;
                    case "U": rfcsuma.Add("31"); break;
                    case "V": rfcsuma.Add("32"); break;
                    case "W": rfcsuma.Add("33"); break;
                    case "X": rfcsuma.Add("34"); break;
                    case "Y": rfcsuma.Add("35"); break;
                    case "Z": rfcsuma.Add("36"); break;
                    case " ": rfcsuma.Add("37"); break;
                    default: rfcsuma.Add("00"); break;
                }
            }

            for (int i = 13; i > 1; i--)
            {
                nv += (rfcsuma.Count == y) ? 0 : (int.Parse(rfcsuma[y]) * i);
                y++;
            }

            nv = nv % 11;
            if (nv == 0)
                return "0";
            else if (nv <= 10)
            {
                nv = 11 - nv;
                return nv.ToString() == "10" ? "A" : nv.ToString();
            }
            else if (nv.ToString() == "10")
            {
                return "A";
            }
            return "0";
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
                    case "CH": return nombre.Replace("CH", "C");
                    case "LL": return nombre.Replace("LL", "L");
                    case "TR": return nombre.Replace("TR", "T");
                }
            }
            return nombre;
        }

        private string ObtenerVocalInterna(string cadena)
        {
            string[] vocales = new[] { "A", "E", "I", "O", "U" };

            if (string.IsNullOrEmpty(cadena))
                return "X";

            for (int i = 1; i < cadena.Length; i++)
            {
                if (Array.Exists(vocales, vocal => vocal == cadena[i].ToString()))
                {
                    return cadena[i].ToString();
                }
            }
            return "X";
        }

        private string QuitarPalabrasProhibidas(string rfc)
        {
            string palabrasAltisonantes = ObtenerPalabrasAltisonantes();
            Regex regex = new Regex(rfc);
            Match match = regex.Match(palabrasAltisonantes);

            return match.Success
                ? rfc.Substring(0, 1) + "X" + rfc.Substring(2, 2)
                : rfc;
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

        /// <summary>
        /// Obtiene la fecha en formato yyMMdd para RFC.
        /// </summary>
        /// <param name="fecha">Fecha a formatear.</param>
        /// <returns>Fecha en formato yyMMdd.</returns>
        public string ObtenerFechaRfc(DateTime fecha)
        {
            string anio = fecha.Year.ToString().Substring(2, 2);
            string mes = fecha.Month.ToString().Length == 1 ? "0" + fecha.Month : fecha.Month.ToString();
            string dia = fecha.Day.ToString().Length == 1 ? "0" + fecha.Day : fecha.Day.ToString();
            return anio + mes + dia;
        }

        /// <summary>
        /// Datos normalizados de una persona física para el cálculo de RFC.
        /// </summary>
        public class DatosPersonaFisica
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
