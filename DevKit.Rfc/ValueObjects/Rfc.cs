namespace DevKit.Rfc.ValueObjects
{
    /// <summary>
    /// Value Object que representa un RFC (Registro Federal de Contribuyentes).
    /// Inmutable y con validación de formato.
    /// </summary>
    public sealed class Rfc
    {
        private readonly string _value;
        private const int LongitudRfcPersonaFisica = 13;
        private const int LongitudRfcPersonaMoral = 12;

        /// <summary>
        /// Obtiene el valor del RFC.
        /// </summary>
        public string Value => _value;

        /// <summary>
        /// Obtiene los primeros 4 caracteres (letras del nombre).
        /// </summary>
        public string LetrasNombre => _value.Substring(0, Math.Min(4, _value.Length));

        /// <summary>
        /// Obtiene la fecha de nacimiento o constitución en formato yyMMdd.
        /// </summary>
        public string Fecha => _value.Substring(4, 6);

        /// <summary>
        /// Obtiene la homoclave (solo para RFC con homoclave).
        /// </summary>
        public string Homoclave => _value.Length > LongitudRfcPersonaFisica ?
            _value.Substring(LongitudRfcPersonaFisica, 3) : string.Empty;

        /// <summary>
        /// Obtiene el dígito verificador.
        /// </summary>
        public string DigitoVerificador => _value.Length switch
        {
            LongitudRfcPersonaFisica => _value.Substring(12, 1),
            LongitudRfcPersonaMoral => _value.Substring(11, 1),
            _ => string.Empty
        };

        /// <summary>
        /// Indica si es RFC de persona física.
        /// </summary>
        public bool EsPersonaFisica => _value.Length == LongitudRfcPersonaFisica;

        /// <summary>
        /// Indica si es RFC de persona moral.
        /// </summary>
        public bool EsPersonaMoral => _value.Length == LongitudRfcPersonaMoral;

        /// <summary>
        /// Indica si tiene homoclave.
        /// </summary>
        public bool TieneHomoclave => _value.Length > LongitudRfcPersonaFisica;

        /// <summary>
        /// Constructor privado para crear un RFC validado.
        /// </summary>
        /// <param name="valor">Value del RFC.</param>
        /// <exception cref="ArgumentException">Si el formato es inválido.</exception>
        private Rfc(string valor)
        {
            _value = valor ?? throw new ArgumentNullException(nameof(valor));

            if (!EsFormatoValido(valor))
            {
                throw new ArgumentException("El formato del RFC no es válido.", nameof(valor));
            }
        }

        /// <summary>
        /// Crea una nueva instancia de Rfc con validación.
        /// </summary>
        /// <param name="valor">Value del RFC.</param>
        /// <returns>Instancia de Rfc.</returns>
        /// <exception cref="ArgumentException">Si el formato es inválido.</exception>
        public static Rfc Crear(string valor)
        {
            return new Rfc(valor);
        }

        /// <summary>
        /// Intenta crear un RFC sin lanzar excepciones.
        /// </summary>
        /// <param name="valor">Value del RFC.</param>
        /// <param name="rfc">RFC creado si el formato es válido.</param>
        /// <returns>True si el formato es válido, false en caso contrario.</returns>
        public static bool TryCrear(string valor, out Rfc rfc)
        {
            rfc = null;

            if (string.IsNullOrEmpty(valor) || !EsFormatoValido(valor))
            {
                return false;
            }

            rfc = new Rfc(valor);
            return true;
        }

        /// <summary>
        /// Valida el formato de un RFC.
        /// </summary>
        /// <param name="valor">Value a validar.</param>
        /// <returns>True si el formato es válido.</returns>
        private static bool EsFormatoValido(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return false;

            // Validar longitud (12 para moral, 13 o más para física con homoclave)
            if (valor.Length < LongitudRfcPersonaMoral || valor.Length > LongitudRfcPersonaFisica + 3)
                return false;

            // Validar que los primeros caracteres sean letras
            int letrasNombre = valor.Length switch
            {
                LongitudRfcPersonaMoral => 3, // Persona moral: 3 letras
                LongitudRfcPersonaFisica => 4, // Persona física sin homoclave: 4 letras
                _ => 4 // Persona física con homoclave: 4 letras
            };

            for (int i = 0; i < letrasNombre; i++)
            {
                if (!char.IsLetter(valor[i]))
                    return false;
            }

            // Validar que la fecha sea numérica
            for (int i = letrasNombre; i < letrasNombre + 6; i++)
            {
                if (!char.IsDigit(valor[i]))
                    return false;
            }

            // Validar homoclave (si existe)
            if (valor.Length > LongitudRfcPersonaFisica)
            {
                for (int i = LongitudRfcPersonaFisica; i < LongitudRfcPersonaFisica + 3; i++)
                {
                    if (!char.IsLetterOrDigit(valor[i]))
                        return false;
                }
            }

            // Validar dígito verificador
            int posicionDigito = valor.Length switch
            {
                LongitudRfcPersonaMoral => 11,
                LongitudRfcPersonaFisica => 12,
                _ => valor.Length - 1
            };

            if (posicionDigito >= valor.Length || !char.IsLetterOrDigit(valor[posicionDigito]))
                return false;

            return true;
        }

        /// <summary>
        /// Convierte el RFC a su representación de cadena.
        /// </summary>
        /// <returns>Value del RFC.</returns>
        public override string ToString()
        {
            return _value;
        }

        /// <summary>
        /// Determina si dos RFCs son iguales.
        /// </summary>
        /// <param name="obj">Objeto a comparar.</param>
        /// <returns>True si son iguales.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((Rfc)obj);
        }

        /// <summary>
        /// Determina si dos RFCs son iguales.
        /// </summary>
        /// <param name="other">RFC a comparar.</param>
        /// <returns>True si son iguales.</returns>
        public bool Equals(Rfc other)
        {
            if (other is null) return false;
            return string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtiene el código hash del RFC.
        /// </summary>
        /// <returns>Código hash.</returns>
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(_value);
        }

        /// <summary>
        /// Operador de igualdad.
        /// </summary>
        /// <param name="izquierda">RFC izquierda.</param>
        /// <param name="derecha">RFC derecha.</param>
        /// <returns>True si son iguales.</returns>
        public static bool operator ==(Rfc izquierda, Rfc derecha)
        {
            return Equals(izquierda, derecha);
        }

        /// <summary>
        /// Operador de desigualdad.
        /// </summary>
        /// <param name="izquierda">RFC izquierda.</param>
        /// <param name="derecha">RFC derecha.</param>
        /// <returns>True si son diferentes.</returns>
        public static bool operator !=(Rfc izquierda, Rfc derecha)
        {
            return !Equals(izquierda, derecha);
        }
    }
}
