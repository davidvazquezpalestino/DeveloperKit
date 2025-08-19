namespace CoreInterfaces;

/// <summary>Define un contrato para controlar el estado de solo lectura de un objeto.</summary>
public interface IReadOnly
{
    /// <summary>Obtiene o establece un valor que indica si el objeto es de solo lectura.</summary>
    bool ReadOnly { get; set; }
    /// <summary>Obtiene o establece un valor que indica si se debe ignorar el estado de solo lectura.</summary>
    bool IgnoreReadOnly { get; set; }
}