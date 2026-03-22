using System;
using System.Collections.Generic;
using System.Linq;

namespace DevKit.Rfc
{
    /*******
     * CLASE PARA EL CÁLCULO DEL RFC
     * AUTOR: AIMEE RIOS
     * **********/
    public class Rfc
    {
        private bool _generarHomoclave;
        public Rfc(bool pGenerarHomoclave)
        {
            _generarHomoclave = pGenerarHomoclave;
        }
        public string CalcularRFCPersonaFisica(string pNombre, string pPaterno, string pMaterno, DateTime fecha)
        {
            string Paterno = pPaterno.ToUpper();
            string Materno = pMaterno.ToUpper();
            string Nombre = pNombre.ToUpper();
            if (string.IsNullOrEmpty(Nombre) || string.IsNullOrEmpty(Paterno))
            {
                return "";
            }
            string rfc = GetFechaNacimiento(fecha);
            string FechaNacimiento = rfc;
            //FILTRA ACENTOS
            string PaternoF = RfcFiltraAcentos(Paterno);
            string MaternoF = RfcFiltraAcentos(Materno);
            string NombreF = RfcFiltraAcentos(Nombre);
            //GUARDA NOMBRE ORIGINAL PARA GENERAR HOMOCLAVE
            string PaternoOrig = PaternoF;
            string MaternoOrig = MaternoF;
            string NombreOrig = NombreF;
            //ELIMINA PALABRAS SOBRANTES DE APELLIDOS Y NOMBRE
            PaternoF = RfcFiltraNombres(PaternoF);
            MaternoF = RfcFiltraNombres(MaternoF);
            NombreF = RfcFiltraNombres(Nombre);

            if (PaternoF.Length > 0 && MaternoF.Length > 0)
            {
                if (PaternoF.Length < 3)
                {
                    rfc = RfcApellidoCorto(PaternoF, MaternoF, NombreF);
                }
                else
                {
                    rfc = ArmarRfc(PaternoF, MaternoF, NombreF);
                }
            }

            if (PaternoF.Length == 0 && MaternoF.Length > 0)
            {
                rfc = RfcUnApellido(NombreF, MaternoF);
            }
            if (PaternoF.Length > 0 && MaternoF.Length == 0)
            {
                rfc = RfcUnApellido(NombreF, PaternoF);
            }

            rfc = RfcQuitaProhibidas(rfc);
            rfc = rfc + FechaNacimiento;
            if (_generarHomoclave)//evalua si calcula la homoclave EAS 2018-05-25
            {
                rfc += GetHomonimia(PaternoOrig, MaternoOrig, NombreOrig);
                rfc = rfc + RfcDigitoVerificador(rfc);
            }
            else
                rfc += "000";
            return rfc;
        }

