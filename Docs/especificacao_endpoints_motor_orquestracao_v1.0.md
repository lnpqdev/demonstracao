# ESPECIFICAÇÃO TÉCNICA DE ENDPOINTS

## Motor de Orquestração de Pré-Contratação de Fundos — Contrato por Etapa da Jornada

**Versão 1.0** — Julho de 2026 (26/07/2026)

> **Documento complementar.** Esta especificação assume como base as regras de negócio, a árvore de decisão (seção 5) e a máquina de estados (seção 6) descritas em `documento_tecnico_pre_contratacao_fundos_v0.2.md`. Ela não repete essas regras — apenas as traduz em contrato técnico, endpoint a endpoint. Consulte o documento v0.2 para o contexto, o escopo e os pontos em aberto (seção 11) ainda pendentes de validação com os times donos das APIs 1 a 8.

---

## Sumário

1. Objetivo e como usar este documento
2. Visão geral: um endpoint por etapa de decisão
3. Fichas de endpoint
4. Diagrama de componentes atualizado (Figura 1 v2)
5. Regras transversais de composição
6. Rastreabilidade com o documento v0.2
7. Registro de versões

---

## 1. Objetivo e como usar este documento

O documento v0.2 propôs um contrato ilustrativo do Motor de Orquestração com apenas dois endpoints genéricos: `GET /checklist` e `POST /eventos` (seção 9). Esse contrato é suficiente para discutir o conceito, mas não é suficiente para construir a solução: ele não define, etapa a etapa, **quais validações rodam**, **quais APIs de domínio são compostas**, **o que bloqueia o avanço do cliente** e **qual o próximo estado da jornada**.

Este documento resolve essa lacuna propondo **um endpoint por etapa de decisão** da jornada (seção 5.1 do v0.2), cada um responsável por:

- receber a chamada do BFF de Pré-Contratação (ou do Fluxo Transacional, quando aplicável);
- consultar/compor apenas as APIs de domínio relevantes àquela etapa;
- aplicar a regra de negócio daquela etapa e decidir se o cliente pode avançar, avança com pendência, ou é bloqueado;
- devolver o próximo estado da FSM (seção 6 do v0.2), para que o BFF/Frontend não precisem reconstruir a árvore de decisão.

Use este documento como o contrato de referência para implementação do Motor; use o v0.2 como referência de regra de negócio e para acompanhar os pontos ainda em aberto com os times donos das APIs.

## 2. Visão geral: um endpoint por etapa de decisão

| # | Etapa (ref. seção 5.1 do v0.2) | Estado FSM origem → destino (seção 6 do v0.2) | Endpoint | APIs de domínio compostas |
|---|---|---|---|---|
| 1 | Acesso à vitrine / checklist inicial (passos 1–2) | `INICIO` → `VITRINE_LIBERADA` ou `VITRINE_COM_PENDENCIA` | `GET /pre-contratacao/v1/checklist-inicial` | API 4 (perfil vigente), API 3 (termo geral assinado), API 8 (pendência cadastral), API 5 (CVM 50 / entes públicos / elegibilidade) |
| 2 | Seleção de produto (passos 3–4) | `VITRINE_*` → `PRODUTO_SELECIONADO` → `STEPS_DEFINIDOS` | `POST /pre-contratacao/v1/selecao-produto` | API 7 (catálogo, atributo termo pré-determinado), API 3 (regra de termo do produto), API 1 (fundo elegível para aplicação) |
| 3 | Coleta de perfil/termo — step do fluxo transacional (passos 5–6) | `STEPS_DEFINIDOS` → `COLETA_PERFIL_TERMO` | `POST /pre-contratacao/v1/steps/perfil-termo` | API 4 (preenchimento de perfil), API 3 (assinatura de termo do produto, TIQ/TIP, TDI) |
| 4 | Simulação (passo 7) | `COLETA_PERFIL_TERMO` → `SIMULACAO` | `POST /pre-contratacao/v1/simulacao` | API 1 (posição/benchmark), API 6 (saldo disponível) |
| 5 | Verificação de desenquadramento (passo 8) | `SIMULACAO` → `VALIDACAO_ENQUADRAMENTO` | `POST /pre-contratacao/v1/validacao-enquadramento` | API 4 (validações de suitability / desenquadramento) |
| 6 | Termo de ciência de desenquadramento — condicional (passo 8a) | `VALIDACAO_ENQUADRAMENTO` → `TERMO_DESENQUADRAMENTO` | `POST /pre-contratacao/v1/termo-desenquadramento` | API 3 (termo de ciência de risco) |
| 7 | Conclusão (passos 8a/8b) | `TERMO_DESENQUADRAMENTO` ou `VALIDACAO_ENQUADRAMENTO` → `CONCLUIDO` | `POST /pre-contratacao/v1/concluir` | API 2 (criar contratação, efetivar contratação/2º fator) |
| — | Evento transversal de conclusão de step | Recalcula `estadoAtual` a partir de qualquer estado | `POST /pre-contratacao/v1/eventos` | Reaproveita a validação do endpoint da etapa correspondente (mantido do v0.2, seção 9.2) |

