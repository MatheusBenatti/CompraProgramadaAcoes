# Apresentação: Sistema de Compra Programada de Ações

## 📋 Sumário

1. **Visão Geral do Projeto**
2. **Arquitetura e Decisões de Design**
3. **Lógica de Implementação**
4. **Componentes Principais**
5. **Padrões e Boas Práticas**
6. **Tecnologias e Infraestrutura**
7. **Desafios e Soluções**
8. **Demonstração Prática**

---

## 1. 🎯 Visão Geral do Projeto

### Objetivo Principal
Sistema automatizado de investimento para compra programada de ações com foco em **cestas diversificadas do mercado brasileiro**.

### Problema Resolvido
- **Dificuldade de investidores iniciantes** em montar carteiras diversificadas
- **Complexidade de gestão** de múltiplos ativos
- **Disciplina de investimento** com aportes mensais automáticos
- **Otimização de custos** através de compras consolidadas

### Valor Proposto
- **Acessibilidade**: Investimento a partir de valores mensais acessíveis
- **Diversificação**: Cestas "Top Five" com 5 ativos selecionados
- **Automação**: Execução sem intervenção manual
- **Conformidade**: Cálculo automático de imposto de renda

---

## 2. 🏗️ Arquitetura e Decisões de Design

### Clean Architecture: Por quê?

```
┌─────────────────────────────────────────────────────────┐
│                    API Layer                              │
│  Controllers, Swagger, Middleware                        │
├─────────────────────────────────────────────────────────┤
│                  Application Layer                       │
│  Use Cases, Services, DTOs, Interfaces                   │
├─────────────────────────────────────────────────────────┤
│                    Domain Layer                          │
│  Entities, Value Objects, Business Rules                 │
├─────────────────────────────────────────────────────────┤
│                 Infrastructure Layer                     │
│  Database, Cache, Messaging, External Services          │
└─────────────────────────────────────────────────────────┘
```

### Decisões Arquiteturais

#### **Domain-Driven Design (DDD)**
- **Razão**: Complexidade do domínio financeiro exige modelagem rica
- **Benefícios**: 
  - Regras de negócio centralizadas
  - Linguagem ubíqua clara
  - Entidades com comportamento próprio

#### **Event-Driven Architecture**
- **Razão**: Desacoplamento entre sistemas e assincronia
- **Implementação**: Apache Kafka para eventos de domínio
- **Benefícios**:
  - Resiliência e escalabilidade
  - Integração com sistemas fiscais
  - Auditoria completa

#### **CQRS (Command Query Responsibility Segregation)**
- **Razão**: Separação clara entre leitura e escrita
- **Implementação**: Use Cases para comandos, Services para consultas
- **Benefícios**:
  - Performance otimizada
  - Escalabilidade independente
  - Código mais maintainable

---

## 3. 🧠 Lógica de Implementação

### Motor de Compras: O Coração do Sistema

#### **1. Agendamento Inteligente**
```csharp
public Task<bool> DeveExecutarHoje(DateTime data)
{
    // Dias 5, 15, 25 têm prioridade (mesmo que fim de semana)
    var ehDiaDeExecucao = data.Day == 5 || data.Day == 15 || data.Day == 25;
    
    if (ehDiaDeExecucao) return Task.FromResult(true);
    
    // Verificar dia útil (segunda a sexta)
    if (data.DayOfWeek == DayOfWeek.Saturday || data.DayOfWeek == DayOfWeek.Sunday)
        return Task.FromResult(false);
        
    return Task.FromResult(false);
}
```

**Raciocínio**: 
- **Dias fixos** garantem previsibilidade para clientes
- **Prioridade sobre finais de semana** mantém regularidade
- **Dias úteis** como fallback para liquidez

#### **2. Consolidação de Compras**
```csharp
var valorTotalAporte = clientesAtivos.Sum(c => c.ValorMensal / 3);
var comprasConsolidadas = CalcularComprasConsolidadas(cestaVigente, precos, valorTotalAporte);
```

**Raciocínio**:
- **1/3 do valor mensal**: 3 execuções mensais (5, 15, 25)
- **Compra consolidada**: Melhor taxa por volume
- **Redução de custos operacionais**