        private string RfcDigitoVerificador(string pRFC)
        {
            System.Collections.Generic.List<string> rfcsuma = new System.Collections.Generic.List<string>();
            int nv = 0;
            int y = 0;
            for (int i = 0; i < pRFC.Length; i++)
            {
                var letra = pRFC.Substring(i, 1);
                //valores para la generación del código verificador del RFC
                switch (letra)
                {
                    case "0":
                        rfcsuma.Add("00");
                        break;
                    case "1":
                        rfcsuma.Add("01");
                        break;
                    case "2":
                        rfcsuma.Add("02");
                        break;
                    case "3":
                        rfcsuma.Add("03");
                        break;
                    case "4":
                        rfcsuma.Add("04");
                        break;
                    case "5":
                        rfcsuma.Add("05");
                        break;
                    case "6":
                        rfcsuma.Add("06");
                        break;
                    case "7":
                        rfcsuma.Add("07");
                        break;
                    case "8":
                        rfcsuma.Add("08");
                        break;
                    case "9":
                        rfcsuma.Add("09");
                        break;
                    case "A":
                        rfcsuma.Add("10");
                        break;
                    case "B":
                        rfcsuma.Add("11");
                        break;
                    case "C":
                        rfcsuma.Add("12");
                        break;
                    case "D":
                        rfcsuma.Add("13");
                        break;
                    case "E":
                        rfcsuma.Add("14");
                        break;
                    case "F":
                        rfcsuma.Add("15");
                        break;
                    case "G":
                        rfcsuma.Add("16");
                        break;
                    case "H":
                        rfcsuma.Add("17");
                        break;
                    case "I":
                        rfcsuma.Add("18");
                        break;
                    case "J":
                        rfcsuma.Add("19");
                        break;
                    case "K":
                        rfcsuma.Add("20");
                        break;
                    case "L":
                        rfcsuma.Add("21");
                        break;
                    case "M":
                        rfcsuma.Add("22");
                        break;
                    case "N":
                        rfcsuma.Add("23");
                        break;
                    case "Ñ":
                        rfcsuma.Add("24");
                        break;
                    case "O":
                        rfcsuma.Add("25");
                        break;
                    case "P":
                        rfcsuma.Add("26");
                        break;
                    case "Q":
                        rfcsuma.Add("27");
                        break;
                    case "R":
                        rfcsuma.Add("28");
                        break;
                    case "S":
                        rfcsuma.Add("29");
                        break;
                    case "T":
                        rfcsuma.Add("30");
                        break;
                    case "U":
                        rfcsuma.Add("31");
                        break;
                    case "V":
                        rfcsuma.Add("32");
                        break;
                    case "W":
                        rfcsuma.Add("33");
                        break;
                    case "X":
                        rfcsuma.Add("34");
                        break;
                    case "Y":
                        rfcsuma.Add("35");
                        break;
                    case "Z":
                        rfcsuma.Add("36");
                        break;
                    case " ":
                        rfcsuma.Add("37");
                        break;
                    default:
                        rfcsuma.Add("00");
                        break;
                }
            }

            for (int i = 13; i > 1; i--)
            {
                nv = nv + ((rfcsuma.Count == y) ? 0 : (Int32.Parse(rfcsuma[y]) * i));
                y++;
            }
            nv = nv % 11;
            if (nv == 0)
            {
                return "0";
            }
            else if (nv <= 10)
            {
                string nvs = "";
                nv = 11 - nv;
                nvs = nv.ToString();
                if (nv.ToString() == "10")
                {
                    nvs = "A";
                }
                return nvs;
            }
            else if (nv.ToString() == "10")
            {
                return "A";
            }
            return "0";
        }

