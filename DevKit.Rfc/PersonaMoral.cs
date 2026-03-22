using System;

namespace DevKit.Rfc
{
    /*******
     * CLASE PARA EL CÁLCULO DEL RFC DE PERSONA MORAL
     * AUTOR: AIMEE RIOS
     * **********/
    public class PersonaMoral
    {
        private bool _generarHomoclave;
        public PersonaMoral(bool pGenerarHomoclave)
        {
            _generarHomoclave = pGenerarHomoclave;
        }

        public string CalcularRFCPersonaMoral(string pRazonSocial, DateTime fecha)
        {
            string rfc = "";
            string razonSocial = pRazonSocial.ToUpper();
            if (string.IsNullOrEmpty(razonSocial))
            {
                return "";
            }
            string FechaNacimiento = GetFechaNacimiento(fecha);
            //FILTRA ACENTOS
            string RazonSocialF = RfcFiltraAcentos(razonSocial);
            //GUARDA NOMBRE ORIGINAL PARA GENERAR HOMOCLAVE
            string RazonSocialOrig = RazonSocialF;
            //ELIMINA PALABRAS SOBRANTES DE APELLIDOS Y NOMBRE
            RazonSocialF = RfcFiltraNombres(RazonSocialF);

            rfc = ArmarRfcMoral(RazonSocialF);

            rfc = RfcQuitaProhibidas(rfc);
            rfc = rfc + FechaNacimiento;
            if (_generarHomoclave)//evalua si calcula la homoclave
            {
                rfc += GetHomonimia(RazonSocialOrig);
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
                }
            }
            //genera el digito verificador
            double suma = 0;
            double digito = 0;
            double cociente = 0;
            double residuo = 0;
            double resultado = 0;
            double[] rfcsumaint = Array.ConvertAll(rfcsuma.ToArray(), s => double.Parse(s));
            for (int i = 0; i < rfcsumaint.Length; i++)
            {
                suma += rfcsumaint[i] * (i + 1);
            }
            cociente = Math.Truncate(suma / 11);
            residuo = suma - (cociente * 11);
            resultado = 11 - residuo;
            if (resultado == 11)
            {
                return "0";
            }
            else
            {
                if (resultado == 10)
                {
                    return "A";
                }
                else
                {
                    return resultado.ToString();
                }
            }
        }

        private string GetHomonimia(string pRazonSocial)
        {
            string[] strArCaracteresEspeciales = { "/", "-", "." };
            for (var i = 0; i < strArCaracteresEspeciales.Length; i++)
            {
                pRazonSocial = pRazonSocial.Replace(strArCaracteresEspeciales[i], "X");
            }

            string homonimia = "";
            //obtenemos las primeras tres letras de la razón social
            if (pRazonSocial.Length >= 3)
            {
                homonimia = pRazonSocial.Substring(0, 3);
            }
            else
            {
                homonimia = pRazonSocial.PadRight(3, 'X');
            }
            return homonimia;
        }

        private string ArmarRfcMoral(string pRazonSocial)
        {
            string rfc = "";
            //obtenemos las primeras tres letras de la razón social
            if (pRazonSocial.Length >= 3)
            {
                rfc = pRazonSocial.Substring(0, 3);
            }
            else
            {
                rfc = pRazonSocial.PadRight(3, 'X');
            }
            return rfc;
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
