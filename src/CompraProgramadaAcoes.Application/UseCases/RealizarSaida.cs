using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;

namespace CompraProgramadaAcoes.Application.UseCases;

public class RealizarSaida(IClienteRepository clienteRepository)
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async Task<SaidaResponse> ExecuteAsync(int clienteId)
    {
        // Buscar cliente
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null)
            throw new ClienteNaoEncontradoException();

        // Verificar se já está inativo
        if (!cliente.Ativo)
        {
            return new SaidaResponse
            {
                ClienteId = cliente.Id,
                Nome = cliente.Nome,
                Ativo = cliente.Ativo,
                DataSaida = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm"),
                Mensagem = "Cliente já está inativo."
            };
        }

        // Inativar cliente
        cliente.Desativar();
        
        // Salvar alterações
        await _clienteRepository.SaveChangesAsync();

        return new SaidaResponse
        {
            ClienteId = cliente.Id,
            Nome = cliente.Nome,
            Ativo = false,
            DataSaida = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm"),
            Mensagem = "Adesão encerrada. Sua posição em custódia foi mantida."
        };
    }
}
