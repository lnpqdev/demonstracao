# Roteiro de Demonstração — BFF com ASP.NET Core 8, MediatR e Clean Architecture

> **Objetivo da demo:** montar, passo a passo com o time, um **BFF (Backend for Frontend)** que consome
> uma API pública (**BrasilAPI**), organizado em **Clean Architecture**, desacoplado com **MediatR**,
> validado com **FluentValidation** e coberto por **testes de unidade** (xUnit + Moq).
>
> **Duração estimada:** 25–30 min.

---

## 0. O que é um BFF (contextualização — 1 min)

Um **Backend for Frontend** é uma camada de backend feita sob medida para **um** cliente (web, mobile…).
Ele **orquestra e agrega** chamadas a outras APIs/serviços e devolve ao front um contrato **já no formato
que a tela precisa**. Benefícios: menos lógica no front, menos "chatter" de rede, e um ponto único para
segurança/cache/resiliência.

Nesta demo o "serviço de origem" é a **BrasilAPI** (consulta de CEP) e o nosso BFF traduz a resposta dela
para um contrato próprio.

---

## 1. Clean Architecture — a ideia central (leia ANTES de codar)

A regra de ouro é **a Regra da Dependência**: o código-fonte só pode depender para **dentro**. As camadas
internas não sabem nada sobre as externas.

![Diagrama da Clean Architecture do BFF: camadas concêntricas (Domain no centro, Application, Infrastructure e API na borda) com as dependências apontando para dentro](docs/clean-architecture.svg)

```
        ┌─────────────────────────────────────────────┐
        │                    API                       │  ← frameworks (ASP.NET, Swagger)
        │  Controllers, Program.cs, ExceptionHandler   │     composition root
        │   ┌───────────────────────────────────────┐  │
        │   │            INFRASTRUCTURE              │  │  ← detalhes (HTTP, banco, filas)
        │   │  BrasilApiClient (implementa a port)  │  │
        │   │   ┌───────────────────────────────┐   │  │
        │   │   │         APPLICATION           │   │  │  ← casos de uso / regras da aplicação
        │   │   │  UseCases, Ports, DTOs,       │   │  │
        │   │   │  Validators, Behaviors        │   │  │
        │   │   │   ┌───────────────────────┐   │   │  │
        │   │   │   │       DOMAIN          │   │   │  │  ← regras de negócio puras
        │   │   │   │  Entidade Endereco    │   │   │  │
        │   │   │   └───────────────────────┘   │   │  │
        │   │   └───────────────────────────────┘   │  │
        │   └───────────────────────────────────────┘  │
        └─────────────────────────────────────────────┘

   Dependências (setas do código) apontam SEMPRE para dentro:
        API ──► Infrastructure ──► Application ──► Domain
                 (Infra e Api implementam/consomem interfaces da Application)
```

### O que cada camada CONHECE e o que NÃO deve conhecer

| Camada | Projeto | **Conhece** | **NÃO deve conhecer** |
|---|---|---|---|
| **Domain** | `BffDemo.Domain` | Só C# puro (a própria linguagem) | ASP.NET, MediatR, HTTP, banco, DTOs externos, **nada** |
| **Application** | `BffDemo.Application` | Domain; MediatR/FluentValidation (orquestração) | HTTP, BrasilAPI, ASP.NET, banco concreto — só a **interface** (port) |
| **Infrastructure** | `BffDemo.Infrastructure` | Application (implementa as ports), Domain, HttpClient | Controllers/ASP.NET, quem a consome |
| **API** | `BffDemo.Api` | Todas (só para compor a DI) e ASP.NET | regras de negócio (elas vivem no Domain/Application) |

**Frase-chave para o time:** *"O Domain é o centro e não conhece ninguém. A BrasilAPI é um detalhe
substituível que mora na borda. Trocar BrasilAPI por ViaCEP não toca em Domain nem Application."*

### Por que isso vale a pena
- **Testabilidade:** Domain e Application testam sem subir servidor nem internet (só mocks).
- **Substituição:** troca de provedor de CEP, de banco ou de framework web afeta só a borda.
- **Organização:** cada mudança tem um lugar óbvio para acontecer.

---

## 2. Comandos `dotnet` para criar a solução (rode ao vivo)

