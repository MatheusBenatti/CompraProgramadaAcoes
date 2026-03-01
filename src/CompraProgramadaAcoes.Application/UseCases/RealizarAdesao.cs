using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Domain.Interfaces;
using CompraProgramadaAcoes.Application.Mappers;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;

namespace CompraProgramadaAcoes.Application.UseCases;

public class RealizarAdesao(
      IClienteRepository clienteRepository,
      IContaGraficaRepository contaGraficaRepository,
      ICustodiaRepository custodiaRepository,
      IClienteFactory clienteFactory,
      IContaGraficaFactory contaGraficaFactory,
      ICustodiaFactory custodiaFactory)
{
  private readonly IClienteRepository _clienteRepository = clienteRepository;
  private readonly IContaGraficaRepository _contaGraficaRepository = contaGraficaRepository;
  private readonly ICustodiaRepository _custodiaRepository = custodiaRepository;
  private readonly IClienteFactory _clienteFactory = clienteFactory;
  private readonly IContaGraficaFactory _contaGraficaFactory = contaGraficaFactory;
  private readonly ICustodiaFactory _custodiaFactory = custodiaFactory;

  public async Task<AdesaoResponse> ExecuteAsync(AdesaoRequest request)
  {
    // Validar CPF único
    if (await _clienteRepository.CpfExistsAsync(request.Cpf))
      throw new InvalidOperationException("CPF já cadastrado no sistema");

    // Criar cliente
    var cliente = _clienteFactory.Criar(request.Nome, request.Cpf, request.Email, request.ValorMensal);

    // Salvar cliente para obter ID
    cliente = await _clienteRepository.AddAsync(cliente);
    await _clienteRepository.SaveChangesAsync();

    // Criar Conta Gráfica Filhote 
    var contaGrafica = _contaGraficaFactory.Criar(cliente.Id);
    contaGrafica = await _contaGraficaRepository.AddAsync(contaGrafica);
    await _contaGraficaRepository.SaveChangesAsync();

    // Gerar número da conta após obter ID
    contaGrafica.GerarNumeroConta();

    // Criar Custódia Filhote 
    var custodia = _custodiaFactory.Criar(cliente.Id, contaGrafica.Id);
    await _custodiaRepository.AddAsync(custodia);

    // Associar entidades
    cliente.AssociarContaGrafica(contaGrafica);
    cliente.AssociarCustodia(custodia);

    // Salvar todas as alterações de uma vez
    await _clienteRepository.SaveChangesAsync();

    return AdesaoResponseMap.MapearParaResponse(cliente, contaGrafica);
  }
}