#### **3. Distribuição Proporcional**
```csharp
foreach (var cliente in clientes)
{
    var valorAporteCliente = cliente.ValorMensal / 3;
    var percentualCliente = valorAporteCliente / totalAportes;
    
    // Distribuir baseado na proporção do cliente
    var quantidadeCliente = (int)Math.Floor(valorAporteAtivo / ordem.PrecoUnitario);
}
```

**Raciocínio**:
- **Proporcionalidade**: Cada cliente recebe na proporção de seu aporte
- **Precisão matemática**: Cálculo exato de percentuais
- **Arredondamento para baixo**: Evita frações de ações

### Sistema de Custódia

#### **Preço Médio Ponderado**
```csharp
var quantidadeAnterior = custodiaCliente.Quantidade;
var valorAnterior = quantidadeAnterior * custodiaCliente.PrecoMedio;
var valorNovo = quantidadeCliente * ordem.PrecoUnitario;
var quantidadeTotal = quantidadeAnterior + quantidadeCliente;
var precoMedioNovo = (valorAnterior + valorNovo) / quantidadeTotal;
```

**Raciocínio**:
- **Método matemático correto**: Preço médio ponderado
- **Acumulação**: Mantém histórico de todas as compras
- **Base para cálculo de IR**: Preço médio é essencial

---

## 4. 🏛️ Componentes Principais

### Domain Layer: O Cérebro do Negócio

#### **ClienteAggregate**
```csharp
public class ClienteAggregate : IAggregateRoot
{
    public void Investir(ValorMonetario valorInvestido, Dictionary<Ticker, int> ativosDistribuidos)
    {
        if (!Ativo) throw new InvalidOperationException("Cliente inativo não pode investir");
        
        AddDomainEvent(new InvestimentoRealizadoEvent(Id, valorInvestido, ativosDistribuidos, DateTime.UtcNow));
    }
}
```

**Design Pattern**: **Aggregate Root**
- **Consistência**: Garante integridade do cliente
- **Eventos**: Dispara eventos automaticamente
- **Invariantes**: Protege regras de negócio

#### **Value Objects: CPF e Ticker**
```csharp
public sealed record CPF(string Valor)
{
    public static CPF Create(string cpf)
    {
        if (!IsValid(cpf)) throw new ArgumentException("CPF inválido");
        return new CPF(cpfLimpo);
    }
}
```

**Design Pattern**: **Value Object**
- **Imutabilidade**: Não pode ser alterado após criação
- **Validação**: Regras de negócio embutidas
- **Type Safety**: Evita strings mágicas

### Application Layer: Orquestração

#### **Use Cases: Intenções Claras**
- `RealizarAdesao`: Novo cliente entra no sistema
- `ExecutarCompraUseCase`: Motor principal de compras
- `CadastrarCestaUseCase`: Admin configura carteiras

#### **Services: Coordinators**
```csharp
public class MotorCompraProgramada : IMotorCompraProgramada
{
    // Orquestra todo o processo de compra
    // Coordena múltiplos repositórios
    // Gerencia eventos de domínio
}
```

### Infrastructure Layer: Implementação Concreta

#### **Repository Pattern**
```csharp
public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id);
    Task<List<Cliente>> ObterClientesAtivosAsync();
    Task AddAsync(Cliente cliente);
}
```

**Benefícios**:
- **Desacoplamento**: Domain não conhece o banco
- **Testabilidade**: Facilita mocks
- **Swapability**: Troca de implementação

#### **Event Handlers: Reação a Eventos**
```csharp
public class OrdemCompraEventHandler : IEventHandler<OrdemCompraCriadaEvent>
{
    public async Task Handle(OrdemCompraCriadaEvent notification)
    {
        // Publicar no Kafka para sistema fiscal
        await _messagePublisher.PublishAsync("ordens-compra", JsonSerializer.Serialize(eventData));
    }
}
```

---

## 5. 🎨 Padrões e Boas Práticas

### SOLID Principles

#### **Single Responsibility**
- `MotorCompraProgramada`: Apenas orquestra compras
- `CotacaoService`: Apenas busca cotações
- `KafkaPublisher`: Apenas publica mensagens

#### **Open/Closed**
- `ICotacaoProvider`: Interface aberta para extensão
- `ITipoMercado`: Enumerado fechado para modificação