```bash
# 1. Solução + projeto Web API (a camada mais externa)
dotnet new sln -n BffDemo
dotnet new webapi -n BffDemo.Api --framework net8.0 --use-controllers

# 2. As 3 camadas internas como class libraries
dotnet new classlib -n BffDemo.Domain         --framework net8.0
dotnet new classlib -n BffDemo.Application     --framework net8.0
dotnet new classlib -n BffDemo.Infrastructure  --framework net8.0

# 3. Adicionar tudo à solução
dotnet sln add BffDemo.Api BffDemo.Domain BffDemo.Application BffDemo.Infrastructure

# 4. Referências APONTANDO PARA DENTRO (o coração da Clean Architecture)
dotnet add BffDemo.Application    reference BffDemo.Domain          # App conhece Domain
dotnet add BffDemo.Infrastructure reference BffDemo.Application     # Infra conhece App
dotnet add BffDemo.Api            reference BffDemo.Application BffDemo.Infrastructure

# 5. Pacotes, CADA UM na sua camada
dotnet add BffDemo.Application    package MediatR
dotnet add BffDemo.Application    package FluentValidation
dotnet add BffDemo.Application    package FluentValidation.DependencyInjectionExtensions
dotnet add BffDemo.Application    package Microsoft.Extensions.DependencyInjection.Abstractions
dotnet add BffDemo.Application    package Microsoft.Extensions.Logging.Abstractions
dotnet add BffDemo.Infrastructure package Microsoft.Extensions.Http
dotnet add BffDemo.Infrastructure package Microsoft.Extensions.Configuration.Abstractions

dotnet build
```

> **Destaque:** a Api NÃO recebe `MediatR`/`FluentValidation` como pacote direto — eles chegam por
> **referência transitiva** vindo da Application. Isso reforça que a orquestração é da Application, não da Api.

Em seguida removemos o boilerplate `WeatherForecast` e os `Class1.cs` das libraries.

---

## 3. Estrutura final do projeto

```
BffDemo/
├── BffDemo.Domain/                         # 🟢 CENTRO — regras de negócio puras
│   └── Enderecos/Endereco.cs               # entidade + regra Formatar()
│
├── BffDemo.Application/                     # 🔵 CASOS DE USO
│   ├── Abstractions/IEnderecoProvider.cs   # PORT (interface) — o que a app precisa
│   ├── Ceps/ObterCep/
│   │   ├── ObterCepQuery.cs                 # input do caso de uso (IRequest)
│   │   ├── ObterCepResponse.cs             # DTO de saída (contrato do BFF)
│   │   ├── ObterCepHandler.cs              # orquestração do caso de uso
│   │   └── ObterCepQueryValidator.cs       # validação de entrada
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs           # pipeline: valida antes do handler
│   │   └── LoggingBehavior.cs              # pipeline: loga tempo
│   └── DependencyInjection.cs              # AddApplication()
│
├── BffDemo.Infrastructure/                  # 🟠 DETALHES (borda)
│   ├── BrasilApi/
│   │   ├── EnderecoExternoResponse.cs      # DTO do contrato EXTERNO (internal)
│   │   └── BrasilApiClient.cs              # ADAPTER: implementa IEnderecoProvider
│   └── DependencyInjection.cs             # AddInfrastructure(configuration)
│
├── BffDemo.Api/                             # 🔴 APRESENTAÇÃO / composition root
│   ├── Controllers/CepsController.cs       # thin controller
│   ├── Common/Errors/ValidationExceptionHandler.cs  # ValidationException -> 400
│   ├── Program.cs                          # compõe as camadas na DI
│   └── appsettings.json                    # BrasilApi:BaseUrl
│
└── BffDemo.Api.Tests/                       # ✅ TESTES (xUnit + Moq)
    ├── Domain/EnderecoTests.cs
    └── Features/Ceps/ObterCep{Handler,QueryValidator}Tests.cs
```

---

## 4. Ordem de criação dos arquivos (o passo a passo da apresentação)

Esta é a **ordem recomendada para criar arquivo por arquivo ao vivo**. A estratégia é **de dentro para
fora** (*inside-out*): começamos pelo centro (Domain) e avançamos até a borda (API).