        private string GetHomonimia(string pPaterno, string pMaterno, string pNombre)
        {
            string nombreCompleto = pPaterno.Trim() + ' ' + pMaterno.Trim() + ' ' + pNombre.Trim();
            string numero = "0";
            string letra = "", numero1 = "", numero2 = "";
            int numeroSuma = 0;
            for (int i = 0; i < nombreCompleto.Length; i++)
            {
                letra = nombreCompleto.Substring(i, 1).ToUpper();
                switch (letra)
                {
                    case " ":
                        numero = numero + "00";
                        break;
                    case "Ñ":
                        numero = numero + "40";
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
                    default:
                        numero = numero + "00";
                        break;
                }
            }
            // + 1
            for (int i = 0; i < numero.Length; i++)
            {
                numero1 = ((i + 1) == numero.Length) ? "0" : numero.Substring(i, 2);
                numero2 = (i == numero.Length - 1) ? "0" : numero.Substring(i + 1, 1);
                numeroSuma = numeroSuma + (Int32.Parse(numero1) * Int32.Parse(numero2));
            }
            var numero3 = numeroSuma % 1000;
            var numero4 = numero3 / 34;
            var numero5 = numero4.ToString().Split('.')[0];
            var numero6 = numero3 % 34;
            string homonimio = "";

            //valores que se asignan a la clave diferenciadora de homonimia en base al coeficiente y al residuo.
            switch (numero5)
            {
                case "0":
                    homonimio = "1";
                    break;
                case "1":
                    homonimio = "2";
                    break;
                case "2":
                    homonimio = "3";
                    break;
                case "3":
                    homonimio = "4";
                    break;
                case "4":
                    homonimio = "5";
                    break;
                case "5":
                    homonimio = "6";
                    break;
                case "6":
                    homonimio = "7";
                    break;
                case "7":
                    homonimio = "8";
                    break;
                case "8":
                    homonimio = "9";
                    break;
                case "9":
                    homonimio = "A";
                    break;
                case "10":
                    homonimio = "B";
                    break;
                case "11":
                    homonimio = "C";
                    break;
                case "12":
                    homonimio = "D";
                    break;
                case "13":
                    homonimio = "E";
                    break;
                case "14":
                    homonimio = "F";
                    break;
                case "15":
                    homonimio = "G";
                    break;
                case "16":
                    homonimio = "H";
                    break;
                case "17":
                    homonimio = "I";
                    break;
                case "18":
                    homonimio = "J";
                    break;
                case "19":
                    homonimio = "K";
                    break;
                case "20":
                    homonimio = "L";
                    break;
                case "21":
                    homonimio = "M";
                    break;
                case "22":
                    homonimio = "N";
                    break;
                case "23":
                    homonimio = "P";
                    break;
                case "24":
                    homonimio = "Q";
                    break;
                case "25":
                    homonimio = "R";
                    break;
                case "26":
                    homonimio = "S";
                    break;
                case "27":
                    homonimio = "T";
                    break;
                case "28":
                    homonimio = "U";
                    break;
                case "29":
                    homonimio = "V";
                    break;
                case "30":
                    homonimio = "W";
                    break;
                case "31":
                    homonimio = "X";
                    break;
                case "32":
                    homonimio = "Y";
                    break;
                case "33":
                    homonimio = "Z";
                    break;
            }

            //valores que se asignan a la clave diferenciadora de homonimia en base al coeficiente y al residuo.
            switch (numero6)
            {
                case 0:
                    homonimio = homonimio + "1";
                    break;
                case 1:
                    homonimio = homonimio + "2";
                    break;
                case 2:
                    homonimio = homonimio + "3";
                    break;
                case 3:
                    homonimio = homonimio + "4";
                    break;
                case 4:
                    homonimio = homonimio + "5";
                    break;
                case 5:
                    homonimio = homonimio + "6";
                    break;
                case 6:
                    homonimio = homonimio + "7";
                    break;
                case 7:
                    homonimio = homonimio + "8";
                    break;
                case 8:
                    homonimio = homonimio + "9";
                    break;
                case 9:
                    homonimio = homonimio + "A";
                    break;
                case 10:
                    homonimio = homonimio + "B";
                    break;
                case 11:
                    homonimio = homonimio + "C";
                    break;
                case 12:
                    homonimio = homonimio + "D";
                    break;
                case 13:
                    homonimio = homonimio + "E";
                    break;
                case 14:
                    homonimio = homonimio + "F";
                    break;
                case 15:
                    homonimio = homonimio + "G";
                    break;
                case 16:
                    homonimio = homonimio + "H";
                    break;
                case 17:
                    homonimio = homonimio + "I";
                    break;
                case 18:
                    homonimio = homonimio + "J";
                    break;
                case 19:
                    homonimio = homonimio + "K";
                    break;
                case 20:
                    homonimio = homonimio + "L";
                    break;
                case 21:
                    homonimio = homonimio + "M";
                    break;
                case 22:
                    homonimio = homonimio + "N";
                    break;
                case 23:
                    homonimio = homonimio + "P";
                    break;
                case 24:
                    homonimio = homonimio + "Q";
                    break;
                case 25:
                    homonimio = homonimio + "R";
                    break;
                case 26:
                    homonimio = homonimio + "S";
                    break;
                case 27:
                    homonimio = homonimio + "T";
                    break;
                case 28:
                    homonimio = homonimio + "U";
                    break;
                case 29:
                    homonimio = homonimio + "V";
                    break;
                case 30:
                    homonimio = homonimio + "W";
                    break;
                case 31:
                    homonimio = homonimio + "X";
                    break;
                case 32:
                    homonimio = homonimio + "Y";
                    break;
                case 33:
                    homonimio = homonimio + "Z";
                    break;
            }
            return homonimio;
        }

