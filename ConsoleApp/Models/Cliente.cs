namespace ConsoleNet8.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Email { get; set; }
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }
    public string Ciudad { get; set; }
}
