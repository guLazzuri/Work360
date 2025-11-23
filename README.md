# Work360 API

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-9.0-512BD4)
![Oracle](https://img.shields.io/badge/Oracle-Database-F80000?logo=oracle)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?logo=swagger)

API RESTful para gerenciamento de produtividade e acompanhamento de tarefas, reuniões e eventos de foco de trabalho. O Work360 é uma solução completa para organização profissional com recursos de relatórios e análise de produtividade.

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Arquitetura](#arquitetura)
- [Entidades](#entidades)
- [Endpoints da API](#endpoints-da-api)
- [Configuração e Instalação](#configuração-e-instalação)
- [Migrations](#migrations)
- [Testes](#testes)
- [Health Check](#health-check)
- [Observabilidade](#observabilidade)
- [HATEOAS](#hateoas)

## 🎯 Sobre o Projeto

Work360 é uma API desenvolvida em .NET 9 que permite aos usuários gerenciar suas atividades profissionais de forma eficiente. O sistema oferece:

- ✅ Gerenciamento de tarefas com prioridades e situações
- 📅 Controle de reuniões com duração automática
- 🎯 Monitoramento de sessões de foco
- 📊 Relatórios detalhados de produtividade
- 💡 Insights sobre risco de burnout e tendências

## 🚀 Tecnologias Utilizadas

### Core
- **.NET 9.0** - Framework principal
- **ASP.NET Core** - Web API
- **C#** - Linguagem de programação

### Banco de Dados
- **Entity Framework Core 9.0** - ORM
- **Oracle Database** - Banco de dados relacional
- **Oracle.EntityFrameworkCore 9.23.26000** - Provider para Oracle

### Monitoramento e Observabilidade
- **OpenTelemetry** - Distributed tracing
- **HealthChecks** - Verificação de saúde da aplicação
- **Logging** integrado com console e debug

### Testes
- **xUnit** - Framework de testes
- **Moq** - Library para mocking
- **InMemory Database** - Testes com banco em memória

### Outros
- **Swagger/OpenAPI** - Documentação interativa
- **API Versioning** - Versionamento de API
- **HATEOAS** - Hypermedia as the Engine of Application State

## 🏗️ Arquitetura

O projeto segue uma arquitetura em camadas organizadas da seguinte forma:

```
Work360/
│
├── Controller/                 # Controladores da API
│   ├── UserController.cs
│   ├── TasksController.cs
│   ├── MeetingsController.cs
│   ├── EventsController.cs
│   └── ReportController.cs
│
├── Domain/                     # Camada de domínio
│   ├── Entity/                 # Entidades do domínio
│   │   ├── User.cs
│   │   ├── Tasks.cs
│   │   ├── Meeting.cs
│   │   ├── Events.cs
│   │   └── Report.cs
│   ├── DTO/                    # Data Transfer Objects
│   │   ├── PageResult.cs
│   │   └── LinkDto.cs
│   └── Enum/                   # Enumerações
│       ├── EventType.cs
│       ├── Priority.cs
│       └── TaskSituation.cs
│
├── Infrastructure/             # Camada de infraestrutura
│   ├── Context/
│   │   └── Work360Context.cs  # DbContext do EF Core
│   ├── Services/
│   │   └── HateoasService.cs  # Serviço HATEOAS
│   ├── Health/
│   │   └── OracleHealthCheck.cs
│   ├── Attributes/
│   │   └── NotEmptyGuidAttribute.cs
│   └── Traccer/
│       └── Traccer.cs
│
├── Migrations/                 # Migrations do Entity Framework
│   ├── 20251122212344_InitialCreate.cs
│   └── Work360ContextModelSnapshot.cs
│
└── Work360.Tests/             # Projeto de testes unitários
    ├── UserControllerTest.cs
    ├── TasksControllerTests.cs
    ├── ReportControllerTests.cs
    └── HateoasServiceTests.cs
```

## 📊 Entidades

### 👤 User
Representa um usuário do sistema.

```csharp
public class User
{
    public Guid UserID { get; set; }        // Chave primária
    public string Name { get; set; }        // Nome do usuário (obrigatório)
    public string Password { get; set; }    // Senha (obrigatório)
}
```

**Tabela:** `Work360_User`

### ✅ Tasks
Representa uma tarefa a ser executada.

```csharp
public class Tasks
{
    public Guid TaskID { get; set; }           // Chave primária
    public Guid UserID { get; set; }           // ID do usuário
    public string Title { get; set; }          // Título da tarefa
    public Priority Priority { get; set; }     // LOW, MEDIUM, HIGH
    public int EstimateMinutes { get; set; }   // Tempo estimado em minutos
    public string Description { get; set; }    // Descrição
    public TaskSituation TaskSituation { get; set; }  // OPEN, IN_PROGRESS, COMPLETED
    public DateTime CreatedTask { get; set; }  // Data de criação
    public DateTime? FinalDateTask { get; set; }  // Data de conclusão
    public int SpentMinutes { get; set; }      // Tempo gasto
}
```

**Tabela:** `Work360_Tasks`

### 📅 Meeting
Representa uma reunião.

```csharp
public class Meeting
{
    public Guid MeetingID { get; set; }      // Chave primária
    public Guid UserID { get; set; }         // ID do usuário
    public string Title { get; set; }        // Título da reunião
    public string Description { get; set; }  // Descrição
    public DateTime StartDate { get; set; }  // Data/hora de início
    public DateTime? EndDate { get; set; }   // Data/hora de fim
    public int? MinutesDuration { get; set; }  // Duração em minutos
}
```

**Tabela:** `Work360_Meetings`

### 🎯 Events
Representa eventos de sessão de foco.

```csharp
public class Events
{
    public Guid EventID { get; set; }        // Chave primária
    public Guid UserID { get; set; }         // ID do usuário
    public EventType EventType { get; set; } // START_FOCUS_SESSION, END_FOCUS_SESSION
    public DateTime StartDate { get; set; }  // Data/hora de início
    public DateTime? EndDate { get; set; }   // Data/hora de fim
    public int Duration { get; set; }        // Duração em minutos
}
```

**Tabela:** `Work360_Events`

### 📈 Report
Representa relatórios de produtividade (não persistido no banco).

```csharp
public class Report
{
    public Guid UserID { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int FinishedMeetings { get; set; }
    public int FocusMinutes { get; set; }
    public double CompletionPercentage { get; set; }
    public string BurnoutRisk { get; set; }
    public string ProductivityTrend { get; set; }
    public string FocusTrend { get; set; }
    public string Insights { get; set; }
    public string AIRecommendation { get; set; }
    public string Summary { get; set; }
}
```

### 🔢 Enumerações

**EventType**
```csharp
public enum EventType
{
    START_FOCUS_SESSION,
    END_FOCUS_SESSION
}
```

**Priority**
```csharp
public enum Priority
{
    LOW,
    MEDIUM,
    HIGH
}
```

**TaskSituation**
```csharp
public enum TaskSituation
{
    OPEN,
    IN_PROGRESS,
    COMPLETED
}
```

## 🌐 Endpoints da API

A API está versionada como `v1.0` e todos os endpoints seguem o padrão:
```
/api/v1/{controller}/{action}
```

### 👤 User Endpoints

#### **GET** `/api/v1/User`
Lista todos os usuários com paginação.

**Query Parameters:**
- `pageNumber` (int, opcional, default: 1) - Número da página
- `pageSize` (int, opcional, default: 10) - Tamanho da página

**Resposta de sucesso (200):**
```json
{
  "items": [
    {
      "userID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "João Silva",
      "password": "senha123"
    }
  ],
  "currentPage": 1,
  "pageSize": 10,
  "totalItems": 1,
  "totalPages": 1,
  "links": [
    {
      "href": "/api/v1/User?pageNumber=1&pageSize=10",
      "rel": "self",
      "method": "GET"
    }
  ]
}
```

#### **GET** `/api/v1/User/{id}`
Busca um usuário específico por ID.

**Path Parameters:**
- `id` (Guid) - ID do usuário

**Respostas:**
- `200 OK` - Usuário encontrado
- `404 Not Found` - Usuário não encontrado

#### **POST** `/api/v1/User`
Cria um novo usuário.

**Body:**
```json
{
  "name": "Maria Santos",
  "password": "senha456"
}
```

**Respostas:**
- `201 Created` - Usuário criado com sucesso
- `400 Bad Request` - Dados inválidos

#### **PUT** `/api/v1/User/{id}`
Atualiza um usuário existente.

**Path Parameters:**
- `id` (Guid) - ID do usuário

**Body:**
```json
{
  "userID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Maria Santos Atualizada",
  "password": "novaSenha789"
}
```

**Respostas:**
- `204 No Content` - Atualização bem-sucedida
- `400 Bad Request` - ID não corresponde
- `404 Not Found` - Usuário não encontrado

#### **DELETE** `/api/v1/User/{id}`
Remove um usuário.

**Path Parameters:**
- `id` (Guid) - ID do usuário

**Respostas:**
- `204 No Content` - Usuário removido
- `404 Not Found` - Usuário não encontrado

---

### ✅ Tasks Endpoints

#### **GET** `/api/v1/Tasks`
Lista todas as tarefas com paginação.

**Query Parameters:**
- `pageNumber` (int, opcional)
- `pageSize` (int, opcional)

#### **GET** `/api/v1/Tasks/{id}`
Busca uma tarefa específica.

#### **POST** `/api/v1/Tasks`
Cria uma nova tarefa.

**Body:**
```json
{
  "userID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Implementar API de relatórios",
  "priority": "HIGH",
  "estimateMinutes": 180,
  "description": "Desenvolver endpoint de relatórios com filtros"
}
```

#### **PUT** `/api/v1/Tasks/{id}`
Atualiza uma tarefa existente.

#### **PUT** `/api/v1/Tasks/cancel/{id}`
Finaliza uma tarefa (marca como COMPLETED e calcula tempo gasto).

**Respostas:**
- `204 No Content` - Tarefa finalizada
- `404 Not Found` - Tarefa não encontrada

#### **DELETE** `/api/v1/Tasks/{id}`
Remove uma tarefa.

---

### 📅 Meeting Endpoints

#### **GET** `/api/v1/Meeting`
Lista todas as reuniões com paginação.

#### **GET** `/api/v1/Meeting/{id}`
Busca uma reunião específica.

#### **POST** `/api/v1/Meeting`
Cria uma nova reunião.

**Body:**
```json
{
  "userID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Daily Standup",
  "description": "Reunião diária da equipe",
  "startDate": "2025-11-23T09:00:00Z"
}
```

#### **PUT** `/api/v1/Meeting/{id}`
Atualiza uma reunião.

#### **PUT** `/api/v1/Meeting/cancel/{id}`
Finaliza uma reunião (calcula duração automaticamente).

#### **DELETE** `/api/v1/Meeting/{id}`
Remove uma reunião.

---

### 🎯 Events Endpoints

#### **GET** `/api/v1/Events`
Lista todos os eventos de foco.

#### **GET** `/api/v1/Events/{id}`
Busca um evento específico.

#### **POST** `/api/v1/Events`
Cria um novo evento de foco.

**Body:**
```json
{
  "userID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "eventType": "START_FOCUS_SESSION"
}
```

#### **PUT** `/api/v1/Events/{id}`
Atualiza um evento.

#### **PUT** `/api/v1/Events/cancel/{id}`
Finaliza um evento de foco (marca como END_FOCUS_SESSION e calcula duração).

#### **DELETE** `/api/v1/Events/{id}`
Remove um evento.

---

### 📈 Report Endpoints

#### **GET** `/api/v1/Report`
Gera relatório de produtividade para um usuário em um período.

**Query Parameters:**
- `userId` (Guid, obrigatório) - ID do usuário
- `StartDate` (DateOnly, obrigatório) - Data de início (formato: YYYY-MM-DD)
- `EndDate` (DateOnly, obrigatório) - Data de fim (formato: YYYY-MM-DD)
- `pageNumber` (int, opcional)
- `pageSize` (int, opcional)

**Exemplo:**
```
GET /api/v1/Report?userId=3fa85f64-5717-4562-b3fc-2c963f66afa6&StartDate=2025-11-01&EndDate=2025-11-30
```

**Resposta de sucesso (200):**
```json
{
  "item": {
    "userID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "startDate": "2025-11-01",
    "endDate": "2025-11-30",
    "completedTasks": 15,
    "inProgressTasks": 3,
    "finishedMeetings": 22,
    "focusMinutes": 1200,
    "completionPercentage": 83.33,
    "burnoutRisk": null,
    "productivityTrend": null,
    "focusTrend": null,
    "insights": null,
    "aiRecommendation": null,
    "summary": null
  },
  "currentPage": 1,
  "pageSize": 10,
  "totalItems": 18,
  "links": []
}
```

## ⚙️ Configuração e Instalação

### Pré-requisitos

- .NET 9 SDK
- Oracle Database (ou acesso a um servidor Oracle)
- Visual Studio 2022 / Visual Studio Code / Rider

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/Work360.git
cd Work360
```

### 2. Configure a connection string

Edite o arquivo `appsettings.json` com suas credenciais do Oracle:

```json
{
  "ConnectionStrings": {
    "Oracle": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=seu_servidor:1521/orcl;"
  }
}
```

### 3. Instale as dependências

```bash
dotnet restore
```

### 4. Execute as migrations

```bash
dotnet ef database update
```

### 5. Execute a aplicação

```bash
dotnet run
```

A API estará disponível em:
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001
- **Swagger UI:** http://localhost:5000 (redirecionamento automático)

## 🔄 Migrations

O projeto utiliza Entity Framework Core Migrations para gerenciar o schema do banco de dados.

### Migration Inicial

A migration `InitialCreate` (criada em 22/11/2025) cria as seguintes tabelas:

- `Work360_User`
- `Work360_Tasks`
- `Work360_Meetings`
- `Work360_Events`

### Comandos úteis

**Criar uma nova migration:**
```bash
dotnet ef migrations add NomeDaMigration
```

**Aplicar migrations:**
```bash
dotnet ef database update
```

**Reverter para uma migration específica:**
```bash
dotnet ef database update NomeDaMigration
```

**Remover a última migration (não aplicada):**
```bash
dotnet ef migrations remove
```

**Gerar script SQL:**
```bash
dotnet ef migrations script
```

### Configurações especiais no DbContext

O `Work360Context` possui configurações específicas:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Mapeamento de tabelas
    modelBuilder.Entity<User>().ToTable("Work360_User");
    modelBuilder.Entity<Events>().ToTable("Work360_Events");
    modelBuilder.Entity<Tasks>().ToTable("Work360_Tasks");
    modelBuilder.Entity<Meeting>().ToTable("Work360_Meetings");

    // Conversão de Enums para String no banco
    modelBuilder.Entity<Events>()
        .Property(e => e.EventType)
        .HasConversion<string>()
        .HasMaxLength(50);

    modelBuilder.Entity<Tasks>()
        .Property(t => t.Priority)
        .HasConversion<string>()
        .HasMaxLength(50);

    modelBuilder.Entity<Tasks>()
        .Property(t => t.TaskSituation)
        .HasConversion<string>()
        .HasMaxLength(50);
}
```

## 🧪 Testes

O projeto inclui testes unitários completos utilizando xUnit, Moq e banco de dados InMemory.

### Estrutura de Testes

```
Work360.Tests/
├── UserControllerTest.cs         # Testes do UserController
├── TasksControllerTests.cs       # Testes do TasksController
├── ReportControllerTests.cs      # Testes do ReportController
└── HateoasServiceTests.cs        # Testes do serviço HATEOAS
```

### Executar os testes

**Via CLI:**
```bash
dotnet test
```

**Com detalhamento:**
```bash
dotnet test --verbosity detailed
```

**Com cobertura de código:**
```bash
dotnet test /p:CollectCoverage=true
```

### Exemplo de Teste (UserController)

```csharp
[Fact]
public async Task GetUser_ReturnsOkResult_WhenUserExists()
{
    // Arrange
    var user = new User { 
        UserID = Guid.NewGuid(), 
        Name = "Test User", 
        Password = "password123" 
    };
    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    // Act
    var result = await _controller.GetUser(user.UserID);

    // Assert
    var okResult = Assert.IsType<ActionResult<User>>(result);
    var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
    var returnedUser = Assert.IsType<User>(okObjectResult.Value);

    Assert.Equal(user.UserID, returnedUser.UserID);
    Assert.Equal("Test User", returnedUser.Name);
}
```

### Cenários de Teste Cobertos

- ✅ Listagem de recursos com paginação
- ✅ Busca de recurso por ID
- ✅ Criação de recursos
- ✅ Atualização de recursos
- ✅ Remoção de recursos
- ✅ Validações de entrada
- ✅ Tratamento de recursos não encontrados
- ✅ Geração de links HATEOAS

## 🏥 Health Check

A API possui endpoint de health check para monitoramento da saúde da aplicação.

### Endpoint

```
GET /health
```

### Resposta de sucesso:

```json
{
  "status": "Healthy",
  "timestamp": "2025-11-23T10:30:00Z",
  "duration": "00:00:00.0234567",
  "checks": [
    {
      "name": "Oracle",
      "status": "Healthy",
      "description": null,
      "duration": "00:00:00.0123456",
      "exception": null,
      "data": {}
    }
  ]
}
```

### Health Check customizado para Oracle

O projeto implementa um `OracleHealthCheck` que verifica a conectividade com o banco de dados:

```csharp
public class OracleHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public OracleHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy("Oracle database is reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Oracle database is unreachable", 
                ex
            );
        }
    }
}
```

## 📊 Observabilidade

### OpenTelemetry Tracing

O projeto implementa distributed tracing com OpenTelemetry para rastreamento de requisições.

**Configuração em Program.cs:**
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(trace =>
    {
        trace
            .AddSource("Work360.API")
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("Work360.API")
            )
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });
```

**Uso nos Controllers:**
```csharp
public static readonly ActivitySource ActivitySource = new ActivitySource("Work360");

var activity = ActivitySource.StartActivity("UserController.GetUsers");
activity?.SetTag("users.pageNumber", pageNumber);
activity?.SetTag("users.pageSize", pageSize);
```

### Logging

O sistema utiliza logging integrado do ASP.NET Core:

```csharp
_logger.LogInformation("Listando usuários: Página {Page}, Tamanho {Size}", 
    pageNumber, pageSize);
```

Logs são enviados para:
- Console
- Debug output
- Providers configurados

## 🔗 HATEOAS

A API implementa o padrão HATEOAS (Hypermedia as the Engine of Application State) através do `HateoasService`.

### Estrutura de Links

Cada resposta paginada inclui links de navegação:

```json
{
  "items": [...],
  "links": [
    {
      "href": "/api/v1/User?pageNumber=1&pageSize=10",
      "rel": "self",
      "method": "GET"
    },
    {
      "href": "/api/v1/User?pageNumber=2&pageSize=10",
      "rel": "next",
      "method": "GET"
    },
    {
      "href": "/api/v1/User?pageNumber=1&pageSize=10",
      "rel": "first",
      "method": "GET"
    },
    {
      "href": "/api/v1/User?pageNumber=5&pageSize=10",
      "rel": "last",
      "method": "GET"
    }
  ]
}
```

### Benefícios

- ✅ Navegação facilitada entre páginas
- ✅ Descoberta de recursos relacionados
- ✅ API auto-descritiva
- ✅ Redução de acoplamento cliente-servidor

## 📝 Swagger / OpenAPI

A API possui documentação interativa via Swagger UI.

### Acessar Swagger

Após iniciar a aplicação, acesse:
```
http://localhost:5000
```

O Swagger UI permite:
- 📖 Visualizar todos os endpoints
- 🧪 Testar requisições diretamente
- 📋 Ver modelos de dados
- 🔍 Consultar códigos de resposta

## 🔒 Validações e Atributos Customizados

### NotEmptyGuidAttribute

Atributo personalizado para validar que um Guid não está vazio:

```csharp
[NotEmptyGuid(ErrorMessage = "UserID cannot be empty GUID.")]
public Guid UserID { get; set; }
```

### Validações nas Entidades

Todas as entidades possuem validações com Data Annotations:
- `[Required]` - Campo obrigatório
- `[Key]` - Chave primária
- `[Display]` - Nome de exibição
- `[Table]` - Nome da tabela no banco

## 📦 Pacotes NuGet Principais

| Pacote | Versão | Descrição |
|--------|--------|-----------|
| Microsoft.EntityFrameworkCore | 9.0.0 | ORM principal |
| Oracle.EntityFrameworkCore | 9.23.26000 | Provider Oracle |
| Microsoft.AspNetCore.Mvc.Versioning | 5.1.0 | Versionamento de API |
| Swashbuckle.AspNetCore | 6.6.0 | Swagger/OpenAPI |
| OpenTelemetry | 1.14.0 | Distributed tracing |
| xUnit | 2.9.3 | Framework de testes |
| Moq | 4.20.72 | Mocking para testes |

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para:

1. Fazer um fork do projeto
2. Criar uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commitar suas mudanças (`git commit -m 'Adiciona nova feature'`)
4. Fazer push para a branch (`git push origin feature/MinhaFeature`)
5. Abrir um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT.

## 👥 Autores

- **Gustavo** - Desenvolvimento inicial

## 📞 Contato

Para dúvidas ou sugestões, entre em contato através do GitHub.

---

**Work360** - Gerenciamento de produtividade profissional 🚀
