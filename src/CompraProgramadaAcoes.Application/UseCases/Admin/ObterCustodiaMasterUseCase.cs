using CompraProgramadaAcoes.Application.DTOs.Admin;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Services;
using CompraProgramadaAcoes.Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace CompraProgramadaAcoes.Application.UseCases.Admin;

public class ObterCustodiaMasterUseCase(
    IContaGraficaRepository contaGraficaRepository,
    ICustodiaRepository custodiaRepository,
    CotahistParser cotahistParser,
    IConfiguration configuration,
    IWebHostEnvironment env)
{
  private readonly IContaGraficaRepository _contaGraficaRepository = contaGraficaRepository;
  private readonly ICustodiaRepository _custodiaRepository = custodiaRepository;
  private readonly CotahistParser _cotahistParser = cotahistParser;
  private readonly IConfiguration _configuration = configuration;
  private readonly IWebHostEnvironment _env = env;

  private string ObterCaminhoCotacoes()
  {
    var path = _configuration["FileStorage:CotacoesPath"] ?? "cotacoes";
    return Path.GetFullPath(Path.Combine(_env.ContentRootPath, path));
  }

  public async Task<ContaMasterCustodiaResponse> ExecuteAsync()
  {
    // Buscar conta master (tipo MASTER)
    var contas = await _contaGraficaRepository.ObterPorTipoAsync("MASTER");
    var contaMaster = contas.FirstOrDefault() ?? 
        throw new NotFoundException("Conta master não encontrada.", "CONTA_MASTER_NAO_ENCONTRADA");
    var custodia = await _custodiaRepository.ObterPorContaGraficaAsync(contaMaster.Id);

    // Obter cotações atuais
    var tickers = custodia.Where(c => c.Quantidade > 0).Select(c => c.Ticker);
    var cotacoes = _cotahistParser.ObterCotacoesFechamento(ObterCaminhoCotacoes(), tickers);

    var itensCustodia = custodia
        .Where(c => c.Quantidade > 0)
        .Select(c => new ContaMasterCustodiaItemResponse
        {
          Ticker = c.Ticker!,
          Quantidade = c.Quantidade,
          PrecoMedio = c.PrecoMedio,
          ValorAtual = cotacoes.GetValueOrDefault(c.Ticker)?.PrecoFechamento ?? 0,
          Origem = "Residuo distribuicao"
        }).ToList();

    var valorTotalResiduo = itensCustodia.Sum(i => i.ValorAtual * i.Quantidade);

    return new ContaMasterCustodiaResponse
    {
      ContaMaster = new ContaMasterInfoResponse
      {
        Id = contaMaster.Id,
        NumeroConta = contaMaster.NumeroConta!,
        Tipo = contaMaster.Tipo
      },
      Custodia = itensCustodia,
      ValorTotalResiduo = valorTotalResiduo
    };
  }
}
