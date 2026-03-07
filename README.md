# Compra Programada Ações

Sistema de investimento automatizado para compra programada de ações com foco em cestas diversificadas do mercado brasileiro.

## 🎯 Visão Geral

Projeto desenvolvido para o Desafio que implementa um sistema completo de compra programada de ações.

- **Gestão de Clientes:** Adesão, saída e manutenção de contas
- **Cestas Top Five:** Gestão de carteiras recomendadas com 5 ativos
- **Motor de Compras:** Execução automatizada de investimentos
- **Rebalanceamento:** Ajuste automático de carteiras
- **Cálculo de IR:** Geração de eventos para imposto de renda

### Exemplo de Uso

#### Youtube
- url: https://www.youtube.com/


## 🏗️ Arquitetura

### Clean Architecture
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

### Tecnologias
- **.NET 9.0** - Framework principal
- **MySQL 8.0** - Banco de dados relacional
- **Redis** - Cache distribuído
- **Apache Kafka** - Mensageria e eventos
- **Docker** - Containerização
- **Swagger/OpenAPI** - Documentação da API

## 🚀 Quick Start

### Pré-requisitos
- Docker Desktop
- .NET 9.0 SDK
- Git

### 1. Clonar o Projeto
```bash
git clone <repository-url>
cd CompraProgramadaAcoes
```

### 2. Iniciar Infraestrutura
```bash
docker-compose up -d mysql redis zookeeper kafka kafka-ui
```

### 3. Executar a API
```bash
docker-compose up api
```

### Coverage
dotnet test "tests/CompraProgramadaAcoes.UnitTests/CompraProgramadaAcoes.UnitTests.csproj" --settings:tests/coverage.runsettings --collect:"XPlat Code Coverage”

### Gerar html
reportgenerator "-reports:tests/CompraProgramadaAcoes.UnitTests/TestResults/**/coverage.cobertura.xml" "-targetdir:tests/CompraProgramadaAcoes.UnitTests/TestResults/Report" "-reporttypes:Html"

### Abrir relatorio
Start-Process "{caminho arquivo}"


### 4. Acessar Documentação
- **Swagger UI:** http://localhost:5070
- **Kafka UI:** http://localhost:8081

## 📁 Estrutura do Projeto

```
CompraProgramadaAcoes/
├── src/
│   ├── CompraProgramadaAcoes.Api/           # Web API
│   ├── CompraProgramadaAcoes.Application/    # Camada de Aplicação
│   ├── CompraProgramadaAcoes.Domain/         # Domínio
│   ├── CompraProgramadaAcoes.Infrastructure/ # Infraestrutura
│   └── CompraProgramadaAcoes.Workers/        # Background Services
├── tests/                                    # Testes Unitários
├── docs/                                     # Documentação
├── docker-compose.yml                        # Orquestração Docker
└── README.md                                 # Este arquivo
```


## 📊 Funcionalidades

### 1. Gestão de Clientes
- **Adesão:** Novos clientes podem aderir ao sistema
- **Saída:** Clientes podem solicitar saída do programa
- **Manutenção:** Alteração de valor mensal de investimento
- **Consultas:** Carteira de ativos e rentabilidade

### 2. Cestas Top Five
- **Cadastro:** Administradores cadastram cestas com 5 ativos
- **Validação:** 100% de distribuição obrigatória
- **Histórico:** Controle de versões das cestas
- **Cache:** Performance com Redis

### 3. Motor de Compras
- **Agendamento:** Dias 5, 15 e 25 de cada mês
- **Dias Úteis:** Considera finais de semana
- **Consolidação:** Agrupa compras para melhor taxa
- **Distribuição:** Reparte ativos entre clientes

### 4. Rebalanceamento
- **Mudança de Cesta:** Vende ativos removidos, compra novos
- **Desvio Proporção:** Ajusta carteiras fora do alvo
- **Controle Manual:** Execução sob demanda do admin

### 5. Cálculo de IR
- **Dedu-duro:** 0,005% sobre operações
- **Venda:** 20% sobre lucros (acima de R$ 20k/mês)
- **Eventos:** Publicados no Kafka para sistema fiscal

## 📚 API Documentation

### Endpoints Principais

#### Admin (`/api/admin`)
```http
POST   /api/admin/cesta              # Cadastrar/alterar cesta
GET    /api/admin/cesta/atual        # Consultar cesta vigente
GET    /api/admin/cesta/historico    # Histórico de cestas
GET    /api/admin/conta-master/custodia # Custódia master
```

#### Clientes (`/api/clientes`)
```http
POST   /api/clientes/adesao          # Adesão de cliente
POST   /api/clientes/{id}/saida      # Saída de cliente
PUT    /api/clientes/{id}/valor-mensal # Alterar valor
GET    /api/clientes/{id}/carteira   # Consultar carteira
GET    /api/clientes/{id}/rentabilidade # Rentabilidade
```

#### Motor (`/api/motor`)
```http
POST   /api/motor/executar-compra    # Executar compra (testes)
```

#### Rebalanceamento (`/api/rebalanceamento`)
```http
POST   /api/rebalanceamento/mudanca-cesta # Rebalancear por mudança
```

## 🗄️ Banco de Dados

### Schema Principal
```sql
Clientes                 -- Dados dos clientes
ContasGraficas          -- Contas de cada cliente
CestasRecomendacao      -- Cestas Top Five
ItensCesta              -- Ativos das cestas
OrdensCompra            -- Ordens executadas
Distribuicoes           -- Distribuição para clientes
Custodias               -- Posições dos clientes
Cotacoes                -- Preços dos ativos
EventosIR               -- Eventos fiscais
```

### Migrations
As migrations são executadas automaticamente na inicialização da API.


