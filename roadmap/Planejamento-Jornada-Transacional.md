# Planejamento de Desenvolvimento — Jornada Transacional

**Documento para o time de negócios** · Backlog detalhado, estimativas, capacity e roadmap de entrega
Versão 1.0 · Base: sessão de kickoff técnico (board de post-its)

---

## 1. Resumo executivo

O desenvolvimento da jornada transacional foi decomposto em **41 itens de trabalho** distribuídos em **cinco frentes** (Pré-requisitos, Backend, Frontend, Observability e Testes), executadas em **três ondas de entrega de três semanas cada** — nove semanas no total.

| Indicador | Valor |
| --- | --- |
| Itens de trabalho mapeados | 41 |
| Itens já estimados pelo time técnico | 13 (21 pontos) |
| Itens pendentes de Planning Poker | 28 |
| Estimativa total de referência | 121 pontos · 242 horas |
| Duração planejada | 9 semanas (3 ondas de 3 semanas) |
| Squad sugerido | 2 backend, 1 frontend, 1 SRE/infra, 1 QA — **a confirmar** |

As três ondas foram desenhadas para maximizar trabalho em paralelo: enquanto a infraestrutura é provisionada, a trilha de testes prepara massas e virtualiza endpoints, o que permite ao frontend desenvolver antes de o backend estar pronto.

> **Premissas abertas.** Os pontos dos 28 itens ainda não votados são valores de referência, usados apenas para dimensionar as ondas; serão substituídos pelo resultado da sessão de Planning Poker. A composição do squad também depende de confirmação.

---

## 2. Frentes de trabalho

| Frente | O que entrega | Itens |
| --- | --- | --- |
| Pré-requisitos | Contratos de endpoints, conta AWS, padrões de nome e toggles | 4 |
| Backend | Infraestrutura, gateways, BFF e peça habilitadora | 21 |
| Frontend | CloudFront, microfrontend Angular e componentes da jornada | 7 |
| Observability | Datadog, dashboards, alarmes e padronização de logs | 4 |
| Testes | Massas, virtualização, validação de integração, TAAC e pentest | 6 |

### Caminho crítico

1. Conta AWS YB3 e padrões de nome
2. Repositórios de infraestrutura e rede (NLB, ALB, VPC Link)
3. Cluster ECS e containers Fargate
4. Endpoints da peça habilitadora e do BFF
5. Integração frontend × BFF × Peça Centralizadora
6. TAAC, pentest e pacote de entrega

Tudo que não está nesta sequência é paralelizável.

---

## 3. Backlog detalhado

Legenda de perfil: Backend, Frontend, SRE/Infra, QA. Pontos "a votar" entram na sessão de Planning Poker.

