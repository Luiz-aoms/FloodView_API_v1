using FloodView.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodView.Api.Controllers;

[ApiController]
[Route("api/dispositivos")]
public class DispositivosController : ControllerBase
{
    private readonly DadosFloodView _dados;

    public DispositivosController(DadosFloodView dados)
    {
        _dados = dados;
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar(
        CadastrarDispositivoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            return BadRequest("Informe o nome do dispositivo.");

        var dispositivo = new Dispositivo
        {
            Nome = request.Nome.Trim()
        };

        _dados.Dispositivos.Add(dispositivo);
        await _dados.SaveChangesAsync();

        return Ok(new
        {
            dispositivo.Id,
            dispositivo.Nome,
            dispositivo.Ativo,
            dispositivo.Pausado,
            dispositivo.UltimoContatoUtc
        });
    }

    [HttpPost("{id:int}/leituras")]
    public async Task<IActionResult> RegistrarLeitura(
        int id,
        RegistrarLeituraRequest request)
    {
        var dispositivo = await _dados.Dispositivos.FindAsync(id);

        if (dispositivo is null)
            return NotFound("Dispositivo não cadastrado.");

        if (!dispositivo.Ativo || dispositivo.Pausado)
            return Conflict("O dispositivo está pausado ou desativado.");

        if (request.DistanciaCm < 2 || request.DistanciaCm > 400)
            return BadRequest("A distância precisa estar entre 2 e 400 cm.");

        var leitura = new Leitura
        {
            DispositivoId = id,
            DistanciaCm = request.DistanciaCm,
            RecebidaEmUtc = DateTime.UtcNow
        };

        dispositivo.UltimoContatoUtc = leitura.RecebidaEmUtc;
        _dados.Leituras.Add(leitura);
        await _dados.SaveChangesAsync();

        return Ok(new
        {
            leitura.Id,
            leitura.DispositivoId,
            leitura.DistanciaCm,
            leitura.RecebidaEmUtc
        });
    }

    [HttpGet("{id:int}/leituras")]
    public async Task<IActionResult> ListarLeituras(
    int id,
    [FromQuery] int limite = 60)
    {
        var dispositivoExiste = await _dados.Dispositivos
            .AnyAsync(d => d.Id == id);

        if (!dispositivoExiste)
            return NotFound("Dispositivo não cadastrado.");

        limite = Math.Clamp(limite, 1, 200);

        var leituras = await _dados.Leituras
            .Where(l => l.DispositivoId == id)
            .OrderByDescending(l => l.RecebidaEmUtc)
            .Take(limite)
            .Select(l => new
            {
                l.Id,
                l.DistanciaCm,
                l.RecebidaEmUtc
            })
            .ToListAsync();

        return Ok(leituras);
    }
}

public record CadastrarDispositivoRequest(string Nome);
public record RegistrarLeituraRequest(double DistanciaCm);