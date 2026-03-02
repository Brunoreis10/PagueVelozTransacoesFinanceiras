# 💳 TransacoesFinanceiras

Backend moderno em .NET 9 para processamento de transações financeiras, estruturado com **Clean Architecture + DDD + CQRS**, garantindo separação de responsabilidades, testabilidade e escalabilidade.

---

## 📌 Visão Geral

O sistema implementa um motor de processamento de transações financeiras com suporte a:

- Crédito
- Débito
- Reserva
- Captura
- Reversão
- Transferência
- Idempotência via `reference_id`
- Controle de concorrência (lock otimista com EF Core)
- Retry com backoff exponencial (Polly)
- Logs estruturados com Serilog
- Health Checks
- Métricas Prometheus
- Testes unitários e de integração
- Execução via Docker Compose com PostgreSQL

---

## 🏗️ Arquitetura

O projeto segue **Clean Architecture (Onion Architecture)** com princípios de DDD, SOLID e CQRS.

```
TransacoesFinanceiras
│
├── src
│   ├── Backend
│   │   ├── TransacoesFinanceiras.API
│   │   ├── TransacoesFinanceiras.Application
│   │   ├── TransacoesFinanceiras.Domain
│   │   └── TransacoesFinanceiras.Infrastructure
│   │
│   └── Shared
│       └── TransacoesFinanceiras.Exceptions
│
└── Tests
    └── TransacoesFinanceiras.Tests
```

---

## 🔎 Camadas

### 🧠 Domain
- Entidades, Enums, Value Objects
- Interfaces de Repositório
- Regras de negócio e Eventos de domínio
- **Não depende de nenhuma outra camada.**

### 📦 Application
- Commands, Queries, Handlers (MediatR)
- DTOs, Validações (FluentValidation)
- Pipeline Behaviors
- **Depende apenas de Domain.**

### 🏗 Infrastructure
- Entity Framework Core + PostgreSQL
- Repositórios concretos
- Resiliência (Polly) e Logging
- **Depende de Application e Domain.**

### 🌐 API
- Controllers, Swagger, Health Checks, Prometheus
- Configuração de DI, Serilog, Dockerfile
- **Depende apenas de Application.**

### 🔁 Shared
- Exceptions customizadas
- Resource file (`ResourceMessagesException.resx`)
- Mensagens padronizadas

---

## 🛠 Tecnologias Utilizadas

| Categoria | Tecnologia |
|---|---|
| Runtime | .NET 9 / ASP.NET Core 9 |
| ORM | Entity Framework Core 9 |
| Banco de Dados | PostgreSQL 16 |
| Mensageria Interna | MediatR |
| Validação | FluentValidation |
| Resiliência | Polly |
| Logs | Serilog |
| Métricas | Prometheus |
| Documentação | Swagger / OpenAPI |
| Testes | xUnit, Moq, FluentAssertions |
| Infraestrutura | Docker / Docker Compose |

---

## 🚀 Execução Local (Sem Docker)

### Pré-requisitos
- .NET 9 SDK
- PostgreSQL 16+

### 1️⃣ Configurar banco

Certifique-se que o PostgreSQL esteja rodando e edite o `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=admin"
}
```

### 2️⃣ Restaurar dependências

```bash
dotnet restore
```

### 3️⃣ Aplicar migrations

```bash
cd src/Backend/TransacoesFinanceiras.API
dotnet ef database update
```

### 4️⃣ Executar API

```bash
dotnet run
```

Swagger disponível em: `https://localhost:5001/swagger`

---

## 🐳 Execução com Docker (Recomendado)

O projeto possui `docker-compose.yml` configurado com:
- API (.NET 9)
- PostgreSQL 16
- Healthcheck
- Volume persistente para banco
- Migrations automáticas no startup

### Pré-requisitos
- Docker Desktop instalado

### Subir ambiente completo

Na raiz do projeto:

```bash
docker compose up -d --build
```

### Acessar aplicação

| Serviço | URL |
|---|---|
| Swagger | http://localhost:5000/swagger |
| Health Check | http://localhost:5000/health |
| Prometheus metrics | http://localhost:5000/metrics |

### Banco de dados (via Docker)

| Parâmetro | Valor |
|---|---|
| Host | localhost |
| Porta | 5433 |
| Database | postgres |
| User | postgres |
| Password | admin |

### Parar ambiente

```bash
docker compose down
```

Para remover volumes:

```bash
docker compose down -v
```

---

## 🔄 Migrations

As migrations são aplicadas automaticamente na inicialização da API (com retry resiliente).

Para rodar manualmente:

```bash
dotnet ef database update \
  --project src/Backend/TransacoesFinanceiras.Infrastructure \
  --startup-project src/Backend/TransacoesFinanceiras.API
```