| ID | Item | Frente | Onda | Depende de | Pontos | Perfil | Critério de conclusão |
| --- | --- | --- | --- | --- | --- | --- | --- |
| PR-01 | Definição de contratos dos endpoints analisados | Pré-requisitos | 1 | — | a votar | Backend + Frontend | Contrato acordado e publicado no repositório de contrato |
| PR-02 | Criação da nova conta AWS (YB3) | Pré-requisitos | 1 | — | a votar | SRE / Infra | Conta provisionada e acessos liberados |
| PR-03 | Definir padrões de nomes para as peças | Pré-requisitos | 1 | — | a votar | Backend + SRE | Padrão documentado e aplicado nos repositórios |
| PR-04 | Definição de toggles | Pré-requisitos | 1 | — | a votar | Backend | Lista de toggles e estratégia de ativação definidas |
| BE-01 | Criação Repositório Infra Gateway Negócio | Backend · Infra | 1 | PR-02, PR-03 | 1 | SRE / Infra | Repositório criado a partir do template e pipeline verde |
| BE-02 | Criação Repositório Contrato API Gateway Negócio | Backend · Infra | 1 | PR-01 | 1 | Backend | Contrato publicado e versionado |
| BE-03 | Criação Repositório Infra Gateway BFF | Backend · Infra | 1 | PR-02, PR-03 | 1 | SRE / Infra | Repositório criado e pipeline verde |
| BE-04 | Criação Repositório Contrato API Gateway BFF | Backend · Infra | 1 | PR-01 | 1 | Backend | Contrato publicado e versionado |
| BE-05 | Criação Repositório infra NLB + ALB + VPC Link | Backend · Infra | 1 | BE-01, BE-03 | 1 | SRE / Infra | Rede provisionada e rotas validadas |
| BE-06 | Configurar Caronte — somente no gateway BFF | Backend · Infra | 1 | BE-03 | 1 | SRE / Infra | Caronte ativo no gateway BFF |
| BE-07 | Provisionar cache Valkey | Backend · Infra | 1 | BE-05 | 1 | SRE / Infra | Cache disponível e testado |
| BE-08 | Provisionar quickconfig para portal manager | Backend · Infra | 1 | PR-02 | a votar | SRE / Infra | Quickconfig aplicado no portal manager |
| BE-09 | Criação da infra de cluster ECS | Backend · Infra | 1 | BE-05 | 2 | SRE / Infra | Cluster ativo e monitorado |
| BE-10 | Criação Container ECS Fargate (BFF) | Backend · Infra | 1 | BE-09 | 2 | SRE / Infra | Container sobe e responde ao health check |
| BE-11 | Criação Container ECS Fargate (Peça Habilitadora) | Backend · Infra | 1 | BE-09 | 2 | SRE / Infra | Container sobe e responde ao health check |
| BE-12 | Repositório Artifactory HTTPS endpoints | Backend · Infra | 1 | PR-03 | a votar | Backend | Dependência publicada no Artifactory |
| BE-13 | Criação da aplicação no portal de credenciais e scopes | Backend · Infra | 1 | PR-03 | 3 | Backend | Aplicação criada e scopes aprovados |
| BE-14 | Criação Repositório Secrets Manager | Backend · Infra | 1 | PR-02 | 3 | SRE / Infra | Segredos cadastrados e consumidos pela aplicação |
| BE-15 | Desenvolvimento da dependência do Artifactory de HTTPS endpoints | Backend · Dev | 2 | BE-12 | a votar | Backend | Biblioteca publicada e consumida pelo BFF |
| BE-16 | [Peça Habilitadora] Implementação dos endpoints de fundos | Backend · Dev | 2 | PR-01, BE-11 | a votar | Backend | Endpoints respondendo conforme contrato |
| BE-17 | [Peça Habilitadora] Implementação dos endpoints de termos | Backend · Dev | 2 | PR-01, BE-11 | a votar | Backend | Endpoints respondendo conforme contrato |
| BE-18 | [Peça Habilitadora] Implementação dos endpoints de perfil | Backend · Dev | 2 | PR-01, BE-11 | a votar | Backend | Endpoints respondendo conforme contrato |
| BE-19 | [BFF] Implementação dos endpoints complementares (fora da peça) | Backend · Dev | 2 | PR-01, BE-10 | a votar | Backend | Endpoints complementares disponíveis no BFF |
| BE-20 | Criação de testes unitários (90% de cobertura) | Backend · Dev | 2 | BE-16 a BE-19 | a votar | Backend | Cobertura mínima de 90% no pipeline |
| BE-21 | Integração entre BFF e Peça Centralizadora | Backend · Dev | 3 | BE-19 | a votar | Backend | Fluxo completo respondendo via Peça Centralizadora |
| FE-01 | Criação da infra CloudFront + bucket S3 | Frontend · Infra | 1 | PR-02 | 1 | SRE / Infra | Distribuição publicada e bucket versionado |
| FE-02 | Criação da aplicação microfrontend Angular (Module Federation) | Frontend · Infra | 1 | FE-01 | 1 | Frontend | Microfrontend carregando no host |
| FE-03 | Desenvolvimento do componente da primeira etapa da jornada (IDS + WCAG) | Frontend · Dev | 2 | FE-02, QA-03 | a votar | Frontend | Componente aprovado em IDS e WCAG |
| FE-04 | Desenvolvimento do componente do modal de termos (IDS + WCAG) | Frontend · Dev | 2 | FE-02, QA-03 | a votar | Frontend | Componente aprovado em IDS e WCAG |
| FE-05 | Implementar tagueamento | Frontend · Dev | 2 | Techspec do time de design | a votar | Frontend | Eventos disparando conforme techspec |
| FE-06 | Integração com endpoints backend — primeira etapa | Frontend · Dev | 3 | BE-16, BE-19 | a votar | Frontend | Tela consumindo endpoints reais |
| FE-07 | Integração com endpoints backend — modal de termos | Frontend · Dev | 3 | BE-17 | a votar | Frontend | Modal consumindo endpoints reais |
| OB-01 | Integração Datadog | Observability | 2 | BE-10, BE-11 | a votar | SRE / Infra | Métricas e traces chegando ao Datadog |
| OB-02 | Padronização de logs | Observability | 2 | BE-10, BE-11 | a votar | Backend + SRE | Logs no padrão definido e pesquisáveis |
| OB-03 | Construção de dashboard | Observability | 3 | OB-01 | a votar | SRE / Infra | Dashboard publicado e revisado com o time |
| OB-04 | Criação de alarmes | Observability | 3 | OB-01 | a votar | SRE / Infra | Alarmes com destinatários e limiares definidos |
| QA-01 | Criar massas específicas para a solução | Testes | 1 | PR-01 | a votar | QA | Massas disponíveis nos ambientes de teste |
| QA-02 | Mapeamento de cenários de testes para TAAC | Testes | 1 | PR-01 | a votar | QA | Cenários mapeados e priorizados |
| QA-03 | Virtualização dos endpoints | Testes | 1 | PR-01 | a votar | QA | Endpoints virtualizados disponíveis ao frontend |
| QA-04 | Validação da integração BFF × Peça Centralizadora | Testes | 3 | BE-21 | a votar | QA | Cenários de integração aprovados |
| QA-05 | Desenvolvimento de TAAC | Testes | 3 | QA-02, BE-21 | a votar | QA | Suíte automatizada executando no pipeline |
| QA-06 | Preparar pacote para pentest e criação da tarefa | Testes | 3 | BE-21, FE-06 | a votar | QA | Pacote entregue e tarefa aberta com a segurança |

