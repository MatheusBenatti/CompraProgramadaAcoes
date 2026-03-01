using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Mappers
{
  public class AdesaoResponseMap
  {
    private const string Format = "yyyy-MM-ddTHH:mm:ss";
    public static AdesaoResponse MapearParaResponse(Cliente cliente, ContaGrafica contaGrafica) => new()
    {
      ClienteId = cliente.Id,
      Nome = cliente.Nome,
      Cpf = cliente.Cpf,
      Email = cliente.Email,
      ValorMensal = cliente.ValorMensal,
      Ativo = cliente.Ativo,
      DataAdesao = cliente.DataAdesao.ToString(Format),
      ContaGrafica = new ContaGraficaResponse
      {
        Id = contaGrafica.Id,
        NumeroConta = contaGrafica.NumeroConta!,
        Tipo = contaGrafica.Tipo,
        DataCriacao = contaGrafica.DataCriacao.ToString(Format)
      }
    };
  }
}