> **Nota:** os endpoints 1–7 são a decomposição do único nó "Motor de Orquestração" da Figura 1 do v0.2. Cada um pode ser implementado como uma rota HTTP distinta ou como um handler interno do mesmo serviço — o contrato é o mesmo nos dois casos. Ver seção 4.

## 3. Fichas de endpoint

Cada ficha segue o mesmo padrão de campos da seção 8 do documento v0.2 (rota, regra, request, response, autenticação, pendências), acrescido de um campo específico de **validações que bloqueiam o avanço** — o elemento central desta especificação.

### 3.1 Checklist inicial da vitrine

| Campo | Conteúdo |
|---|---|
| Rota | `GET /pre-contratacao/v1/checklist-inicial?clienteId={id}&canal={web\|app}` |
| Objetivo (ref. v0.2) | Passos 1–2 da seção 5.1: decidir se a vitrine é exibida normalmente ou com banner de pendência. |
| Request | `{ "clienteId": "123456", "canal": "app" }` |
| Response | ```{ "clienteId": "123456", "estadoAtual": "VITRINE_LIBERADA", "bloqueiaSelecao": false, "pendencias": { "perfilInvestidor": false, "termoGeralAssinado": true, "pendenciaCadastral": false, "elegibilidadeCvm50": true }, "exibirBanner": false }``` |
| Validações executadas | 1. API 4 → possui perfil de investimento vigente? 2. API 3 → possui termo geral assinado? (1 OU 2 = sem banner) 3. API 8 → possui pendência cadastral? (proposta: compõe o banner, não bloqueia seleção — a confirmar, ver seção 11 do v0.2) 4. API 5 → elegibilidade CVM 50/entes públicos, quando aplicável ao perfil do cliente (a confirmar, ver seção 11 do v0.2). **Nenhuma dessas validações bloqueia a seleção de produto** — apenas determinam `exibirBanner`, conforme passo 2b da seção 5.1 do v0.2. |
| Próximo(s) estado(s) | `VITRINE_LIBERADA` (sem pendência) ou `VITRINE_COM_PENDENCIA` (com pendência) — seleção de produto liberada em ambos os casos. |
| Autenticação / Headers | *A preencher com o padrão de API interno.* |
| Pendências herdadas do v0.2 | Onde exatamente a pendência cadastral (API 8) entra na árvore — só banner ou também bloqueio de efetivação (revalidar no endpoint 3.7). Ponto da jornada e perfis aplicáveis para API 5. |

### 3.2 Seleção de produto

| Campo | Conteúdo |
|---|---|
| Rota | `POST /pre-contratacao/v1/selecao-produto` |
| Objetivo (ref. v0.2) | Passos 3–4 da seção 5.1: verificar se o produto escolhido tem termo pré-determinado e montar os steps do fluxo transacional. |
| Request | `{ "clienteId": "123456", "produtoId": "FIC-RF-001" }` |
| Response | ```{ "estadoAtual": "STEPS_DEFINIDOS", "produto": { "elegivel": true, "possuiTermoPreDeterminado": true, "termoId": "TERMO-FIC-RF-001" }, "stepsNecessarios": ["ASSINATURA_TERMO_PRODUTO", "SIMULACAO", "VALIDACAO_ENQUADRAMENTO"] }``` |
| Validações executadas | 1. API 1 → produto está na lista de fundos elegíveis para aplicação do cliente? (bloqueia se não elegível) 2. API 7 → produto possui atributo de termo pré-determinado no catálogo? 3. API 3 → cruzamento com regra de termo associada ao produto (a confirmar se o atributo vive na API 7 ou na API 3, ver seção 11 do v0.2). Se (1) falhar, **bloqueia a seleção**; (2)/(3) apenas decidem qual step adicional será incluído (termo específico vs. step genérico — passos 4a/4b do v0.2). |
| Próximo(s) estado(s) | `PRODUTO_SELECIONADO` → `STEPS_DEFINIDOS`. |
| Autenticação / Headers | *A preencher com o padrão de API interno.* |
| Pendências herdadas do v0.2 | Confirmar se "produto possui termo pré-determinado" vive na API 7 ou na API 3. |

