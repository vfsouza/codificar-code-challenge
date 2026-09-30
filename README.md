# Sistema de Controle de Chamados Internos

Aplicação web para abrir, acompanhar e resolver chamados internos (pedidos como "meu computador travou" ou "a impressora não funciona"). Cada chamado tem um responsável definido, e a atribuição pode ser automática para equilibrar a carga entre a equipe de suporte.

---

## Sumário

- [Como executar](#como-executar)
- [Telas](#telas)
- [Banco de dados](#banco-de-dados)
- [Stack e justificativas](#stack-e-justificativas)
- [Arquitetura](#arquitetura)
- [Regras de negócio](#regras-de-negócio)
- [Testes](#testes-automatizados)
- [Decisões técnicas](#decisões-técnicas)
- [Trade-offs](#trade-offs)
- [Bibliotecas utilizadas](#bibliotecas-utilizadas)
- [Uso de IA](#uso-de-ia)
- [Solução de problemas](#solução-de-problemas)

---

## Como executar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

Não precisa de Docker nem de servidor de banco: o banco é um arquivo SQLite criado automaticamente.

### Passos

Na raiz do repositório:

```bash
# 1. Restaura as ferramentas locais (dotnet-ef)
dotnet tool restore

# 2. Executa a aplicação
dotnet run --project src/Codificar.Web
```

A aplicação sobe em **http://localhost:5127**.

Na primeira execução o banco é criado e populado sozinho: não é preciso rodar nenhum comando de migration.

### Testes

```bash
dotnet test
```

Não é preciso configurar nada: cada teste cria o próprio banco SQLite em memória.

---

## Telas

| Rota | Tela |
|---|---|
| `/` ou `/chamados` | Listagem: painel de carga por responsável, filtros e tabela. Clicar em uma linha abre o detalhe. |
| `/chamados/novo` | Abertura de chamado, com responsável manual ou **"Atribuir automaticamente"** (padrão). |
| `/chamados/{id}` | Detalhe, com botões de ação conforme o status atual ("Iniciar atendimento", "Marcar como resolvido", "Fechar", "Reabrir"). |
| `/chamados/{id}/editar` | Edição dos dados, do responsável e do status. |

Rotas inexistentes exibem uma página de "não encontrado", e as operações mostram o resultado (sucesso ou erro de regra de negócio) em notificações no canto da tela.

---

## Banco de dados

### Criação e carga inicial

A cada inicialização, a aplicação:

1. aplica as migrations pendentes (`Database.MigrateAsync()`), o que cria o arquivo do banco se ele não existir;
2. se não houver nenhum chamado, cria **9 chamados de exemplo**.

Os **4 responsáveis** (Ana Souza, Bruno Lima, Carla Mendes e Diego Rocha) vêm da própria migration (`HasData`).

Os chamados de exemplo foram montados para demonstrar a distribuição automática. A carga pendente resultante é:

| Responsável  | Chamados pendentes | Observação                                  |
|--------------|:------------------:|---------------------------------------------|
| Ana Souza    | 3                  |                                             |
| Carla Mendes | 2                  |                                             |
| Bruno Lima   | 1                  | tem também um chamado resolvido             |
| Diego Rocha  | 0                  | tem chamados, mas todos resolvidos/fechados |

Ou seja: o próximo chamado com atribuição automática vai para o **Diego**, mesmo ele já tendo chamados, porque nenhum deles está em aberto (ver [Definição de "em aberto"](#definição-de-em-aberto)).

### Onde fica o arquivo

A connection string (`appsettings.json`) é `Data Source=chamados.db`, relativa à pasta do projeto Web:

```
src/Codificar.Web/chamados.db
```

O arquivo está no `.gitignore`.

### Resetar o banco

Pare a aplicação, apague o arquivo e execute de novo:

```bash
rm src/Codificar.Web/chamados.db*
dotnet run --project src/Codificar.Web
```

O `*` também remove os arquivos auxiliares do SQLite (`chamados.db-wal` e `chamados.db-shm`).

### Migrations (para desenvolvimento)

O `dotnet-ef` é uma **ferramenta local**, com a versão fixada em `dotnet-tools.json` e alinhada à versão do EF Core (10.x). Não precisa de instalação global: basta `dotnet tool restore`.

As migrations ficam no projeto Infrastructure; o projeto Web é usado como startup:

```bash
# Criar uma nova migration
dotnet ef migrations add NomeDaMigration -p src/Codificar.Infrastructure -s src/Codificar.Web -o Persistence/Migrations

# Remover a última migration (antes de ela ser aplicada em outro ambiente)
dotnet ef migrations remove -p src/Codificar.Infrastructure -s src/Codificar.Web

# Aplicar manualmente (opcional: a aplicação já aplica ao subir)
dotnet ef database update -p src/Codificar.Infrastructure -s src/Codificar.Web
```

---

## Stack e justificativas

| Camada           | Tecnologia                                    |
|------------------|-----------------------------------------------|
| Plataforma       | .NET 10                                       |
| Web / UI         | Blazor Web App, render mode Interactive Server |
| Componentes / CSS | MudBlazor                                    |
| Persistência     | Entity Framework Core 10 + SQLite             |
| Testes           | xUnit + SQLite em memória                     |

### Por que Blazor Server

A proposta pede atenção ao atrito entre frontend e backend em uma equipe pequena. Com Blazor em modo **Interactive Server**, UI e regras de negócio ficam no mesmo projeto e na mesma linguagem:

- **sem API REST intermediária:** os componentes chamam os services da camada Application por injeção de dependência, sem DTOs duplicados no cliente, sem serialização e sem versionamento de contrato;
- **um único build e um único deploy;**
- **tipagem de ponta a ponta:** uma mudança em um DTO quebra a compilação da tela que o usa, e não em runtime.

É o mesmo objetivo de abordagens como Inertia.js (monólito com UI reativa sem API separada), aplicado ao ecossistema .NET, que é a stack em que eu entrego mais qualidade no prazo proposto.

Não usei os modos WebAssembly/Auto: acrescentariam um segundo projeto e uma API para servir o cliente, sem ganho real para uma ferramenta interna de poucos usuários simultâneos.

### Por que MudBlazor

A proposta pede um framework CSS moderno que evite reinventar a roda. O MudBlazor é uma biblioteca de componentes Material Design para Blazor que já inclui grid, utilitários de espaçamento e cor, tabelas com ordenação, selects, diálogos e snackbars. Com ele não é preciso escrever CSS nem JavaScript próprios. O Bootstrap que vem no template do Blazor foi removido para evitar dois sistemas de estilo concorrentes.

### Por que SQLite

Zero setup para quem for avaliar: nenhum servidor, container ou credencial. O EF Core isola o provider; trocar para PostgreSQL ou SQL Server exige basicamente mudar o pacote e o `UseSqlite` no registro de DI, além de gerar novas migrations.

---

## Arquitetura

Organização em camadas (inspirada em Clean Architecture, sem cerimônia excessiva):

```
src/
  Codificar.Domain/          Entidades, enums, regras de negócio e exceções. Sem dependências.
  Codificar.Application/     Casos de uso (services), DTOs, distribuição automática.
                             Depende de Domain e de uma abstração do banco (IChamadosDbContext).
  Codificar.Infrastructure/  EF Core: DbContext, configurations, migrations, seed.
  Codificar.Web/             Blazor: páginas, componentes e composição (Program.cs).
tests/
  Codificar.UnitTests/       Domínio, distribuidor e services (SQLite em memória).
```

Dependências entre projetos:

```
Web ──► Application ──► Domain
 │            ▲
 └──► Infrastructure
```

- **Domain** não conhece EF nem ASP.NET. As regras (transições de status, validações, o que é "pendente") ficam na entidade, e não espalhadas pelas telas.
- **Application** expõe interfaces (`IChamadoService`, `IResponsavelService`, `IDistribuidorChamados`) consumidas pela Web.
- **Infrastructure** implementa a persistência e se registra com `AddInfrastructure(connectionString)`.
- **Web** só compõe: `AddApplication()`, `AddInfrastructure(...)` e `InitializeDatabaseAsync()`.

### Componentização da UI

| Componente        | Responsabilidade |
|-------------------|------------------|
| `ChamadoForm`     | Formulário reutilizado em **Novo** e **Editar**. Não chama services: recebe o modelo e expõe `OnSalvar`/`OnCancelar`. |
| `FiltrosChamados` | Filtros da listagem, com two-way binding (`@bind-Filtro`). |
| `PainelCarga`     | Carga pendente de cada responsável, exibida no topo da listagem. Destaca quem é o **próximo** da distribuição automática. |
| `StatusBadge`, `PrioridadeBadge` | Apresentação visual consistente de status e prioridade. |
| `ChamadoNaoEncontrado` | Aviso compartilhado pelas telas de detalhe e edição. |

Páginas e componentes com lógica usam code-behind (`.razor.cs`); componentes puramente visuais mantêm o `@code` inline. Os componentes recebem dados por parâmetro e devolvem ações por callback; só as páginas acessam os services.

---

## Regras de negócio

### Campos do chamado

| Campo          | Observação |
|----------------|------------|
| Título         | obrigatório, até 150 caracteres |
| Descrição      | obrigatória, até 2000 caracteres |
| Prioridade     | Baixa, Média ou Alta |
| Status         | Aberto, Em andamento, Resolvido ou Fechado |
| Responsável    | escolhido manualmente ou atribuído automaticamente |
| Solicitante    | obrigatório, texto livre (quem pediu) |
| Aberto em      | data/hora de abertura, definida pelo sistema |
| Atualizado em  | última alteração |
| Concluído em   | preenchido ao resolver; limpo ao reabrir |

### Fluxo de status

```
Aberto ──► Em andamento ──► Resolvido ──► Fechado
                ▲               │
                └── reabrir ────┘
```

- Um chamado **resolvido** pode ser **fechado** (confirmação) ou **reaberto** (volta para Em andamento).
- **Fechado** é um estado final.
- Transições fora desse fluxo lançam `TransicaoStatusInvalidaException`. A tela de detalhe só exibe as ações permitidas para o status atual (`PodeTransicionarPara`).

### Definição de "em aberto"

**Em aberto = `Aberto` ou `Em andamento`.**

A distribuição automática existe para equilibrar a **carga de trabalho**. Quando o chamado chega a `Resolvido`, o trabalho do responsável já terminou; `Fechado` é só a confirmação de quem pediu. Contar esses estados puniria quem resolve mais rápido.

A regra tem uma única fonte, `StatusChamadoExtensions.EstaPendente()`, usada pela entidade (ex.: não é possível reatribuir um chamado concluído), pelo distribuidor e pelos filtros.

### Distribuição automática

Ao abrir um chamado, o responsável pode ser escolhido manualmente ou deixado em **"Atribuir automaticamente"** (opção padrão). No modo automático:

1. considera apenas responsáveis **ativos**;
2. conta os chamados **em aberto** de cada um;
3. escolhe o de **menor carga**;
4. **empate:** vence o de **menor `Id`**. O critério é determinístico e previsível, o que facilita testar e explicar o comportamento;
5. se não houver responsável ativo, a operação falha com uma exceção de domínio.

A contagem é feita no banco, apoiada no índice `(ResponsavelId, Status)`.

### Listagem

- Painel de carga por responsável no topo.
- Filtros por status (multi-seleção, padrão: **pendentes**), prioridade e responsável, além de busca por título ou solicitante. Um botão restaura os filtros padrão.
- Ordenação padrão: pendentes primeiro → prioridade (alta → baixa) → mais antigos primeiro. Assim o que está esperando há mais tempo e é mais urgente aparece no topo.
- Filtros e ordenação são aplicados na consulta ao banco, e não em memória. Como a prioridade é gravada como texto, a ordenação usa o peso (Alta > Média > Baixa), e não a ordem alfabética.

---

## Testes automatizados

58 testes com **xUnit**, organizados por camada:

| Arquivo | O que cobre |
|---|---|
| `Domain/ChamadoTests` | Abertura e validações, todas as transições de status (permitidas e proibidas), `ConcluidoEm` ao resolver/reabrir/fechar, bloqueio de reatribuição de chamado concluído, próximos status possíveis. |
| `Domain/StatusChamadoExtensionsTests` | Definição de "em aberto". |
| `Application/DistribuidorChamadosTests` | Menor carga, desempate por menor `Id`, Resolvido/Fechado não contam, inativos ignorados, erro sem responsáveis ativos. |
| `Application/ResponsavelServiceTests` | Lista de ativos e painel de carga na ordem da distribuição. |
| `Application/ChamadoServiceTests` | Criação automática e manual, validação de responsável, uso do `TimeProvider`, filtros, busca, ordenação, edição reabrindo/resolvendo com troca de responsável, alteração de status. |

Os testes da camada Application rodam contra um **SQLite em memória** (uma conexão por teste, com o schema criado pelo próprio `ChamadosDbContext`). Assim as consultas passam pela mesma tradução LINQ → SQL da aplicação. As datas são controladas por um `TimeProvider` fixo (`RelogioFixo`).

---

## Decisões técnicas

| Decisão | Motivo |
|---|---|
| `IDbContextFactory` e um contexto por operação | No Blazor Server o *scope* de DI dura o circuito inteiro (a aba aberta). Um `DbContext` scoped viveria por horas, acumulando entidades rastreadas e sofrendo com concorrência. Cada operação do service abre e descarta o próprio contexto. |
| Datas em `DateTime` UTC | O SQLite não ordena `DateTimeOffset` nativamente. Todas as datas são gravadas em UTC, e um value converter (`UtcDateTimeConverter`) marca o `Kind` como UTC na leitura, porque o SQLite não guarda essa informação. |
| `TimeProvider` injetado | As regras que dependem de "agora" são testáveis com um relógio fixo, sem `DateTime.UtcNow` espalhado. |
| Limites de tamanho como constantes da entidade | O mesmo valor alimenta o mapeamento do EF e a validação do formulário (`[MaxLength]`). |
| Consulta de carga compartilhada | O painel de carga e a distribuição automática usam a mesma consulta, então nunca discordam sobre quem é o próximo. |
| Enums gravados como texto | O banco fica legível, e reordenar os membros do enum não corrompe os dados. |
| Entidade com setters privados | O estado só muda pelos métodos da entidade (`Abrir`, `Editar`, `AlterarStatus`...), que validam as invariantes. |
| FK com `DeleteBehavior.Restrict` | Não é possível apagar um responsável que tenha chamados. |
| Testes de service com SQLite em memória | Diferente do provider *InMemory* do EF, o SQLite em memória respeita constraints, FKs e a tradução real de LINQ para SQL. |
| `.editorconfig` | Namespaces file-scoped, chaves na mesma linha e tabs, aplicados pelo `dotnet format` e pela IDE. |

---

## Trade-offs

Decisões conscientes para caber no escopo e no prazo:

- **Sem autenticação:** o solicitante é informado como texto livre. Numa próxima versão, viria do usuário logado.
- **Responsáveis sem tela de cadastro:** são carregados via seed, como permite o escopo. O campo `Ativo` já existe para suportar a desativação sem apagar histórico.
- **Sem histórico de alterações:** fica registrado apenas `AtualizadoEm`. Uma trilha de auditoria (quem mudou o quê) seria o próximo passo.
- **Application depende do core do EF Core:** a interface `IChamadosDbContext` expõe `DbSet<T>`. Isso evita um repositório genérico que só repassaria chamadas ao EF, ao custo de acoplar a Application ao pacote `Microsoft.EntityFrameworkCore` (apenas o core, não o provider).
- **Migrations aplicadas na inicialização:** é conveniente para rodar localmente. Em produção, o ideal seria aplicá-las no pipeline de deploy.
- **Concorrência na distribuição:** duas aberturas simultâneas podem escolher o mesmo responsável, porque a contagem e a inserção não estão na mesma transação. Para uma ferramenta interna com poucos usuários é aceitável; resolver exigiria lock ou fila.
- **Edição lista só responsáveis ativos:** se o responsável de um chamado for desativado, ele não aparece no select de edição. Hoje não há como desativar pela interface.
- **Busca:** usa `LIKE` do SQLite, que ignora maiúsculas/minúsculas apenas em letras sem acento.
- **Fuso horário:** as datas são exibidas no fuso do servidor, o que coincide com o do usuário ao rodar localmente.
- **Sem paginação na listagem:** o filtro padrão (só pendentes) mantém a lista curta; com volume maior, a tabela passaria a paginar no banco.
- **Testes de componentes:** a UI foi validada manualmente; os testes automatizados cobrem domínio e Application.

---

## Bibliotecas utilizadas

| Pacote | Versão | Uso |
|---|---|---|
| [MudBlazor](https://mudblazor.com/) | 9.11.0 | Componentes de UI e estilo |
| Microsoft.EntityFrameworkCore | 10.0.12 | ORM (abstrações na Application) |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.12 | Provider SQLite |
| Microsoft.EntityFrameworkCore.Design | 10.0.12 | Geração de migrations |
| dotnet-ef (ferramenta local) | 10.0.12 | CLI de migrations |
| [xUnit](https://xunit.net/) | 2.9.3 | Framework de testes |
| xunit.runner.visualstudio | 3.1.4 | Execução dos testes no `dotnet test` e na IDE |
| Microsoft.NET.Test.Sdk | 17.14.1 | Infraestrutura de testes do .NET |
| coverlet.collector | 6.0.4 | Coleta de cobertura (`dotnet test --collect:"XPlat Code Coverage"`) |
| Fonte Roboto (Google Fonts) | — | Fonte recomendada pelo MudBlazor; sem internet, o navegador usa uma fonte substituta |

---

## Uso de IA

O [Claude Code](https://claude.com/claude-code) (Anthropic) foi usado como ferramenta de apoio à produtividade durante o desenvolvimento: discussão de alternativas de arquitetura, geração de trechos de código, investigação de problemas de ambiente (como o bloqueio do Smart App Control descrito abaixo), revisão e documentação.

As decisões de projeto foram tomadas por mim, e todo o código foi revisado, executado e testado antes de entrar no repositório.

---

## Solução de problemas

### `Uma política de Controle de Aplicativo bloqueou este arquivo (0x800711C7)`

No Windows 11 com o **Controle Inteligente de Aplicativos** (Smart App Control) ativo, o Windows pode bloquear DLLs recém-compiladas e sem assinatura, porque ainda não têm "reputação". O sintoma aparece no `dotnet ef` ou ao executar a aplicação:

```
Could not load file or assembly '...\Codificar.Application.dll'.
Uma política de Controle de Aplicativo bloqueou este arquivo. (0x800711C7)
No DbContext named '...ChamadosDbContext' was found.
```

O segundo erro é consequência do primeiro: sem a DLL, o EF não encontra o `DbContext`.

**Como confirmar:** Visualizador de Eventos → *Logs de Aplicativos e Serviços → Microsoft → Windows → CodeIntegrity → Operational* (eventos 3033/3077 citando a DLL do projeto).

**Solução:** *Segurança do Windows → Controle de aplicativos e navegador → Controle Inteligente de Aplicativos → Desativado*.

> Atenção: em versões mais antigas do Windows 11, reativar essa proteção exige reinstalar o sistema.

### A aplicação não encontra a connection string

O `Program.cs` falha logo na inicialização se `ConnectionStrings:Chamados` não existir. Ela está definida em `src/Codificar.Web/appsettings.json`; confira se o arquivo não foi alterado.

### O banco ficou em um estado estranho

Apague `src/Codificar.Web/chamados.db*` e rode de novo (ver [Resetar o banco](#resetar-o-banco)).