**Por que essa ordem?** Dois motivos que reforçam a Clean Architecture na prática:
1. **Cada arquivo só referencia o que já existe** → o projeto vai **compilando a cada passo**, sem erros
   de "tipo não encontrado". O time vê a dependência apontando sempre para dentro.
2. **Segue o raciocínio de negócio → técnico:** primeiro "o que é o negócio?", depois "o que o caso de uso
   precisa?", e só no fim "como isso vira HTTP?". A regra nasce antes do detalhe.

> **Alternativa (*outside-in*):** começar pelo Controller e ir "descobrindo" as dependências. É boa para
> quem já domina o padrão, mas para **ensinar** a Clean Architecture o *inside-out* é mais claro — evita
> ficar com código que não compila e mantém a narrativa das camadas.

### Fase 0 — Esqueleto (antes de qualquer `.cs`)
Os projetos e as referências da **Seção 2**. Rode os `dotnet new` / `reference` / `package` e um `build`
para partir de uma solução verde e vazia.

### Sequência de arquivos

| # | Arquivo | Camada | Pergunta que ele responde | Depende de |
|---|---|---|---|---|
| 1 | `Domain/Enderecos/Endereco.cs` | 🟢 Domain | "Qual é o conceito de negócio e sua regra?" | — (nada) |
| 2 | `Application/Abstractions/IEnderecoProvider.cs` | 🔵 App | "O que o caso de uso precisa do mundo externo?" (a **port**) | Domain |
| 3 | `Application/Ceps/ObterCep/ObterCepResponse.cs` | 🔵 App | "O que o frontend vai receber?" (a meta) | — |
| 4 | `Application/Ceps/ObterCep/ObterCepQuery.cs` | 🔵 App | "Qual é a entrada do caso de uso?" | MediatR |
| 5 | `Application/Ceps/ObterCep/ObterCepHandler.cs` | 🔵 App | "Como orquestro Query → port → Response?" | itens 2,3,4 |
| 6 | `Application/Ceps/ObterCep/ObterCepQueryValidator.cs` | 🔵 App | "Que entrada é válida?" | item 4 |
| 7 | `Application/Behaviors/ValidationBehavior.cs` | 🔵 App | "Como validar toda request no pipeline?" | FluentValidation |
| 8 | `Application/Behaviors/LoggingBehavior.cs` | 🔵 App | "Como logar/medir toda request?" | MediatR |
| 9 | `Application/DependencyInjection.cs` | 🔵 App | "Como registro MediatR + validators?" (`AddApplication`) | itens 5–8 |
| 10 | `Infrastructure/BrasilApi/EnderecoExternoResponse.cs` | 🟠 Infra | "Qual o formato do JSON de terceiros?" | — |
| 11 | `Infrastructure/BrasilApi/BrasilApiClient.cs` | 🟠 Infra | "Como implemento a port via HTTP?" (o **adapter**) | itens 2,10 + Domain |
| 12 | `Infrastructure/DependencyInjection.cs` | 🟠 Infra | "Como ligo a port ao adapter?" (`AddInfrastructure`) | item 11 |
| 13 | `Api/appsettings.json` | 🔴 API | "Onde fica a URL da BrasilAPI?" | — |
| 14 | `Api/Program.cs` | 🔴 API | "Como componho todas as camadas?" | `AddApplication`, `AddInfrastructure` |
| 15 | `Api/Controllers/CepsController.cs` | 🔴 API | "Como exponho isso como endpoint HTTP?" | itens 3,4 (Query/Response) |
| 16 | `Api/Common/Errors/ValidationExceptionHandler.cs` | 🔴 API | "Como transformo erro de validação em 400?" | FluentValidation |
| 17 | `Tests/Domain/EnderecoTests.cs` | ✅ Test | "A regra de domínio está correta?" | item 1 |
| 18 | `Tests/.../ObterCepQueryValidatorTests.cs` | ✅ Test | "As regras de entrada funcionam?" | item 6 |
| 19 | `Tests/.../ObterCepHandlerTests.cs` | ✅ Test | "O caso de uso mapeia certo? (mock da port)" | itens 2,5 |

### Roteiro narrado por fase (o que dizer ao criar cada bloco)

