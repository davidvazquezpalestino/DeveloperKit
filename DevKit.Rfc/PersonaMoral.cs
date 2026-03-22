namespace DevKit.Rfc
{
    /// <summary>
    /// Calculador de RFC para personas morales.
    /// Implementa el algoritmo oficial del SAT.
    /// </summary>
    /// <remarks>
    /// Inicializa una nueva instancia del calculador de RFC para personas morales.
    /// </remarks>
    /// <param name="generarHomoclave">Indica si se debe generar homoclave.</param>
    public class PersonaMoralCalculator(bool generarHomoclave)
    {

        /// <summary>
        /// Calcula el RFC para una persona moral.
        /// </summary>
        /// <param name="razonSocial">Razón social.</param>
        /// <param name="fechaConstitucion">Fecha de constitución.</param>
        /// <returns>RFC calculado.</returns>
        public ValueObjects.Rfc CalcularRfcPersonaMoral(string razonSocial, DateTime fechaConstitucion)
        {
            ValidarParametros(razonSocial, fechaConstitucion);

            string razonSocialNormalizada = NormalizarRazonSocial(razonSocial);
            string rfcBase = ConstruirRfcBase(razonSocialNormalizada);
            string rfcCompleto = CompletarRfc(rfcBase, razonSocialNormalizada, fechaConstitucion);

            return ValueObjects.Rfc.Crear(rfcCompleto);
        }

        private void ValidarParametros(string razonSocial, DateTime fechaConstitucion)
        {
            if (string.IsNullOrWhiteSpace(razonSocial))
                throw new ArgumentException("La razón social es requerida.", nameof(razonSocial));

            if (fechaConstitucion == default)
                throw new ArgumentException("La fecha de constitución es requerida.", nameof(fechaConstitucion));
        }

        private string NormalizarRazonSocial(string razonSocial)
        {
            string razonSocialUpper = EliminarAcentos(razonSocial.ToUpper());
            string razonSocialFiltrada = FiltrarPalabrasComunes(razonSocialUpper);
            string razonSocialCompuesta = FiltrarNombresCompuestos(razonSocialFiltrada);
            return razonSocialCompuesta;
        }

        private string ConstruirRfcBase(string razonSocial)
        {
            // Tomar las primeras 3 letras de la razón social
            string rfcBase = razonSocial.Length >= 3 ? razonSocial.Substring(0, 3) : razonSocial.PadRight(3, 'X');
            return QuitarPalabrasProhibidas(rfcBase);
        }

        private string CompletarRfc(string rfcBase, string razonSocial, DateTime fechaConstitucion)
        {
            StringBuilder rfc = new System.Text.StringBuilder(rfcBase);

            // Agregar fecha de constitución
            rfc.Append(ObtenerFechaRfc(fechaConstitucion));

            if (generarHomoclave)
            {
                // Agregar homoclave
                string homoclave = ObtenerHomonimia(razonSocial);
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

        private string ObtenerHomonimia(string razonSocial)
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

        private string ObtenerDigitoVerificadorRfc(string rfc)
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

        private string ObtenerFechaRfc(DateTime fecha)
        {
            string anio = fecha.Year.ToString().Substring(2, 2);
            string mes = fecha.Month.ToString().Length == 1 ? "0" + fecha.Month : fecha.Month.ToString();
            string dia = fecha.Day.ToString().Length == 1 ? "0" + fecha.Day : fecha.Day.ToString();
            return anio + mes + dia;
        }
    }
}
