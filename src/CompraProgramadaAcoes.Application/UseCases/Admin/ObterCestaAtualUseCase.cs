using CompraProgramadaAcoes.Application.DTOs.Admin;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Services;
using CompraProgramadaAcoes.Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace CompraProgramadaAcoes.Application.UseCases.Admin;

public class ObterCestaAtualUseCase
{
    private readonly ICestaRecomendacaoRepository _cestaRepository;
    private readonly CotahistParser _cotahistParser;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public ObterCestaAtualUseCase(
        ICestaRecomendacaoRepository cestaRepository,
        CotahistParser cotahistParser,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        _cestaRepository = cestaRepository;
        _cotahistParser = cotahistParser;
        _configuration = configuration;
        _env = env;
    }

    private string ObterCaminhoCotacoes()
    {
        var path = _configuration["FileStorage:CotacoesPath"] ?? "cotacoes";
        return Path.GetFullPath(Path.Combine(_env.ContentRootPath, path));
    }

    public async Task<CestaAtualResponse> ExecuteAsync()
    {
        var cesta = await _cestaRepository.ObterCestaVigenteAsync();

        if (cesta == null)
            throw new NotFoundException("Nenhuma cesta ativa encontrada.", "CESTA_NAO_ENCONTRADA");

        // Obter cotações atuais
        var tickers = cesta.Itens.Select(i => i.Ticker).Where(t => !string.IsNullOrEmpty(t));
        var cotacoes = _cotahistParser.ObterCotacoesFechamento(ObterCaminhoCotacoes(), tickers);

        return new CestaAtualResponse
        {
            CestaId = cesta.Id,
            Nome = cesta.Nome,
            Ativa = cesta.Ativa,
            DataCriacao = cesta.DataCriacao,
            Itens = cesta.Itens.Select(i => new CestaItemAtualResponse
            {
                Ticker = i.Ticker,
                Percentual = i.Percentual,
                CotacaoAtual = cotacoes.GetValueOrDefault(i.Ticker)?.PrecoFechamento ?? 0
            }).ToList()
        };
    }
}
