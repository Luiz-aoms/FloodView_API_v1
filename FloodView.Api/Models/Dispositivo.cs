namespace FloodView.Api.Models;

public class Dispositivo
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public bool Pausado { get; set; }
    public DateTime? UltimoContatoUtc { get; set; }

    public List<Leitura> Leituras { get; set; } = [];
}