        private string RfcQuitaProhibidas(string pRfc)
        {
            string res = "";
            pRfc = pRfc.ToUpper();
            string strPalabrasAltisonantes = GetPalabrasAltisonantes();
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pRfc);
            System.Text.RegularExpressions.Match match = regex.Match(strPalabrasAltisonantes);
            if (match.Success)
            {
                string aux = pRfc.Substring(0, 1) + "X" + pRfc.Substring(2, 2);
                return aux;
            }
            else
            {
                return pRfc;
            }

        }

        private string GetPalabrasAltisonantes()
        {
            string strPalabras = "BACA*BAKA*BUEI*BUEY*CACA*CACO*CAGA*CAGO*CAKA*CAKO*COGE*COGI*COJA*COJE*COJI*COJO*COLA*CULO*";
            strPalabras = strPalabras + "FALO*FETO*GETA*GUEI*GUEY*JETA*JOTO*";
            strPalabras = strPalabras + "KOGE*KOJO*KAKA*KULO*MAME*MAMO*MEAR*";
            strPalabras = strPalabras + "MEAS*MEON*MION*COJE*COJI*COJO*CULO*";
            strPalabras = strPalabras + "KACA*KACO*KAGA*KAGO*KAKO*KOGI*KOJA*KOJE*KOJI*KOLA*LILO*LOCA*LOCO*LOKA*LOKO*";
            strPalabras = strPalabras + "MIAR*MOKO*MULO*NACO*NACA*PIPI*PITO*POPO*ROBA*ROBE*ROBO*SENO*TETA*VACA*VAGA*VAGO*VAKA*VUEI*VUEY*WUEI*WUEY*";
            strPalabras = strPalabras + "MOCO*MULA*PEDA*PEDO*PENE*PUTA*PUTO*";
            strPalabras = strPalabras + "QULO*RATA*RUIN*";
            return strPalabras;
        }

        private string ArmarRfc(string pPaterno, string pMaterno, string pNombre)
        {
            string rfc = "";
            //obtenemos la primera letra del apellido paterno
            rfc = rfc + pPaterno[0];
            //obtenemos la primera vocal del apellido paterno
            string[] vocales = { "A", "E", "I", "O", "U" };
            bool bVocal = false;
            int iPosicion = 0;
            for (var i = 0; i < pPaterno.Length; i++)
            {
                if (vocales.Contains("" + pPaterno[i]) && i != 0)
                {
                    bVocal = true;
                    iPosicion = i;
                    break;
                }
            }
            if (bVocal == true)
                rfc = rfc + pPaterno[iPosicion];
            else
                rfc = rfc + "X";

            //obtenemos la primera letra del apellido materno
            if (!String.IsNullOrEmpty(pMaterno))
                rfc = rfc + pMaterno[0];
            else
                rfc = rfc + "X";

            //obtenemos la primera letra del nombre
            rfc = rfc + pNombre[0];
            return rfc;
        }

        private string RfcApellidoCorto(string pPaterno, string pMaterno, string pNombre)
        {
            string rfc = "";
            //obtenemos la primera letra del apellido paterno
            rfc = rfc + pPaterno[0];
            //obtenemos la segunda letra del apellido paterno
            if (pPaterno.Length > 1)
                rfc = rfc + pPaterno[1];
            else
                rfc = rfc + "X";

            //obtenemos la primera letra del apellido materno
            if (!String.IsNullOrEmpty(pMaterno))
                rfc = rfc + pMaterno[0];
            else
                rfc = rfc + "X";

            //obtenemos la primera letra del nombre
            rfc = rfc + pNombre[0];
            return rfc;
        }