### 3.3 Coleta de perfil/termo (step do fluxo transacional)

| Campo | Conteúdo |
|---|---|
| Rota | `POST /pre-contratacao/v1/steps/perfil-termo` |
| Objetivo (ref. v0.2) | Passo 5 da seção 5.1: oferecer preencher perfil, TIQ/TIP, TDI, ou seguir sem perfil (mantendo pendência). |
| Request | `{ "clienteId": "123456", "produtoId": "FIC-RF-001", "opcaoEscolhida": "PREENCHER_PERFIL", "dadosPerfil": { "...": "..." } }` |
| Response | ```{ "estadoAtual": "SIMULACAO", "perfilAtualizado": true, "termosAssinados": ["TERMO-FIC-RF-001"], "seguiuSemPerfil": false }``` |
| Validações executadas | Conforme a opção escolhida: (a) `PREENCHER_PERFIL` → grava via API 4; (b) `ASSINAR_TIQ_TIP` ou `ASSINAR_TDI` → grava via API 3 (nome oficial do termo e regra de elegibilidade a confirmar, ver seção 11 do v0.2 — texto original citava "TCQ"); (c) `ASSINAR_TERMO_PRODUTO` → grava via API 3 o termo definido no endpoint 3.2; (d) `SEGUIR_SEM_PERFIL` → **não bloqueia**, mas precisa ser registrado para auditoria/compliance (API responsável a definir, ver seção 11 do v0.2). Nenhuma dessas opções bloqueia o avanço para a simulação — a pendência apenas é carregada para os passos seguintes. |
| Próximo(s) estado(s) | `SIMULACAO`. |
| Autenticação / Headers | *A preencher com o padrão de API interno.* |
| Pendências herdadas do v0.2 | Nome oficial do termo TIQ/TIP ("TCQ" no texto original); critério TIQ/TIP vs. TDI; onde se registra "seguir sem perfil". |

### 3.4 Simulação

| Campo | Conteúdo |
|---|---|
| Rota | `POST /pre-contratacao/v1/simulacao` |
| Objetivo (ref. v0.2) | Passo 6 da seção 5.1: simular o produto escolhido com dados de posição/benchmark e saldo. |
| Request | `{ "clienteId": "123456", "produtoId": "FIC-RF-001", "valorAporte": 5000.00 }` |
| Response | ```{ "estadoAtual": "VALIDACAO_ENQUADRAMENTO", "saldoDisponivel": 12000.00, "saldoSuficiente": true, "benchmark": { "...": "..." }, "posicaoAtual": { "...": "..." } }``` |
| Validações executadas | 1. API 6 → saldo disponível cobre o valor de aporte simulado? (se não, **bloqueia** — não é possível concluir a etapa) 2. API 1 → benchmark e posição atual do produto, apenas informativo (não bloqueia). |
| Próximo(s) estado(s) | `VALIDACAO_ENQUADRAMENTO`. |
| Autenticação / Headers | *A preencher com o padrão de API interno.* |
| Pendências herdadas do v0.2 | Nenhuma pendência de fluxo mapeada nesta versão para API 1/6; falta fechar contrato técnico real. |

### 3.5 Validação de enquadramento