---

## 🧪 Testes

Executar todos:

```bash
dotnet test
```

Com cobertura:

```bash
dotnet test /p:CollectCoverage=true
```

Estrutura:

```
Tests/
├── UnitTest
└── IntegrationTest
```

---

## 💰 Regras de Negócio

- Saldo disponível nunca pode ficar negativo
- Respeita limite de crédito
- Reservas exigem saldo disponível
- Captura exige saldo reservado
- Operações são atômicas
- Idempotência por `reference_id`
- Retry automático em conflito de concorrência

---

## 📊 Observabilidade

- Logs estruturados (JSON)
- Health Checks
- Prometheus Metrics
- Middleware global de tratamento de exceções

---

## 🔐 Tratamento de Erros

- Exceptions customizadas em `TransacoesFinanceiras.Exceptions`
- Resource file para mensagens padronizadas
- Respostas HTTP padronizadas

---

## 🎯 Princípios Aplicados

SOLID · Clean Architecture · DDD · CQRS · Inversão de Dependência · Separação clara de responsabilidades

---

## 📈 Evolução Arquitetural

A arquitetura permite:

- Separação futura em microserviços
- Integração com mensageria (RabbitMQ, Kafka)
- Escalabilidade horizontal
- Adição de autenticação JWT
- Deploy em ambientes cloud (Azure / AWS)

---

## 🌐 Valores pra teste dos endpoints da API

### Contas

#### `POST /api/Accounts`
Cria uma nova conta.

**Request:**
```json
{
  "clientId": "CLI-001",
  "initialBalance": 0,
  "creditLimit": 50000
}
```

**Response:**
```json
{
  "accountId": "ACC-86924fb8baa9428080addd354eb64f64",
  "clientId": "CLI-001",
  "balance": 0,
  "reservedBalance": 0,
  "creditLimit": 50000,
  "availableBalance": 50000,
  "status": 1
}
```

#### `GET /api/Accounts/{id}`
Obtém uma conta por ID.

#### `GET /api/Accounts/{id}/transactions`
Obtém todas as transações de uma conta.

---

### Transações

#### `POST /api/Transactions`
Cria uma nova transação financeira.

**Request (Crédito — operation: 1):**
```json
{
  "operation": 1,
  "accountId": "ACC-86924fb8baa9428080addd354eb64f64",
  "destinationAccountId": "string",
  "amount": 100000,
  "currency": "BRL",
  "referenceId": "TXN-001",
  "originalReferenceId": "string",
  "metadata": {
    "description": "Depósito inicial"
  }
}
```

**Request (Débito — operation: 2):**
```json
{
  "operation": 2,
  "accountId": "ACC-86924fb8baa9428080addd354eb64f64",
  "destinationAccountId": "string",
  "amount": 50000,
  "currency": "BRL",
  "referenceId": "TXN-002",
  "originalReferenceId": "string",
  "metadata": {
    "description": "Teste"
  }
}
```

**Request (Reserva — operation: 3):**
```json
{
  "operation": 3,
  "accountId": "ACC-86924fb8baa9428080addd354eb64f64",
  "destinationAccountId": "string",
  "amount": 30000,
  "currency": "BRL",
  "referenceId": "TXN-003",
  "originalReferenceId": "string",
  "metadata": {
    "description": "Teste"
  }
}
```

**Request (Captura — operation: 4):**
```json
{
  "operation": 4,
  "accountId": "ACC-86924fb8baa9428080addd354eb64f64",
  "destinationAccountId": "string",
  "amount": 30000,
  "currency": "BRL",
  "referenceId": "TXN-004",
  "originalReferenceId": "string",
  "metadata": {
    "description": "Teste"
  }
}
```

**Request (Reversão — operation: 5):**
```json
{
  "operation": 5,
  "accountId": "ACC-86924fb8baa9428080addd354eb64f64",
  "destinationAccountId": "string",
  "amount": 0,
  "currency": "BRL",
  "referenceId": "TXN-005",
  "originalReferenceId": "TXN-001",
  "metadata": {
    "description": "Teste"
  }
}
```

**Request (Transferência — operation: 6):**
```json
{
  "operation": 6,
  "accountId": "ACC-86924fb8baa9428080addd354eb64f64",
  "destinationAccountId": "ACC-39568e08a8d4443ba37bd72a5c71154d",
  "amount": 20000,
  "currency": "BRL",
  "referenceId": "TXN-006",
  "originalReferenceId": "string",
  "metadata": {
    "description": "Teste"
  }
}
```

---

## 📄 Licença

Projeto desenvolvido para fins educacionais e demonstração técnica.
