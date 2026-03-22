namespace DevKit.Rfc.ValueObjects
{
    /// <summary>
    /// Value Object que representa una CURP (Clave Única de Registro de Población).
    /// Inmutable y con validación de formato.
    /// </summary>
    public sealed class CurpVO
    {
        private readonly string _valor;
        private const int LongitudCurp = 18;

        /// <summary>
        /// Obtiene el valor de la CURP.
        /// </summary>
        public string Valor => _valor;

        /// <summary>
        /// Obtiene los primeros 4 caracteres (letras del nombre).
        /// </summary>
        public string LetrasNombre => _valor.Substring(0, 4);

        /// <summary>
        /// Obtiene la fecha de nacimiento en formato yyMMdd.
        /// </summary>
        public string FechaNacimiento => _valor.Substring(4, 6);

        /// <summary>
        /// Obtiene el sexo (H o M).
        /// </summary>
        public char Sexo => _valor[10];

        /// <summary>
        /// Obtiene el código de entidad federativa.
        /// </summary>
        public string EntidadFederativa => _valor.Substring(11, 2);

        /// <summary>
        /// Obtiene las consonantes internas.
        /// </summary>
        public string ConsonantesInternas => _valor.Substring(13, 3);

        /// <summary>
        /// Obtiene el dígito verificador.
        /// </summary>
        public string DigitoVerificador => _valor.Substring(16, 2);

        /// <summary>
        /// Constructor privado para crear una CURP validada.
        /// </summary>
        /// <param name="valor">Value de la CURP.</param>
        /// <exception cref="ArgumentException">Si el formato es inválido.</exception>
        private CurpVO(string valor)
        {
            _valor = valor ?? throw new ArgumentNullException(nameof(valor));

            if (!EsFormatoValido(valor))
            {
                throw new ArgumentException("El formato de la CURP no es válido.", nameof(valor));
            }
        }

        /// <summary>
        /// Crea una nueva instancia de Curp con validación.
        /// </summary>
        /// <param name="valor">Value de la CURP.</param>
        /// <returns>Instancia de Curp.</returns>
        /// <exception cref="ArgumentException">Si el formato es inválido.</exception>
        public static CurpVO Crear(string valor)
        {
            return new CurpVO(valor);
        }

        /// <summary>
        /// Intenta crear una CURP sin lanzar excepciones.
        /// </summary>
        /// <param name="valor">Value de la CURP.</param>
        /// <param name="curp">CURP creada si el formato es válido.</param>
        /// <returns>True si el formato es válido, false en caso contrario.</returns>
        public static bool TryCrear(string valor, out CurpVO curp)
        {
            curp = null;
            
            if (string.IsNullOrEmpty(valor) || !EsFormatoValido(valor))
            {
                return false;
            }

            curp = new CurpVO(valor);
            return true;
        }

        /// <summary>
        /// Valida el formato de una CURP.
        /// </summary>
        /// <param name="valor">Value a validar.</param>
        /// <returns>True si el formato es válido.</returns>
        private static bool EsFormatoValido(string valor)
        {
            if (string.IsNullOrEmpty(valor) || valor.Length != LongitudCurp)
                return false;

            // Validar que los primeros 4 caracteres sean letras
            for (int i = 0; i < 4; i++)
            {
                if (!char.IsLetter(valor[i]))
                    return false;
            }

            // Validar que la fecha sea numérica
            for (int i = 4; i < 10; i++)
            {
                if (!char.IsDigit(valor[i]))
                    return false;
            }

            // Validar sexo
            char sexo = valor[10];
            if (sexo != 'H' && sexo != 'M')
                return false;

            // Validar que las consonantes sean letras
            for (int i = 13; i < 16; i++)
            {
                if (!char.IsLetter(valor[i]))
                    return false;
            }

            // Validar dígito verificador
            for (int i = 16; i < 18; i++)
            {
                if (!char.IsLetterOrDigit(valor[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Convierte la CURP a su representación de cadena.
        /// </summary>
        /// <returns>Value de la CURP.</returns>
        public override string ToString()
        {
            return _valor;
        }

        /// <summary>
        /// Determina si dos CURPs son iguales.
        /// </summary>
        /// <param name="obj">Objeto a comparar.</param>
        /// <returns>True si son iguales.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((CurpVO)obj);
        }

        /// <summary>
        /// Determina si dos CURPs son iguales.
        /// </summary>
        /// <param name="other">CURP a comparar.</param>
        /// <returns>True si son iguales.</returns>
        public bool Equals(CurpVO other)
        {
            if (other is null) return false;
            return string.Equals(_valor, other._valor, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtiene el código hash de la CURP.
        /// </summary>
        /// <returns>Código hash.</returns>
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(_valor);
        }

        /// <summary>
        /// Operador de igualdad.
        /// </summary>
        /// <param name="izquierda">CURP izquierda.</param>
        /// <param name="derecha">CURP derecha.</param>
        /// <returns>True si son iguales.</returns>
        public static bool operator ==(CurpVO izquierda, CurpVO derecha)
        {
            return Equals(izquierda, derecha);
        }

        /// <summary>
        /// Operador de desigualdad.
        /// </summary>
        /// <param name="izquierda">CURP izquierda.</param>
        /// <param name="derecha">CURP derecha.</param>
        /// <returns>True si son diferentes.</returns>
        public static bool operator !=(CurpVO izquierda, CurpVO derecha)
        {
            return !Equals(izquierda, derecha);
        }
    }
}