| Campo | Conteúdo |
|---|---|
| Rota | `POST /pre-contratacao/v1/validacao-enquadramento` |
| Objetivo (ref. v0.2) | Passo 7 da seção 5.1: verificar se a operação simulada gera desenquadramento do perfil frente à política da empresa. |
| Request | `{ "clienteId": "123456", "produtoId": "FIC-RF-001", "valorAporte": 5000.00 }` |
| Response | ```{ "estadoAtual": "TERMO_DESENQUADRAMENTO", "desenquadra": true, "motivoDesenquadramento": "..." }``` |
| Validações executadas | API 4 → validação de suitability aplicada ao valor simulado e ao perfil vigente do cliente (critério exato de cálculo — antes/depois da simulação e variáveis envolvidas — a confirmar, ver seção 11 do v0.2). Se `desenquadra=true`, **bloqueia a conclusão até assinatura do termo de ciência** (endpoint 3.6); se `false`, segue direto para conclusão. |
| Próximo(s) estado(s) | `TERMO_DESENQUADRAMENTO` (se desenquadra) ou `CONCLUIDO` (se não desenquadra). |
| Autenticação / Headers | *A preencher com o padrão de API interno.* |
| Pendências herdadas do v0.2 | Critério exato de cálculo do desenquadramento (momento e variáveis). |

### 3.6 Termo de ciência de desenquadramento (condicional)

| Campo | Conteúdo |
|---|---|
| Rota | `POST /pre-contratacao/v1/termo-desenquadramento` |
| Objetivo (ref. v0.2) | Passo 8a da seção 5.1: coletar a assinatura do termo de ciência de desenquadramento. |
| Request | `{ "clienteId": "123456", "produtoId": "FIC-RF-001", "aceite": true }` |
| Response | ```{ "estadoAtual": "CONCLUIDO", "termoAssinado": true }``` |
| Validações executadas | API 3 → grava o termo de ciência de risco. Se `aceite=false`, **bloqueia a conclusão** (o cliente não pode efetivar a operação desenquadrada sem esse termo). |
| Próximo(s) estado(s) | `CONCLUIDO` (somente se `aceite=true`). |
| Autenticação / Headers | *A preencher com o padrão de API interno.* |
| Pendências herdadas do v0.2 | Nenhuma pendência de fluxo mapeada nesta versão além do contrato técnico real. |

### 3.7 Conclusão

| Campo | Conteúdo |
|---|---|
| Rota | `POST /pre-contratacao/v1/concluir` |
| Objetivo (ref. v0.2) | Passos 8a/8b da seção 5.1: criar e efetivar a contratação (2º fator), encerrando a jornada. |
| Request | `{ "clienteId": "123456", "produtoId": "FIC-RF-001", "valorAporte": 5000.00, "segundoFator": "..." }` |
| Response | ```{ "estadoAtual": "CONCLUIDO", "contratacaoId": "CTR-987654", "efetivado": true }``` |
| Validações executadas | 1. API 2 → validações/permissão de aplicação (suitability e/ou saldo — a confirmar, ver seção 11 do v0.2), cria e efetiva a contratação mediante 2º fator. 2. Revalidação recomendada, mas a confirmar com negócio (seção 11 do v0.2): pendência cadastral (API 8) e elegibilidade CVM 50 (API 5) antes da efetivação final, não apenas no checklist inicial. **Qualquer falha aqui bloqueia a efetivação** — é o único ponto de não-retorno da jornada. |
| Próximo(s) estado(s) | `CONCLUIDO` (estado final). |
| Autenticação / Headers | *A preencher com o padrão de API interno.* |
| Pendências herdadas do v0.2 | Se "validações/permissão de aplicação" é suitability, saldo, ou ambos; se API 8/API 5 devem ser revalidadas neste ponto. |

### 3.8 Evento de conclusão de step (transversal)

| Campo | Conteúdo |
|---|---|
| Rota | `POST /pre-contratacao/v1/eventos` |
| Objetivo (ref. v0.2) | Seção 9.2 do v0.2: usado pelo Fluxo Transacional (API 2) para informar que um step foi concluído fora da sequência direta do Motor (ex.: assinatura feita em tela própria da API 2), permitindo recalcular `estadoAtual`. |
| Request | `{ "clienteId": "123456", "produtoId": "FIC-RF-001", "stepConcluido": "ASSINATURA_TERMO_PRODUTO" }` |
| Response | ```{ "estadoAtual": "SIMULACAO" }``` |
| Validações executadas | Delega para a mesma validação do endpoint correspondente ao step informado (não duplica regra — apenas reexecuta a checagem daquele endpoint para confirmar a transição de estado). |
| Próximo(s) estado(s) | Depende do `stepConcluido` informado — mesma tabela da seção 2. |
| Autenticação / Headers | *A preencher com o padrão de API interno.* |
| Pendências herdadas do v0.2 | Formato definitivo do contrato do motor, alinhado ao padrão de API interno. |