1. **🟢 Domain (item 1):** *"Tudo começa pelo negócio. Crio a entidade `Endereco` e sua regra `Formatar()`
   sem citar HTTP, MediatR ou banco. Isso compila sozinho — é o centro."*
2. **🔵 Application — contrato primeiro (itens 2–4):** *"Antes do 'como', defino o 'o quê': a **port**
   `IEnderecoProvider` (o que preciso de fora) e o `ObterCepResponse` (o que entrego ao front)."*
3. **🔵 Application — orquestração (item 5):** *"Agora o `Handler` amarra tudo — e repare: ele depende da
   **interface**, não da BrasilAPI. Ele nem sabe que existe HTTP."*
4. **🔵 Application — pipeline e fiação (itens 6–9):** *"Validação e logging como Behaviors, e o
   `AddApplication()` que registra a camada."*
5. **🟠 Infrastructure (itens 10–12):** *"Só agora aparece a BrasilAPI. Implemento a port com um
   `BrasilApiClient` e mapeio o JSON externo para a entidade de domínio. A dependência aponta para dentro:
   o detalhe implementa a abstração."*
6. **🔴 API (itens 13–16):** *"A borda: configuro a URL, componho as camadas no `Program.cs`, exponho o
   endpoint no Controller e trato o erro de validação."*
7. **✅ Testes (itens 17–19):** *"Fecho provando cada camada isoladamente — do domínio puro ao caso de uso
   com a port mockada."*

> **Dica de apresentação:** rode `dotnet build` ao terminar cada fase (não cada arquivo). São 5 checkpoints
> verdes que mostram a solução crescendo de dentro para fora sem quebrar.

---

## 5. O fluxo de uma requisição atravessando as camadas

```
HTTP GET /api/ceps/01310100
        │  (API)
        ▼
 CepsController ──► _mediator.Send(new ObterCepQuery("01310100"))
        │  (Application — pipeline MediatR)
        ▼
 ValidationBehavior ──► ObterCepQueryValidator  (inválido? -> 400)
        ▼
 LoggingBehavior  (mede tempo)
        ▼
 ObterCepHandler ──► IEnderecoProvider.ObterPorCepAsync(cep)   ← PORT (Application)
        │                          │
        │             (Infrastructure — ADAPTER)
        │                          ▼
        │            BrasilApiClient ──► GET brasilapi.com.br/api/cep/v2/01310100
        │                          │
        │            EnderecoExternoResponse ──► mapeia ──► Endereco (Domain)
        ▼                          ▲
 recebe Endereco (Domain) ────────┘
        │  mapeia p/ DTO usando endereco.Formatar()  (regra de Domain)
        ▼
 ObterCepResponse ──► 200 OK (JSON)
```

**Mensagem-chave:** a seta que "sai" da Application (`IEnderecoProvider`) é uma **interface**; quem a
implementa é a Infrastructure. Isso inverte a dependência: o detalhe (HTTP/BrasilAPI) depende da
abstração (port), e não o contrário. É a **Inversão de Dependência** na prática.

---

## 6. Aspectos principais de cada arquivo

### 🟢 Domain — `Endereco.cs`
Entidade de negócio, **C# puro**, sem nenhum `using` de framework. A regra "como formatar um endereço"
(`Formatar()`) mora aqui — comportamento de domínio, reutilizável e testável isoladamente. É a camada que
**não muda** quando trocamos banco, API externa ou framework web.

