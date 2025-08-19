namespace CoreInterfaces;

/// <summary>Define un contrato para tipos que tienen un identificador principal de tipo genérico.</summary>
public interface IPrincipal<out T>
{
    /// <summary>Obtiene el identificador principal del objeto.</summary>
    T PrincipalID { get; }
}