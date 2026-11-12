namespace FloodView.Api.Models;

public class Leitura
{
    public int Id { get; set; }
    public int DispositivoId { get; set; }
    public double DistanciaCm { get; set; }
    public DateTime RecebidaEmUtc { get; set; } = DateTime.UtcNow;

    public Dispositivo? Dispositivo { get; set; }
}