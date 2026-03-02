using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.UseCases;

public class AlterarValorMensal(IClienteRepository clienteRepository, IHistoricoValorMensalRepository historicoRepository)
{
  private readonly IClienteRepository _clienteRepository = clienteRepository;
  private readonly IHistoricoValorMensalRepository _historicoRepository = historicoRepository;

  public async Task<AlterarValorMensalResponse> ExecuteAsync(int clienteId, AlterarValorMensalRequest request)
  {
    // Buscar cliente
    var cliente = await _clienteRepository.GetByIdAsync(clienteId)
      ?? throw new ClienteNaoEncontradoException();

    // Verificar se cliente está ativo
    if (!cliente.Ativo)
      throw new ClienteJaInativoException();

    // Verificar se o valor é o mesmo
    if (cliente.ValorMensal == request.NovoValorMensal)
    {
      return new AlterarValorMensalResponse
      {
        ClienteId = cliente.Id,
        ValorMensalAnterior = cliente.ValorMensal,
        ValorMensalNovo = cliente.ValorMensal,
        DataAlteracao = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        Mensagem = "O novo valor mensal é igual ao valor atual."
      };
    }

    // Salvar valor anterior
    var valorAnterior = cliente.ValorMensal;

    // Criar registro de histórico
    var historico = new HistoricoValorMensal(clienteId, valorAnterior, request.NovoValorMensal);
    await _historicoRepository.AddAsync(historico);

    // Atualizar valor mensal
    cliente.AtualizarValorMensal(request.NovoValorMensal);

    // Salvar todas as alterações
    await _clienteRepository.SaveChangesAsync();

    return new AlterarValorMensalResponse
    {
      ClienteId = cliente.Id,
      ValorMensalAnterior = valorAnterior,
      ValorMensalNovo = cliente.ValorMensal,
      DataAlteracao = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
      Mensagem = "Valor mensal atualizado. O novo valor será considerado a partir da próxima data de compra."
    };
  }
}
