namespace CoreInterfaces;

/// <summary>Define un contrato para tipos que representan formularios en la aplicación.</summary>
public interface IForm
{
    /// <summary>Obtiene o establece el tipo del formulario.</summary>
    Type Form { get; set; }
}