#### **Dependency Inversion**
```csharp
public class MotorCompraProgramada(
    IClienteRepository clienteRepository,
    ICustodiaRepository custodiaRepository,
    // ... outras dependências
)
```

### Design Patterns Implementados

#### **Strategy Pattern**: Tipos de Mercado
```csharp
public enum TipoMercado
{
    Lote = 1,        // Lote padrão (100 ações)
    Fracionario = 2  // Fracionário
}
```

#### **Factory Pattern**: Criação de Entidades
```csharp
public class ClienteFactory
{
    public static ClienteAggregate Criar(string nome, CPF cpf, Email email, ValorMonetario valor)
    {
        return new ClienteAggregate(nome, cpf, email, valor);
    }
}
```

#### **Observer Pattern**: Domain Events
```csharp
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

public class ClienteCriadoEvent : IDomainEvent
{
    // Evento disparado automaticamente
}
```

---

## 6. 🛠️ Tecnologias e Infraestrutura

### Stack Tecnológico

#### **Backend**
- **.NET 9.0**: Framework moderno e performático
- **Entity Framework Core**: ORM para persistência
- **MySQL 8.0**: Banco de dados relacional

#### **Cache e Performance**
- **Redis**: Cache distribuído para cesta e cotações
- **In-Memory Cache**: Para dados voláteis

#### **Mensageria**
- **Apache Kafka**: Event streaming e comunicação assíncrona
- **Kafka UI**: Interface de monitoramento

#### **Containerização**
- **Docker**: Ambientes consistentes
- **Docker Compose**: Orquestração local

### Arquitetura de Dados

#### **Schema Principal**
```sql
Clientes                 -- Dados pessoais e configurações
ContasGraficas          -- Contas de investimento
CestasRecomendacao      -- Carteiras recomendadas
ItensCesta              -- Composição das cestas
OrdensCompra            -- Ordens executadas
Distribuicoes           -- Distribuição para clientes
Custodias               -- Posições atuais
Cotacoes                -- Preços históricos
EventosIR               -- Eventos fiscais
```

#### **Relacionamentos Chave**
- **Cliente 1:N ContaGrafica**: Cada cliente tem uma conta
- **Cesta 1:N ItensCesta**: Cada cesta tem 5 ativos
- **OrdemCompra 1:N Distribuicao**: Cada ordem distribui para múltiplos clientes

---

## 7. 🚧 Desafios e Soluções

### Desafio 1: Consistência de Dados

**Problema**: Múltiplas operações precisam ser atômicas
**Solução**: Unit of Work Pattern com Entity Framework

```csharp
await _custodiaRepository.SaveChangesAsync();
// Todas as operações são commitadas juntas
```

### Desafio 2: Performance em Alta Escala

**Problema**: Milhares de clientes simultâneos
**Solução**: 
- **Redis Cache** para cesta e cotações
- **Compras consolidadas** para reduzir operações
- **Processamento assíncrono** com Kafka

### Desafio 3: Cálculo de Imposto de Renda

**Problema**: Complexidade das regras fiscais brasileiras
**Solução**: 
- **Eventos dedicados** para IR
- **Cálculo automático** de dedo-duro (0,005%)
- **Integração** com sistema fiscal via Kafka

### Desafio 4: Testabilidade

**Problema**: Complexidade dos testes com Entity Framework
**Solução**: 
- **InMemory Database** para testes de repositório
- **Mocks focados** em comportamento essencial
- **Testes de integração** separados

---

## 8. 🎭 Demonstração Prática

### Fluxo Completo de Compra

#### **1. Agendamento**
```
Dia 5 do mês → Verificação → Início do processo
```
**Arquivos Responsáveis:**
- `src/CompraProgramadaAcoes.Workers/MotorCompraWorker.cs` - Agendamento mensal
- `src/CompraProgramadaAcoes.Application/Services/MotorCompraProgramada.cs` - Lógica de verificação