---

## 4. Trabalho em paralelo

| Frente | Onda 1 | Onda 2 | Onda 3 |
| --- | --- | --- | --- |
| Pré-requisitos | Contratos, conta AWS, padrões e toggles | — | — |
| Backend | Repositórios, rede, ECS, containers, credenciais e secrets | Endpoints de fundos, termos, perfil, BFF e testes unitários | Integração BFF × Peça Centralizadora |
| Frontend | CloudFront, S3 e microfrontend Angular | Componentes da primeira etapa, modal de termos e tagueamento | Integração com endpoints reais |
| Observability | — | Datadog e padronização de logs | Dashboards e alarmes |
| Testes | Massas, cenários TAAC e virtualização | Acompanhamento dos testes unitários | Validação da integração, TAAC e pacote de pentest |

Três a quatro frentes correm simultaneamente em cada onda. O desacoplador principal é a virtualização dos endpoints (QA-03), que libera o frontend antes de o backend estar concluído.

---

## 5. Planning Poker

**Escala:** Fibonacci — 1, 2, 3, 5, 8, 13.
**Formato:** votação simultânea; divergência de duas casas ou mais abre discussão e novo voto.
**Pauta:** 28 itens pendentes, aproximadamente 90 minutos.
**Conversão para capacity:** 1 ponto = 2 horas de trabalho efetivo (referência definida no planejamento, a calibrar após a Onda 1).

| Pontos | Referência | Itens já votados nesta faixa |
| --- | --- | --- |
| 1 | Criação de repositório a partir de template | BE-01, BE-02, BE-03, BE-04 |
| 2 | Provisionamento com configuração própria | BE-09, BE-10, BE-11 |
| 3 | Item com dependência de outro time ou portal | BE-13, BE-14 |
| 5 | Conjunto de endpoints ou componente completo | — |
| 8 | Entrega com integração e validação ponta a ponta | — |
| 13 | Grande demais: quebrar antes de entrar na onda | — |

### Ordem sugerida da sessão

1. Pré-requisitos (4 itens) — definem o restante do escopo
2. Backend (9 itens pendentes)
3. Frontend (5 itens)
4. Observability (4 itens)
5. Testes (6 itens)

