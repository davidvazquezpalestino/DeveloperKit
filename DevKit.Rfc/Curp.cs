using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevKit.Rfc
{
    /*******
     * CLASE PARA EL CÁLCULO DE LA CURP
     * AUTOR: AIMEE RIOS
     * **********/
    public class Curp
    {
        string[] vocales = { "A", "E", "I", "O", "U" };

        public string CalcularCURP(string pNombre, string pPaterno, string pMaterno, DateTime pFecha, string pSexo, string pEstado)
        {
            string Curp = "";
            string Nombre = LimpiaAcentos(pNombre.ToUpper());           
            string Paterno = LimpiaAcentos(pPaterno.ToUpper());
            string Materno = LimpiaAcentos(pMaterno.ToUpper());
            if (string.IsNullOrEmpty(Nombre) || string.IsNullOrEmpty(Paterno))
            {
                return "";
            }
            Nombre = CURPFiltrado(Nombre);
            Paterno = CURPFiltrado(Paterno);
            string NombreCompuesto = FiltradoCompuesto(Nombre);
            string PaternoCompuesto = FiltradoCompuesto(Paterno);
            string MaternoCompuesto = "";
            if (!String.IsNullOrEmpty(Materno))
            {
                Materno = CURPFiltrado(Materno);
                MaternoCompuesto = FiltradoCompuesto(Materno);
            }


            //Obtenemos la primera vocal del apellido paterno, si contiene caracteres como /,-,. se asigna la x como segunda letra de la curp
            string segundaLetra = ((PaternoCompuesto.IndexOf('/') != -1) || (PaternoCompuesto.IndexOf('-') != -1) || (PaternoCompuesto.IndexOf('.') != -1)) ? "X" : ObtenerVocalInterna(Paterno);
            Curp = PaternoCompuesto[0] + segundaLetra;

            //D/AMICO---si los apellidos o nombre tienen caracteres especiales como /,-, . , entonces se cambian por X
            string[] strArCaracteresEspeciales = { "/", "-", "." };
            for (var i = 0; i < strArCaracteresEspeciales.Length; i++)
            {
                Nombre = Nombre.Replace(strArCaracteresEspeciales[i], "X");
                PaternoCompuesto = PaternoCompuesto.Replace(strArCaracteresEspeciales[i], "X");
                MaternoCompuesto = MaternoCompuesto?.Replace(strArCaracteresEspeciales[i], "X");
            }

            //Obtenemos la primer letra del apellido materno
            if (!String.IsNullOrEmpty(MaternoCompuesto))
                Curp = Curp + MaternoCompuesto[0];
            else
                Curp = Curp + "X";

            //Letra del Nombre
            Curp = Curp + NombreCompuesto[0];

            //Quitar Palabras Altisonantes
            Curp = CurpQuitaProhibidas(Curp);
            //Obtenemos la fecha de nacimiento, el sexo y el lugar de nacimiento
            string sFechaNac = GetFechaNacimientoCurp(pFecha);
            Curp = Curp + sFechaNac;
            Curp = Curp + pSexo + pEstado;

            //Obtenemos la primer consonante interna del apellido paterno,materno y el primer nombre
            Curp = Curp + ObtenerConsonanteInterna(Paterno);
            Curp = Curp + ObtenerConsonanteInterna(Materno);
            Curp = Curp + ObtenerConsonanteInterna(Nombre);

            //Los dos últimos digitos son de control generados por el gobierno para evitar duplicados y no tenemos acceso a ellos.
            //curp = curp + "00";
            int anio = pFecha.Year;
            Curp = Curp + ObtenerDigitoVerificadorCurp(anio, Curp);
            return Curp;
        }
        public string CURPFiltrado(string pNombre)
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

            //if (pNombre.Length > 1)
            //{
            //    switch (pNombre.Substring(0, 2))
            //    {
            //        case "CH":
            //            pNombre = pNombre.Replace("CH", "C");
            //            break;
            //        case "LL":
            //            pNombre = pNombre.Replace("LL", "L");
            //            break;
            //        case "TR":
            //            pNombre = pNombre.Replace("TR", "T");
            //            break;
            //    }
            //}
            return pNombre.Replace(" ", "");
        }

        public string FiltradoCompuesto(string pNombre)
        {
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

            return pNombre;
        }

        private string LimpiaAcentos(string pCadena)
        {
            pCadena = Regex.Replace(pCadena, "Á", "A");
            pCadena = Regex.Replace(pCadena, "É", "E");
            pCadena = Regex.Replace(pCadena, "Í", "I");
            pCadena = Regex.Replace(pCadena, "Ó", "O");
            pCadena = Regex.Replace(pCadena, "Ú", "U");
            pCadena = Regex.Replace(pCadena, "Ñ", "X");
            return pCadena;
        }

        public string ObtenerVocalInterna(string pCadena)
        {
            string tmp = "X";
            int tmp1 = 0, tmp2 = 0;

            if (!String.IsNullOrEmpty(pCadena))
            {
                for (int i = 0; i < pCadena.Length; i++)
                {
                    if (tmp1 == 0 && vocales.Contains("" + pCadena[i]) && tmp2 != 0)
                    {
                        tmp1 = tmp1 + 1;
                        tmp = "" + pCadena[i];
                    }
                    tmp2++;
                }
            }

            return tmp;
        }

        private string CurpQuitaProhibidas(string pCurp)
        {
            string res = "";
            pCurp = pCurp.ToUpper();
            string strPalabrasAltisonantes = GetPalabrasAltisonantes();
            Regex regex = new Regex(pCurp);
            Match match = regex.Match(strPalabrasAltisonantes);
            if (match.Success)
            {
                string aux = pCurp.Substring(0, 1) + "X" + pCurp.Substring(2, 2);
                return aux;
            }
            else
            {
                return pCurp;
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

        public string GetFechaNacimientoCurp(DateTime pFecha)
        {
            string anio = pFecha.Year.ToString().Substring(2, 2);
            string mes = (pFecha.Month.ToString().Length == 1) ? "0" + pFecha.Month.ToString() : pFecha.Month.ToString();
            string dia = (pFecha.Day.ToString().Length == 1) ? "0" + pFecha.Day.ToString() : pFecha.Day.ToString();
            return anio + mes + dia;
        }

        public string ObtenerConsonanteInterna(string pCadena)
        {
            string tmp = "X";
            int tmp1 = 0, tmp2 = 0;

            if (!String.IsNullOrEmpty(pCadena))
            {
                for (int i = 0; i < pCadena.Length; i++)
                {
                    if (tmp1 == 0 && !vocales.Contains("" + pCadena[i]) && tmp2 != 0)
                    {
                        tmp1 = tmp1 + 1;
                        tmp = "" + pCadena[i];
                    }
                    tmp2++;
                }
            }
            return tmp;
        }

        public string ObtenerDigitoVerificadorCurp(int pAnio, string pCurp)
        {
            // Se obtiene el digito verificador
            int contador = 18;
            int contador1 = 0, valor = 0, sumatoria = 0;
            double numVer = 0.00;

            while (contador1 <= 15)
            {
                string pstCom = pCurp.Substring(contador1, 1);
                switch (pstCom)
                {
                    case "0":
                        valor = 0 * contador;
                        break;
                    case "1":
                        valor = 1 * contador;
                        break;
                    case "2":
                        valor = 2 * contador;
                        break;
                    case "3":
                        valor = 3 * contador;
                        break;
                    case "4":
                        valor = 4 * contador;
                        break;
                    case "5":
                        valor = 5 * contador;
                        break;
                    case "6":
                        valor = 6 * contador;
                        break;
                    case "7":
                        valor = 7 * contador;
                        break;
                    case "8":
                        valor = 8 * contador;
                        break;
                    case "9":
                        valor = 9 * contador;
                        break;
                    case "A":
                        valor = 10 * contador;
                        break;
                    case "B":
                        valor = 11 * contador;
                        break;
                    case "C":
                        valor = 12 * contador;
                        break;
                    case "D":
                        valor = 13 * contador;
                        break;
                    case "E":
                        valor = 14 * contador;
                        break;
                    case "F":
                        valor = 15 * contador;
                        break;
                    case "G":
                        valor = 16 * contador;
                        break;
                    case "H":
                        valor = 17 * contador;
                        break;
                    case "I":
                        valor = 18 * contador;
                        break;
                    case "J":
                        valor = 19 * contador;
                        break;
                    case "K":
                        valor = 20 * contador;
                        break;
                    case "L":
                        valor = 21 * contador;
                        break;
                    case "M":
                        valor = 22 * contador;
                        break;
                    case "N":
                        valor = 23 * contador;
                        break;
                    case "Ñ":
                        valor = 24 * contador;
                        break;
                    case "O":
                        valor = 25 * contador;
                        break;
                    case "P":
                        valor = 26 * contador;
                        break;
                    case "Q":
                        valor = 27 * contador;
                        break;
                    case "R":
                        valor = 28 * contador;
                        break;
                    case "S":
                        valor = 29 * contador;
                        break;
                    case "T":
                        valor = 30 * contador;
                        break;
                    case "U":
                        valor = 31 * contador;
                        break;
                    case "V":
                        valor = 32 * contador;
                        break;
                    case "W":
                        valor = 33 * contador;
                        break;
                    case "X":
                        valor = 34 * contador;
                        break;
                    case "Y":
                        valor = 35 * contador;
                        break;
                    case "Z":
                        valor = 36 * contador;
                        break;
                }

                contador = contador - 1;
                contador1 = contador1 + 1;
                sumatoria = sumatoria + valor;
            }

            numVer = sumatoria % 10;
            numVer = Math.Abs(10 - numVer);

            if (numVer == 10)
                numVer = 0;

            string digVerificador = "";
            if (pAnio < 2000)
                digVerificador = "0" + numVer;
            if (pAnio >= 2000)
                digVerificador = "A" + numVer;

            return digVerificador;
        }
    }
}