### 🔵 Application
- **`IEnderecoProvider` (PORT):** a Application *declara* o que precisa ("dado um CEP, devolva um
  `Endereco`") mas **não implementa**. Retorna entidade de **domínio**, não DTO externo — então a
  Application nunca fica sabendo que existe HTTP/BrasilAPI.
- **`ObterCepQuery` / `ObterCepResponse`:** input e DTO de saída do caso de uso. `ObterCepResponse` é o
  **contrato do BFF** (nomes em PT + campo derivado `EnderecoFormatado`) — o "for Frontend".
- **`ObterCepHandler`:** orquestra — normaliza o CEP, chama a **port**, e mapeia `Endereco` (domínio) →
  `ObterCepResponse` (DTO), usando `endereco.Formatar()`.
- **`ObterCepQueryValidator`:** regras de entrada (FluentValidation).
- **`Behaviors/*`:** middleware do MediatR (validação + logging) que vale para todos os casos de uso.
- **`DependencyInjection.AddApplication()`:** registra MediatR (handlers + behaviors) e os validators.

### 🟠 Infrastructure
- **`EnderecoExternoResponse` (`internal`):** espelha o JSON da BrasilAPI (`street`, `city`…). É `internal`
  porque é detalhe da borda — ninguém fora daqui precisa vê-lo.
- **`BrasilApiClient` (ADAPTER):** implementa `IEnderecoProvider` com Typed HttpClient; trata 404 → `null`
  e mapeia **DTO externo → entidade de domínio**.
- **`DependencyInjection.AddInfrastructure(config)`:** liga a port `IEnderecoProvider` ao adapter
  `BrasilApiClient` e lê a URL do `appsettings`. É o **único** lugar que decide "usamos a BrasilAPI".

### 🔴 API (composition root)
- **`Program.cs`:** enxuto — só chama `AddApplication()` e `AddInfrastructure(...)`. Cada camada sabe se
  registrar; a Api apenas compõe.
- **`CepsController`:** *thin controller* — injeta `IMediator`, envia a Query, traduz para status HTTP.
- **`ValidationExceptionHandler`:** converte `ValidationException` em HTTP 400 (ProblemDetails). É
  preocupação de **apresentação**, por isso fica na Api.
- **`appsettings.json`:** `BrasilApi:BaseUrl` — trocar ambiente/URL sem recompilar.

---

## 7. MediatR — o desacoplamento

`AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly))` faz **assembly scanning**: acha todos os
`IRequestHandler` e `IPipelineBehavior` sozinho. Adicionar um caso de uso = criar as classes; **zero**
edição de fiação. O Controller não sabe *como* o CEP é obtido; o Handler não sabe que existe HTTP.
Os **Pipeline Behaviors** (validação/log/cache/retry) ficam num só lugar, sem poluir cada Handler.

---

## 8. FluentValidation — validação como Behavior

```
Send(query) → ValidationBehavior → (válido?) → LoggingBehavior → Handler
                     └ inválido → ValidationException → ExceptionHandler → HTTP 400
```

- **`ObterCepQueryValidator : AbstractValidator<ObterCepQuery>`** — `RuleFor(x => x.Cep).NotEmpty().Must(...)`.
- **`ValidationBehavior`** — recebe `IEnumerable<IValidator<TRequest>>` por DI, roda todos e lança
  `ValidationException` se houver falha (o Handler nem executa).
- **`ValidationExceptionHandler : IExceptionHandler`** — traduz para 400 com `ValidationProblemDetails` (RFC 7807).
- **Registro:** `AddOpenBehavior(typeof(ValidationBehavior<,>))` **antes** do Logging, mais
  `AddValidatorsFromAssembly(...)` (na Application) e `AddExceptionHandler<...>()` + `app.UseExceptionHandler()` (na Api).

Teste: `curl -i http://localhost:5250/api/ceps/123` → **400**.

---

## 9. Testes de unidade — xUnit + Moq

Criar: `dotnet new xunit -n BffDemo.Api.Tests` + `dotnet add reference` para **Application** e **Domain**
+ `dotnet add package Moq`.

- **xUnit:** `[Fact]` (1 caso), `[Theory]`+`[InlineData]` (vários), `Assert.*`.
- **Moq:** substitui a **port** `IEnderecoProvider` por um dublê: `Setup(...).ReturnsAsync(endereco)`
  controla o retorno; `Verify(...)` prova que foi chamada certo (ex.: CEP normalizado).

```csharp
// Arrange — mock da PORT devolve uma entidade de DOMÍNIO (sem internet)
mock.Setup(p => p.ObterPorCepAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Endereco("01310100","Avenida Paulista","Bela Vista","São Paulo","SP"));
var handler = new ObterCepHandler(mock.Object);
// Act + Assert
var r = await handler.Handle(new ObterCepQuery("01310100"), CancellationToken.None);
Assert.Equal("Avenida Paulista, Bela Vista - São Paulo/SP", r!.EnderecoFormatado);
```

Cobrimos: entidade de Domínio (`EnderecoTests`), Handler (mapeamento, normalização, 404→null) e Validator.
Rodar: `dotnet test` → **Aprovado! Com falha: 0, Aprovado: 10**.

> **Por que é testável?** Porque cada camada depende de **abstrações** (interface `IEnderecoProvider`) e a
> regra de negócio vive no Domain, sem I/O. Clean Architecture existe justamente para isso.

---

## 10. SOLID no projeto — onde cada princípio aparece

| Princípio | O que diz | Onde no código |
|---|---|---|
| **S** — Single Responsibility | uma classe, um motivo para mudar | `Controller` só faz HTTP; `Handler` só orquestra; `Validator` só valida; `BrasilApiClient` só fala com a API; `Endereco` só a regra de domínio |
| **O** — Open/Closed | aberto p/ extensão, fechado p/ modificação | adicionar um caso de uso ou um `Behavior` **não altera** o `Program.cs` (assembly scanning). Novo comportamento transversal = nova classe de Behavior |
| **L** — Liskov Substitution | implementações são intercambiáveis pela abstração | qualquer `IEnderecoProvider` (BrasilAPI, ViaCEP, fake de teste) entra no lugar sem quebrar o Handler |
| **I** — Interface Segregation | interfaces pequenas e focadas | `IEnderecoProvider` expõe só `ObterPorCepAsync` — o cliente não depende de métodos que não usa |
| **D** — Dependency Inversion | dependa de abstrações, não de concretos | o Handler (política) depende da **interface** `IEnderecoProvider`; o detalhe `BrasilApiClient` é injetado pela DI. As setas de dependência apontam para a abstração |

**Demonstração ao vivo do "D" + "L" + "O" juntos:** *"Se amanhã a BrasilAPI cair, eu crio um
`ViaCepClient : IEnderecoProvider`, troco uma linha no `AddInfrastructure`, e nem o Domain, nem o
Application, nem os testes de Handler precisam mudar."*

Além do SOLID, o projeto aplica: **Inversão de Controle** (DI do ASP.NET), **Ports & Adapters**
(a port `IEnderecoProvider` + adapter `BrasilApiClient`) e **CQRS-light** (Query/Handler do MediatR).

---

## 11. Rodar e testar ao vivo

```bash
dotnet build
dotnet test                       # 10 testes verdes
dotnet run --project BffDemo.Api  # sobe a API (porta aparece no console, ex.: 5250)
```

```bash
# 200 — CEP válido (Av. Paulista, SP)
curl http://localhost:5250/api/ceps/01310100
# 200 — com hífen (o Handler normaliza)
curl http://localhost:5250/api/ceps/30110-005
# 400 — inválido (Validator barra)
curl -i http://localhost:5250/api/ceps/123
# 404 — inexistente
curl -i http://localhost:5250/api/ceps/00000000
```

Resposta esperada (200):
```json
{
  "cep": "01310100",
  "logradouro": "Avenida Paulista",
  "bairro": "Bela Vista",
  "cidade": "São Paulo",
  "uf": "SP",
  "enderecoFormatado": "Avenida Paulista, Bela Vista - São Paulo/SP"
}
```

Mostre no console os logs `[MediatR] Iniciando... Finalizado ... em Xms` (prova visual do pipeline) e abra
o **Swagger** em `/swagger`.

---

## 12. Fechamento (1 min)

| Preocupação | Onde vive | Benefício |
|---|---|---|
| Regra de negócio pura | Domain | testável, estável, sem dependências |
| Caso de uso / orquestração | Application (Handler) | isolado de HTTP e framework |
| Contrato do front | `ObterCepResponse` | front recebe só o que precisa |
| Integração externa | Infrastructure (`BrasilApiClient`) | substituível sem impacto |
| Entrada HTTP | API (Controller) | fina; trocável (REST/gRPC) |
| Validação/log/cache | Pipeline Behaviors | um lugar, todas as features |
| Fiação | `Program.cs` + `AddXxx` | composição explícita e enxuta |

**Próximos passos sugeridos ao time:** `Polly` para retry/circuit-breaker no HttpClient (Infrastructure),
cache via outro Behavior (Application), e um segundo adapter (`ViaCepClient`) para provar o desacoplamento.