#### **2. Coleta de Dados**
```
Clientes Ativos → Cesta Vigente (Redis) → Cotações (Redis)
```
**Arquivos Responsáveis:**
- `src/CompraProgramadaAcoes.Infrastructure/Repositories/ClienteRepository.cs` - Clientes ativos
- `src/CompraProgramadaAcoes.Infrastructure/Cache/CestaCacheService.cs` - Cesta vigente em Redis
- `src/CompraProgramadaAcoes.Infrastructure/Cache/CotacaoCacheService.cs` - Cotações em Redis

#### **3. Cálculo**
```
Valor Total = Σ(ValorMensal/3) → 
Compras Consolidadas → 
Ajuste com Saldo Master
```
**Arquivos Responsáveis:**
- `src/CompraProgramadaAcoes.Application/Services/MotorCompraProgramada.cs` - Cálculo do valor total
- `src/CompraProgramadaAcoes.Application/Analyzers/TopFiveAnalyzer.cs` - Análise de ativos
- `src/CompraProgramadaAcoes.Infrastructure/Repositories/ContaMasterRepository.cs` - Saldo master

#### **4. Execução**
```
Ordens de Compra → 
Distribuição Proporcional → 
Atualização de Custódia
```
**Arquivos Responsáveis:**
- `src/CompraProgramadaAcoes.Domain/Entities/OrdemCompra.cs` - Entidade da ordem
- `src/CompraProgramadaAcoes.Application/Services/DistribuicaoService.cs` - Distribuição proporcional
- `src/CompraProgramadaAcoes.Infrastructure/Repositories/CustodiaRepository.cs` - Atualização de custódia

#### **5. Eventos**
```
IR Dedo-duro → Kafka → Sistema Fiscal
```
**Arquivos Responsáveis:**
- `src/CompraProgramadaAcoes.Domain/Events/InvestimentoRealizadoEvent.cs` - Evento de investimento
- `src/CompraProgramadaAcoes.Infrastructure/Message/KafkaPublisher.cs` - Publicação no Kafka
- `src/CompraProgramadaAcoes.Infrastructure/EventHandlers/OrdemCompraEventHandler.cs` - Processamento de eventos
- `src/CompraProgramadaAcoes.Infrastructure/Repositories/EventoIRRepository.cs` - Persistência de eventos IR

### API Endpoints

#### **Administração**
```http
POST /api/admin/cesta              # Cadastrar cesta
GET  /api/admin/cesta/atual        # Cesta vigente
GET  /api/admin/conta-master/custodia # Custódia master
```

#### **Clientes**
```http
POST /api/clientes/adesao          # Novo cliente
GET  /api/clientes/{id}/carteira   # Posição atual
PUT  /api/clientes/{id}/valor-mensal # Alterar aporte
```

#### **Motor**
```http
POST /api/motor/executar-compra    # Execução manual
```

---

## 🔮 Próximos Passos

### Melhorias Planejadas

1. **Inteligência Artificial**
   - Análise preditiva para melhor seleção de ativos
   - Otimização automática de carteiras

2. **Expansão de Mercados**
   - Suporte para ETFs e Fundos Imobiliários
   - Mercado internacional (Ações americanas)

3. **Mobile First**
   - App nativo para iOS e Android
   - Notificações push de eventos

4. **Governança Avançada**
   - KYC automático
   - Compliance regulatório avançado

### Métricas de Sucesso

- **Adoção**: Número de clientes ativos
- **Performance**: Tempo de execução do motor
- **Satisfação**: Retenção de clientes
- **Operacional**: Taxa de erro nas operações

---

## 📊 Conclusão

### O que tornou este projeto especial:

1. **Arquitetura Robusta**: Clean Architecture com DDD
2. **Lógica Sofisticada**: Motor de compras otimizado
3. **Performance**: Cache e processamento assíncrono
4. **Conformidade**: Cálculo automático de IR
5. **Escalabilidade**: Arquitetura preparada para crescimento

### Principais Aprendizados:

- **Design Patterns** são essenciais para complexidade
- **Event-Driven** arquitetura possibilita escalabilidade
- **Testes automatizados** garantem qualidade
- **Documentação clara** facilita manutenção

### Impacto no Negócio:

- **Democratização** do investimento em ações
- **Redução** de barreiras de entrada
- **Automação** de processo complexo
- **Conformidade** regulatória garantida

---

## 🙋‍♂️ Perguntas e Respostas

**Obrigado pela atenção!**
