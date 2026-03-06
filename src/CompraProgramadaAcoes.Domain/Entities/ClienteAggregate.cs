using CompraProgramadaAcoes.Domain.Events;
using CompraProgramadaAcoes.Domain.ValueObjects;

namespace CompraProgramadaAcoes.Domain.Entities;

public class ClienteAggregate : IAggregateRoot
{
  public Guid Id { get; private set; }
  public string Nome { get; private set; } = string.Empty;
  public CPF Cpf { get; private set; } = null!;
  public Email Email { get; private set; } = null!;
  public ValorMonetario ValorMensal { get; private set; }
  public bool Ativo { get; private set; }
  public DateTime DataAdesao { get; private set; }

  // Referências por ID (regra de aggregates)
  private readonly List<IDomainEvent> _domainEvents = [];

  public IReadOnlyCollection<IDomainEvent> GetUncommittedEvents() => _domainEvents.AsReadOnly();
  public void ClearUncommittedEvents() => _domainEvents.Clear();

  public ClienteAggregate(string nome, CPF cpf, Email email, ValorMonetario valorMensal)
  {
    Id = Guid.NewGuid();
    Nome = nome;
    Cpf = cpf;
    Email = email;
    ValorMensal = valorMensal;
    Ativo = true;
    DataAdesao = DateTime.UtcNow;

    AddDomainEvent(new ClienteCriadoEvent(Id, Nome, Cpf, Email, ValorMensal, DataAdesao));
  }

  public void AlterarValorMensal(ValorMonetario novoValor)
  {
    if (!Ativo)
      throw new InvalidOperationException("Não é possível alterar valor mensal de cliente inativo");

    if (novoValor <= 0)
      throw new ArgumentException("Valor mensal deve ser positivo");

    var valorAnterior = ValorMensal;
    ValorMensal = novoValor;

    AddDomainEvent(new ValorMensalAlteradoEvent(Id, valorAnterior, ValorMensal, DateTime.UtcNow));
  }

  public void Desativar(string motivo = "Solicitação do cliente")
  {
    if (!Ativo) return; // Já está inativo

    Ativo = false;
    AddDomainEvent(new ClienteDesativadoEvent(Id, motivo, DateTime.UtcNow));
  }

  public void Reativar()
  {
    if (Ativo) return; // Já está ativo

    Ativo = true;
  }

  public void Investir(ValorMonetario valorInvestido, Dictionary<Ticker, int> ativosDistribuidos)
  {
    if (!Ativo)
      throw new InvalidOperationException("Cliente inativo não pode investir");

    if (valorInvestido <= 0)
      throw new ArgumentException("Valor investido deve ser positivo");

    if (!ativosDistribuidos.Any())
      throw new ArgumentException("Deve haver pelo menos um ativo distribuído");

    AddDomainEvent(new InvestimentoRealizadoEvent(Id, valorInvestido, ativosDistribuidos, DateTime.UtcNow));
  }

  private void AddDomainEvent(IDomainEvent domainEvent)
  {
    _domainEvents.Add(domainEvent);
  }
}
