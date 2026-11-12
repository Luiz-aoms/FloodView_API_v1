using System.Globalization;
using System.IO.Ports;
using System.Net.Http.Json;

const string portaSerial = "COM5";
const int dispositivoId = 1;
const string enderecoApi = "https://localhost:7064/";

using var serial = new SerialPort(portaSerial, 9600)
{
    NewLine = "\n",
    ReadTimeout = 1000
};

using var http = new HttpClient
{
    BaseAddress = new Uri(enderecoApi)
};

try
{
    serial.Open();
    Console.WriteLine($"Lendo o Nano pela {portaSerial}. Pressione Ctrl+C para parar.");

    while (true)
    {
        try
        {
            string linha = serial.ReadLine().Trim();

            if (!linha.StartsWith("Distancia:", StringComparison.OrdinalIgnoreCase))
                continue;

            string texto = linha
                .Replace("Distancia:", "", StringComparison.OrdinalIgnoreCase)
                .Replace("cm", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (!double.TryParse(
                    texto,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double distanciaCm))
            {
                Console.WriteLine($"Leitura inválida: {linha}");
                continue;
            }

            var leitura = new { distanciaCm };

            using var resposta = await http.PostAsJsonAsync(
                $"api/dispositivos/{dispositivoId}/leituras",
                leitura);

            if (resposta.IsSuccessStatusCode)
                Console.WriteLine($"Enviado: {distanciaCm:F1} cm");
            else
                Console.WriteLine($"API respondeu: {(int)resposta.StatusCode}");
        }
        catch (TimeoutException)
        {
            // Aguarda a próxima linha enviada pelo Arduino.
        }
        catch (HttpRequestException erro)
        {
            Console.WriteLine($"Falha ao conectar à API: {erro.Message}");
        }
    }
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine($"Não consegui abrir {portaSerial}. Feche o Monitor Serial da IDE.");
}
catch (IOException erro)
{
    Console.WriteLine($"Falha na porta serial: {erro.Message}");
}