---

## 6. Capacity

**Premissas**

- Onda de 3 semanas = 15 dias úteis
- 6 horas produtivas por dia, já descontadas cerimônias e suporte
- Capacidade nominal: 90 h por pessoa por onda; 450 h por onda para um squad de 5 pessoas
- Conversão: 1 ponto = 2 horas

| Onda | Backend | Frontend | SRE / Infra | QA | Pontos | Horas |
| --- | --- | --- | --- | --- | --- | --- |
| Onda 1 · Fundação | 16 h | 6 h | 50 h | 10 h | 41 | 82 h |
| Onda 2 · Desenvolvimento | 56 h | 32 h | 10 h | — | 49 | 98 h |
| Onda 3 · Integração e entrega | 6 h | 16 h | 10 h | 30 h | 31 | 62 h |
| Total | 78 h | 54 h | 70 h | 40 h | 121 | 242 h |

A demanda estimada (242 h) fica bem abaixo da capacidade nominal do squad ao longo das nove semanas. Duas leituras são possíveis e ambas precisam de decisão do time: a razão de 2 horas por ponto pode estar subdimensionada, ou o squad pode ser alocado parcialmente neste projeto. **Recomendação:** usar a Onda 1 como calibragem e revisar a razão hora/ponto na primeira retrospectiva.

---

## 7. Roadmap por ondas

| Onda | Período | Escopo | Pontos | Horas | Critério de saída |
| --- | --- | --- | --- | --- | --- |
| Onda 1 · Fundação | Semanas 1–3 | Ambiente provisionado ponta a ponta: conta AWS, repositórios, rede, cluster, containers, distribuição de frontend e endpoints virtualizados. | 41 | 82 h | Aplicação sobe no cluster, pipeline roda ponta a ponta e o frontend consome endpoints virtualizados. |
| Onda 2 · Desenvolvimento | Semanas 4–6 | Endpoints da peça habilitadora e do BFF, componentes da jornada, tagueamento, Datadog e padronização de logs. | 49 | 98 h | Jornada navegável de ponta a ponta com endpoints próprios respondendo em ambiente de desenvolvimento. |
| Onda 3 · Integração e entrega | Semanas 7–9 | Integração BFF × Peça Centralizadora, frontend sobre endpoints reais, dashboards, alarmes, TAAC e pentest. | 31 | 62 h | Jornada integrada, monitorada no Datadog, com TAAC executando e pentest agendado. |

---

## 8. Riscos e dependências externas

| Risco | Impacto | Efeito no plano | Mitigação |
| --- | --- | --- | --- |
| Provisionamento da conta AWS YB3 | Alto | Atraso empurra toda a Onda 1. | Abrir a solicitação antes do início da Onda 1 e acompanhar semanalmente. |
| Techspec de tagueamento (time de design) | Médio | O card FE-05 sai da Onda 2. | Alinhar prazo com o time de design na primeira semana; plano B é levar o card para a Onda 3. |
| Contrato de endpoints não fechado | Alto | Backend e frontend trabalham sobre premissas divergentes e retrabalham na integração. | Fechar PR-01 antes de qualquer card de desenvolvimento entrar em execução. |
| Janela de pentest | Médio | A fila do time de segurança pode não caber na Onda 3. | Reservar a data assim que a Onda 2 começar. |
| Composição do squad não confirmada | Alto | Todo o capacity deste documento muda. | Confirmar nomes e percentual de alocação antes da Onda 1. |

---

## 9. Próximos passos

| # | Ação | Responsável | Prazo |
| --- | --- | --- | --- |
| 1 | Sessão de Planning Poker para os 28 itens pendentes | Time técnico | A definir |
| 2 | Confirmar composição do squad e percentual de alocação | Gestão | A definir |
| 3 | Definir a data de início da Onda 1 e reservar a janela de pentest | Gestão + Segurança | A definir |
| 4 | Abrir a solicitação da conta AWS YB3 | SRE / Infra | Antes da Onda 1 |
| 5 | Alinhar com o time de design a entrega da techspec de tagueamento | Frontend | Antes da Onda 2 |