        private string RfcUnApellido(string pNombre, string pApellido)
        {
            string rfc = "";
            //obtenemos la primera letra del apellido
            rfc = rfc + pApellido[0];
            //obtenemos la primera vocal del apellido
            string[] vocales = { "A", "E", "I", "O", "U" };
            bool bVocal = false;
            int iPosicion = 0;
            for (var i = 0; i < pApellido.Length; i++)
            {
                if (vocales.Contains("" + pApellido[i]) && i != 0)
                {
                    bVocal = true;
                    iPosicion = i;
                    break;
                }
            }
            if (bVocal == true)
                rfc = rfc + pApellido[iPosicion];
            else
                rfc = rfc + "X";

            //obtenemos la primera letra del nombre
            rfc = rfc + pNombre[0];

            //obtenemos la segunda letra del nombre
            string[] nombres = pNombre.Split(' ');
            if (nombres.Length > 1)
                rfc = rfc + nombres[1][0];
            else
                rfc = rfc + "X";

            return rfc;
        }

        private string RfcFiltraAcentos(string pCadena)
        {
            pCadena = System.Text.RegularExpressions.Regex.Replace(pCadena, "Á", "A");
            pCadena = System.Text.RegularExpressions.Regex.Replace(pCadena, "É", "E");
            pCadena = System.Text.RegularExpressions.Regex.Replace(pCadena, "Í", "I");
            pCadena = System.Text.RegularExpressions.Regex.Replace(pCadena, "Ó", "O");
            pCadena = System.Text.RegularExpressions.Regex.Replace(pCadena, "Ú", "U");
            pCadena = System.Text.RegularExpressions.Regex.Replace(pCadena, "Ñ", "X");
            return pCadena;
        }

        private string RfcFiltraNombres(string pNombre)
        {
            string[] strArPalabras = { ",", "de ", "del ", "la ", "los ", "las ", "y ", "mc ", "mac ", "von ", "van ", "DE ", "DEL ", "LA ", "LOS ", "LAS ", "Y ", "MC ", "MAC ", "VON ", "VAN ", "MA.", "MA. ", "ma ", "MA " };
            for (int i = 0; i < strArPalabras.Length; i++)
            {
                pNombre = pNombre.Replace(strArPalabras[i], "");
            }

            string[] strArPalabrasNombres = { "JOSE ", "MARIA ", "J ", "MA ", "JOSÉ ", "MARÍA ","J. ","MA.", "MARÍA LOS", "MARIA LOS","M ","M. ","M.","J." };
            //string[] strArPalabrasNombres = { "jose ", "maria ", "jose", "maria", "MARÍA", "MARIA", "JOSÉ", "JOSE", "J.", "J. ", "J ", "j ", "MA", "MARIA ", "MARÍA ", "MARÍA LOS", "MARIA LOS" };

            for (int i = 0; i < strArPalabrasNombres.Length; i++)
            {
                string[] nombres = pNombre.Split(' ');
                if (pNombre != strArPalabrasNombres[i] && nombres.Length != 1)
                    if (!String.IsNullOrEmpty(nombres[1]))
                        pNombre = pNombre.Replace(strArPalabrasNombres[i], "");
            }

            if (pNombre.Length > 1)
            {
                switch (pNombre.Substring(0, 2))
                {
                    case "CH":
                        pNombre = pNombre.Replace("CH", "C");
                        break;
                    case "LL":
                        pNombre = pNombre.Replace("LL", "L");
                        break;
                    case "TR":
                        pNombre = pNombre.Replace("TR", "T");
                        break;
                }
            }
            return pNombre.Replace(" ", "");
        }

        public string GetFechaNacimiento(DateTime pFecha)
        {
            string anio = pFecha.Year.ToString().Substring(2, 2);
            string mes = (pFecha.Month.ToString().Length == 1) ? "0" + pFecha.Month.ToString() : pFecha.Month.ToString();
            string dia = (pFecha.Day.ToString().Length == 1) ? "0" + pFecha.Day.ToString() : pFecha.Day.ToString();
            return anio + mes + dia;
        }
    }
}