## 4. Diagrama de componentes atualizado (Figura 1 v2)

![Figura 1 v2 — Componentes com Motor decomposto por etapa](figura1_componentes_v2.png)

> **Fonte editável:** `figura1_componentes_v2.dot` (Graphviz). Mesma convenção de edição da Fig. 1 original (ver seção 13 do v0.2).

**Diferença em relação à Figura 1 do v0.2:** o nó único "Motor de Orquestração" foi decomposto nos 7 endpoints da seção 2 deste documento (mais o endpoint transversal de eventos), cada um desenhado com aresta direta apenas para a(s) API(s) de domínio que efetivamente consulta naquela etapa — tornando explícito, no próprio diagrama, qual validação acontece em qual ponto da composição.

## 5. Regras transversais de composição

- **`estadoAtual` como fonte de verdade** (reforça seção 10 do v0.2): todo endpoint devolve o estado FSM resultante; o BFF/Frontend nunca deduz o próximo passo por conta própria.
- **Bloqueio vs. pendência não bloqueante**: cada ficha da seção 3 declara explicitamente se a falha de uma validação impede o avanço (`bloqueiaSelecao`, `saldoSuficiente=false`, `aceite=false` etc.) ou apenas registra uma pendência informativa (banner). Essa distinção deve estar sempre presente na resposta — nunca apenas implícita no código HTTP.
- **Idempotência e reentrada**: se o cliente reenviar a mesma etapa (ex.: F5, app em background), o endpoint deve devolver o mesmo `estadoAtual` sem duplicar efeitos (ex.: não assinar o mesmo termo duas vezes na API 3). Se o cliente voltar a uma etapa anterior, o Motor deve aceitar reexecutar a validação e recalcular o estado a partir dali — não é permitido "pular" etapas.
- **Comportamento em falha/timeout de API de domínio** (proposta, a validar com arquitetura — ver seção 11 do v0.2): *fail-closed* (bloqueia o avanço) para validações que impedem a efetivação (API 2, API 6 nas etapas de saldo, API 4 na etapa de enquadramento); *fail-open* (segue com pendência sinalizada) para validações apenas informativas de banner (ex.: API 8 e API 5 no checklist inicial, enquanto não confirmado que bloqueiam efetivação).

## 6. Rastreabilidade com o documento v0.2

| Endpoint (este documento) | Seção 7 do v0.2 (regra × API × rota) | Seção 8 do v0.2 (ficha de API) |
|---|---|---|
| `GET /checklist-inicial` | Linhas "Consultar perfil de investimento vigente", "Consultar termo geral já assinado", "Validar pendências cadastrais", "Consultar CVM 50..." | 8.4 (API 4), 8.3 (API 3), 8.8 (API 8), 8.5 (API 5) |
| `POST /selecao-produto` | Linhas "Exibir catálogo de produtos", "Consultar fundos elegíveis...", "Verificar se produto tem termo pré-determinado" | 8.7 (API 7), 8.1 (API 1), 8.3 (API 3) |
| `POST /steps/perfil-termo` | Linhas "Assinar termo específico do produto", "Preencher perfil de investidor", "Assinar TIQ/TIP", "Assinar TDI", "Seguir sem perfil..." | 8.4 (API 4), 8.3 (API 3) |
| `POST /simulacao` | Linhas "Consultar saldo disponível em conta", "Consultar benchmarks do produto" | 8.6 (API 6), 8.1 (API 1) |
| `POST /validacao-enquadramento` | Linha "Verificar desenquadramento de perfil x produto" | 8.4 (API 4) |
| `POST /termo-desenquadramento` | Linha "Assinar termo de ciência de desenquadramento" | 8.3 (API 3) |
| `POST /concluir` | Linhas "Criar a contratação", "Efetivar contratação (2º fator)", "Validações/permissão de aplicação" | 8.2 (API 2) |
| `POST /eventos` | Seção 9.2 do v0.2 | — |

## 7. Registro de versões

| Versão | Data | Alteração |
|---|---|---|
| 1.0 | 26/07/2026 | Primeira versão desta especificação: decomposição do contrato genérico do Motor (seção 9 do v0.2) em um endpoint por etapa de decisão, com validações explícitas, requests/responses ilustrativos e diagrama de componentes v2. Documento complementar ao `documento_tecnico_pre_contratacao_fundos_v0.2.md`. |
