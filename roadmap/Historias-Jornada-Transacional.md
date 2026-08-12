# Histórias de Usuário — Jornada Transacional

**Documento para o time de negócios e para o time técnico**
Os 41 cards do board de kickoff foram reagrupados em **21 histórias de usuário**, cada uma com dor, narrativa, critérios de aceite, Definition of Ready e Definition of Done.
Versão 1.0

---

## Resumo

| Indicador | Valor |
| --- | --- |
| Histórias | 21 |
| Cards de origem consolidados | 41 |
| Épicos | 5 |
| Estimativa de referência | 112 pontos · 224 horas |
| Distribuição | Onda 1: 36 pts · Onda 2: 52 pts · Onda 3: 24 pts |

### Épicos

| Épico | Histórias | Pontos |
| --- | --- | --- |
| A · Fundação de plataforma | 7 | 31 |
| B · Habilitação de dados | 5 | 31 |
| C · Experiência da jornada | 3 | 16 |
| D · Qualidade e integração | 4 | 24 |
| E · Observabilidade | 2 | 10 |

### Critério de agrupamento

Cards foram unidos quando entregam valor apenas em conjunto: infraestrutura de um mesmo recurso (gateways e seus contratos), componente de tela e sua integração com o backend, e telemetria com padronização de logs. Cards foram mantidos separados quando podem ser trabalhados por pessoas diferentes em paralelo — é o caso dos endpoints de fundos, termos e perfil.

---

## Índice

| ID | História | Épico | Onda | Pontos |
| --- | --- | --- | --- | --- |
| HU-01 | Conta AWS e padrões de nomenclatura | A | 1 | 3 |
| HU-02 | Contratos de API e estratégia de toggles | A | 1 | 5 |
| HU-03 | Gateways de negócio e BFF provisionados | A | 1 | 5 |
| HU-04 | Rede e conectividade privada | A | 1 | 2 |
| HU-05 | Runtime de containers para BFF e peça habilitadora | A | 1 | 6 |
| HU-06 | Credenciais, segredos, cache e configuração | A | 1 | 8 |
| HU-07 | Distribuição e casca do frontend | A | 1 | 2 |
| HU-08 | Biblioteca de HTTPS endpoints | B | 2 | 5 |
| HU-09 | Endpoints de fundos na peça habilitadora | B | 2 | 8 |
| HU-10 | Endpoints de termos na peça habilitadora | B | 2 | 5 |
| HU-11 | Endpoints de perfil na peça habilitadora | B | 2 | 5 |
| HU-12 | BFF com endpoints complementares e cobertura de testes | B | 2 | 8 |
| HU-13 | Primeira etapa da jornada transacional | C | 2 | 8 |
| HU-14 | Modal de aceite de termos | C | 2 | 5 |
| HU-15 | Tagueamento da jornada | C | 2 | 3 |
| HU-16 | Massas de teste e virtualização de endpoints | D | 1 | 5 |
| HU-17 | Cenários e automação de testes (TAAC) | D | 3 | 8 |
| HU-18 | Integração com a Peça Centralizadora | D | 3 | 8 |
| HU-19 | Pacote de segurança e pentest | D | 3 | 3 |
| HU-20 | Telemetria e padronização de logs | E | 2 | 5 |
| HU-21 | Dashboards e alarmes operacionais | E | 3 | 5 |

---

# Onda 1 · Fundação (semanas 1–3)

## HU-01 — Conta AWS e padrões de nomenclatura

**Épico:** A · Fundação de plataforma · **Perfil:** SRE / Infra · **Estimativa de referência:** 3 pontos (6 h) · **Cards de origem:** PR-02, PR-03

### Dor
Não existe uma conta AWS dedicada nem convenção de nomes acordada. Cada peça criada agora nasce em ambiente compartilhado e com nome improvisado, o que gera retrabalho de migração e dificulta rastrear custo por produto.

### História
> Como time de plataforma, quero uma conta AWS dedicada e um padrão de nomes acordado para que todos os recursos da jornada nasçam isolados, rastreáveis e com custo atribuível.

### Critérios de aceite
- [ ] A conta YB3 está criada e com acessos concedidos aos perfis do squad
- [ ] O documento de padrão de nomes cobre gateways, containers, repositórios, segredos e buckets
- [ ] O padrão está aplicado nos primeiros recursos criados
- [ ] Tags de custo estão definidas e obrigatórias

### Definition of Ready
- [ ] Solicitação de conta aberta com o time de cloud
- [ ] Lista de perfis e níveis de acesso definida
- [ ] Convenção de nomes rascunhada e revisada pelo arquiteto

### Definition of Done
- [ ] Acesso validado por pelo menos dois perfis diferentes
- [ ] Padrão publicado no repositório de documentação
- [ ] Nenhum recurso da jornada fora do padrão

## HU-02 — Contratos de API e estratégia de toggles

**Épico:** A · Fundação de plataforma · **Perfil:** Backend + Frontend · **Estimativa de referência:** 5 pontos (10 h) · **Cards de origem:** PR-01, PR-04

### Dor
Backend e frontend precisam começar ao mesmo tempo, mas sem contrato fechado cada lado assume um formato de resposta diferente e a divergência só aparece na integração, no fim do projeto.

### História
> Como squad, quero os contratos dos endpoints publicados e versionados e a estratégia de toggles definida para que backend e frontend trabalhem em paralelo sem risco de retrabalho na integração.

### Critérios de aceite
- [ ] Contrato publicado no repositório, com schema de request, response e erros
- [ ] Contrato revisado e aprovado por backend, frontend e QA
- [ ] Lista de toggles definida, com nome, escopo e critério de ativação
- [ ] Estratégia de rollback por toggle documentada

### Definition of Ready
- [ ] Endpoints analisados e mapeados
- [ ] Peça Centralizadora consultada sobre campos disponíveis
- [ ] Padrão de nomes (HU-01) definido

### Definition of Done
- [ ] Contrato versionado e consumido pela virtualização (HU-16)
- [ ] Toggles cadastrados na ferramenta de configuração
- [ ] Sem campos marcados como "a definir"

## HU-03 — Gateways de negócio e BFF provisionados

**Épico:** A · Fundação de plataforma · **Perfil:** SRE / Infra + Backend · **Estimativa de referência:** 5 pontos (10 h) · **Cards de origem:** BE-01, BE-02, BE-03, BE-04, BE-06

### Dor
Sem os gateways não há porta de entrada para a jornada. Hoje as chamadas não têm ponto de controle de autenticação, throttling nem versionamento de contrato.

### História
> Como time de plataforma, quero os gateways de negócio e de BFF provisionados com seus contratos versionados para que a jornada tenha uma porta de entrada controlada e auditável.

### Critérios de aceite
- [ ] Quatro repositórios criados a partir do template: infra e contrato para cada gateway
- [ ] Pipelines executando com sucesso em ambiente de desenvolvimento
- [ ] Caronte configurado no gateway BFF
- [ ] Chamada de teste atravessa o gateway e retorna o esperado

### Definition of Ready
- [ ] Conta AWS disponível (HU-01)
- [ ] Contrato de API publicado (HU-02)
- [ ] Template de repositório acessível ao squad

### Definition of Done
- [ ] Pipelines verdes nos quatro repositórios
- [ ] Rota de smoke test respondendo
- [ ] Configuração revisada em code review

## HU-04 — Rede e conectividade privada

**Épico:** A · Fundação de plataforma · **Perfil:** SRE / Infra · **Estimativa de referência:** 2 pontos (4 h) · **Cards de origem:** BE-05

### Dor
Sem NLB, ALB e VPC Link o gateway não alcança os containers em rede privada, e expor os serviços publicamente não é opção.

### História
> Como time de plataforma, quero a camada de rede provisionada para que o gateway alcance os containers sem exposição pública.

### Critérios de aceite
- [ ] NLB, ALB e VPC Link provisionados via repositório de infraestrutura
- [ ] Rota do gateway até o container validada em ambiente de desenvolvimento
- [ ] Security groups restritos ao tráfego necessário

### Definition of Ready
- [ ] Gateways provisionados (HU-03)
- [ ] Desenho de rede aprovado pela arquitetura

### Definition of Done
- [ ] Teste de conectividade executado e registrado
- [ ] Nenhum recurso com acesso público não intencional

## HU-05 — Runtime de containers para BFF e peça habilitadora

**Épico:** A · Fundação de plataforma · **Perfil:** SRE / Infra · **Estimativa de referência:** 6 pontos (12 h) · **Cards de origem:** BE-09, BE-10, BE-11

### Dor
Não há onde executar as aplicações. Sem cluster e containers, todo o desenvolvimento de backend fica sem ambiente de verificação.

### História
> Como time de backend, quero o cluster ECS e os containers Fargate do BFF e da peça habilitadora provisionados para que as aplicações tenham onde rodar desde o primeiro commit.

### Critérios de aceite
- [ ] Cluster ECS ativo, com autoscaling e limites definidos
- [ ] Container do BFF sobe e responde ao health check
- [ ] Container da peça habilitadora sobe e responde ao health check
- [ ] Deploy automatizado pelo pipeline

### Definition of Ready
- [ ] Rede provisionada (HU-04)
- [ ] Imagem base e runtime definidos
- [ ] Padrão de health check acordado

### Definition of Done
- [ ] Deploy executado do pipeline sem intervenção manual
- [ ] Health checks estáveis por 24 horas
- [ ] Rollback testado

## HU-06 — Credenciais, segredos, cache e configuração

**Épico:** A · Fundação de plataforma · **Perfil:** SRE / Infra + Backend · **Estimativa de referência:** 8 pontos (16 h) · **Cards de origem:** BE-07, BE-08, BE-13, BE-14

### Dor
As aplicações precisam de credenciais e segredos para acessar dependências, e hoje não há nem aplicação registrada no portal de credenciais nem cofre configurado. Sem cache, cada chamada repetida vai até a origem.

### História
> Como time de backend, quero credenciais, segredos, cache e configuração provisionados para que as aplicações acessem suas dependências com segurança e sem repetir chamadas desnecessárias.

### Critérios de aceite
- [ ] Aplicação criada no portal de credenciais com os scopes aprovados
- [ ] Repositório de Secrets Manager criado e consumido pelas aplicações
- [ ] Cache Valkey provisionado e validado
- [ ] Quickconfig aplicado no portal manager
- [ ] Nenhum segredo em código ou variável de ambiente em texto puro

### Definition of Ready
- [ ] Conta AWS e padrão de nomes definidos (HU-01)
- [ ] Scopes necessários levantados com a equipe de segurança
- [ ] Política de expiração de cache definida

### Definition of Done
- [ ] Rotação de segredos documentada
- [ ] Aplicações lendo segredos do cofre em ambiente de desenvolvimento
- [ ] Métricas de hit e miss do cache visíveis

## HU-07 — Distribuição e casca do frontend

**Épico:** A · Fundação de plataforma · **Perfil:** SRE / Infra + Frontend · **Estimativa de referência:** 2 pontos (4 h) · **Cards de origem:** FE-01, FE-02

### Dor
Não existe onde publicar o frontend nem casca de microfrontend registrada no host, então nenhum componente pode ser visto fora da máquina do desenvolvedor.

### História
> Como time de frontend, quero o CloudFront, o bucket S3 e o microfrontend Angular publicados para que qualquer componente desenvolvido seja acessível em ambiente compartilhado.

### Critérios de aceite
- [ ] Distribuição CloudFront ativa apontando para o bucket S3 versionado
- [ ] Microfrontend Angular carregando no host via Module Federation
- [ ] Deploy automatizado pelo pipeline
- [ ] Política de cache e invalidação definida

### Definition of Ready
- [ ] Conta AWS disponível (HU-01)
- [ ] Host de microfrontends identificado e contato definido
- [ ] Versão do Angular e do Module Federation acordadas

### Definition of Done
- [ ] Página de exemplo publicada e acessível
- [ ] Invalidação de cache testada
- [ ] Pipeline de deploy verde

## HU-16 — Massas de teste e virtualização de endpoints

**Épico:** D · Qualidade e integração · **Perfil:** QA · **Estimativa de referência:** 5 pontos (10 h) · **Cards de origem:** QA-01, QA-03

### Dor
O frontend depende do backend para começar, e o backend depende de massas para ser testado. Sem virtualização, as duas trilhas viram uma fila.

### História
> Como squad, quero massas específicas e endpoints virtualizados desde a primeira onda para que o frontend desenvolva em paralelo ao backend, sem espera.

### Critérios de aceite
- [ ] Massas cobrindo cenários de sucesso, erro e borda para fundos, termos e perfil
- [ ] Endpoints virtualizados respondendo conforme o contrato publicado
- [ ] Virtualização acessível ao frontend em ambiente compartilhado
- [ ] Documentação de como apontar a aplicação para a virtualização

### Definition of Ready
- [ ] Contrato publicado (HU-02)
- [ ] Ferramenta de virtualização definida e disponível
- [ ] Cenários de negócio levantados com o time de produto

### Definition of Done
- [ ] Frontend consumindo a virtualização com sucesso
- [ ] Massas versionadas junto ao repositório de testes
- [ ] Divergências entre virtualização e contrato zeradas

---

# Onda 2 · Desenvolvimento (semanas 4–6)

## HU-08 — Biblioteca de HTTPS endpoints

**Épico:** B · Habilitação de dados · **Perfil:** Backend · **Estimativa de referência:** 5 pontos (10 h) · **Cards de origem:** BE-12, BE-15

### Dor
Cada aplicação implementaria sua própria camada de chamada HTTPS, duplicando tratamento de erro, timeout e retry — e divergindo com o tempo.

### História
> Como time de backend, quero uma biblioteca compartilhada de HTTPS endpoints publicada no Artifactory para que BFF e peça habilitadora usem a mesma política de timeout, retry e tratamento de erro.

### Critérios de aceite
- [ ] Repositório criado e publicado no Artifactory
- [ ] Biblioteca com timeout, retry e tratamento de erro padronizados
- [ ] Versionamento semântico aplicado
- [ ] Consumida por pelo menos uma aplicação

### Definition of Ready
- [ ] Padrão de nomes definido (HU-01)
- [ ] Política de retry e timeout acordada com arquitetura
- [ ] Acesso de publicação no Artifactory liberado

### Definition of Done
- [ ] Versão estável publicada
- [ ] Testes unitários da biblioteca acima de 90%
- [ ] README com exemplo de uso

## HU-09 — Endpoints de fundos na peça habilitadora

**Épico:** B · Habilitação de dados · **Perfil:** Backend · **Estimativa de referência:** 8 pontos (16 h) · **Cards de origem:** BE-16

### Dor
A jornada não consegue apresentar os fundos disponíveis ao cliente porque não existe endpoint que exponha esses dados no formato esperado pela tela.

### História
> Como cliente da jornada transacional, quero visualizar os fundos disponíveis para que eu possa escolher onde aplicar sem sair do fluxo.

### Critérios de aceite
- [ ] Endpoints implementados conforme o contrato publicado
- [ ] Retorno validado contra o schema em ambiente de desenvolvimento
- [ ] Erros mapeados e retornados no padrão acordado
- [ ] Cobertura de testes unitários acima de 90%

### Definition of Ready
- [ ] Contrato publicado e aprovado (HU-02)
- [ ] Container da peça habilitadora no ar (HU-05)
- [ ] Massas de teste disponíveis (HU-16)

### Definition of Done
- [ ] Endpoints respondendo no ambiente de desenvolvimento
- [ ] Testes unitários e de contrato verdes no pipeline
- [ ] Documentação do endpoint atualizada

## HU-10 — Endpoints de termos na peça habilitadora

**Épico:** B · Habilitação de dados · **Perfil:** Backend · **Estimativa de referência:** 5 pontos (10 h) · **Cards de origem:** BE-17

### Dor
O aceite de termos é obrigatório na jornada e hoje não há endpoint que devolva o texto vigente nem registre o aceite do cliente.

### História
> Como cliente da jornada transacional, quero consultar e aceitar os termos vigentes para que minha decisão fique registrada de forma auditável.

### Critérios de aceite
- [ ] Endpoint de consulta devolve a versão vigente dos termos
- [ ] Endpoint de aceite registra cliente, versão e data e hora
- [ ] Erros mapeados e retornados no padrão acordado
- [ ] Cobertura de testes unitários acima de 90%

### Definition of Ready
- [ ] Contrato publicado e aprovado (HU-02)
- [ ] Regra de versionamento de termos confirmada com o negócio
- [ ] Container da peça habilitadora no ar (HU-05)

### Definition of Done
- [ ] Aceite persistido e consultável
- [ ] Testes unitários e de contrato verdes
- [ ] Evidência de auditoria validada com o time de negócios

## HU-11 — Endpoints de perfil na peça habilitadora

**Épico:** B · Habilitação de dados · **Perfil:** Backend · **Estimativa de referência:** 5 pontos (10 h) · **Cards de origem:** BE-18

### Dor
A jornada precisa adaptar o que mostra ao perfil do cliente, mas essa informação não está exposta em nenhum endpoint próprio.

### História
> Como cliente da jornada transacional, quero que a jornada reconheça meu perfil para que as opções apresentadas sejam compatíveis com ele.

### Critérios de aceite
- [ ] Endpoints implementados conforme o contrato publicado
- [ ] Regras de perfil validadas com o time de negócios
- [ ] Erros mapeados e retornados no padrão acordado
- [ ] Cobertura de testes unitários acima de 90%

### Definition of Ready
- [ ] Contrato publicado e aprovado (HU-02)
- [ ] Regras de elegibilidade por perfil confirmadas
- [ ] Container da peça habilitadora no ar (HU-05)

### Definition of Done
- [ ] Endpoints respondendo no ambiente de desenvolvimento
- [ ] Testes unitários e de contrato verdes
- [ ] Regras revisadas com o time de negócios

## HU-12 — BFF com endpoints complementares e cobertura de testes

**Épico:** B · Habilitação de dados · **Perfil:** Backend · **Estimativa de referência:** 8 pontos (16 h) · **Cards de origem:** BE-19, BE-20

### Dor
Nem tudo que a tela precisa vem da peça habilitadora. Sem uma camada de composição, o frontend faria várias chamadas e montaria regra de negócio no navegador.

### História
> Como time de frontend, quero um BFF que componha os dados da jornada em respostas prontas para a tela para que o navegador não precise orquestrar chamadas nem aplicar regra de negócio.

### Critérios de aceite
- [ ] Endpoints complementares implementados no BFF conforme contrato
- [ ] Composição das respostas validada contra a necessidade das telas
- [ ] Cobertura de testes unitários de no mínimo 90% no BFF e na peça
- [ ] Pipeline barra merge abaixo da cobertura mínima

### Definition of Ready
- [ ] Contrato publicado (HU-02)
- [ ] Container do BFF no ar (HU-05)
- [ ] Biblioteca de HTTPS endpoints disponível (HU-08)

### Definition of Done
- [ ] Cobertura mínima aplicada como gate no pipeline
- [ ] Endpoints do BFF respondendo em desenvolvimento
- [ ] Contrato do BFF versionado e publicado

## HU-13 — Primeira etapa da jornada transacional

**Épico:** C · Experiência da jornada · **Perfil:** Frontend · **Estimativa de referência:** 8 pontos (16 h) · **Cards de origem:** FE-03, FE-06

### Dor
A primeira tela é o ponto de entrada da jornada e hoje não existe. Sem ela não há como validar o fluxo com o negócio nem medir conversão.

### História
> Como cliente, quero iniciar a jornada transacional em uma tela clara e acessível para que eu consiga concluir a primeira etapa sem ajuda.

### Critérios de aceite
- [ ] Componente construído com o design system institucional (IDS)
- [ ] Conformidade WCAG validada: contraste, navegação por teclado e leitor de tela
- [ ] Integração com os endpoints de fundos e do BFF
- [ ] Estados de carregamento, vazio e erro implementados
- [ ] Responsivo nos breakpoints definidos

### Definition of Ready
- [ ] Layout aprovado pelo time de design
- [ ] Endpoints virtualizados disponíveis (HU-16)
- [ ] Microfrontend publicado (HU-07)

### Definition of Done
- [ ] Componente aprovado em revisão de IDS e de acessibilidade
- [ ] Integração com endpoints reais validada
- [ ] Testes de componente no pipeline

## HU-14 — Modal de aceite de termos

**Épico:** C · Experiência da jornada · **Perfil:** Frontend · **Estimativa de referência:** 5 pontos (10 h) · **Cards de origem:** FE-04, FE-07

### Dor
Sem interface de aceite, o cliente não consegue avançar na jornada e o registro exigido pelo negócio não acontece.

### História
> Como cliente, quero ler e aceitar os termos em um modal acessível para que eu possa prosseguir com segurança sobre o que estou aceitando.

### Critérios de aceite
- [ ] Modal construído com o design system institucional (IDS)
- [ ] Conformidade WCAG validada, incluindo foco preso ao modal e fechamento por teclado
- [ ] Integração com os endpoints de consulta e de aceite de termos
- [ ] Aceite bloqueado até a rolagem completa do texto, se o negócio exigir
- [ ] Estados de erro e nova tentativa implementados

### Definition of Ready
- [ ] Layout do modal aprovado pelo time de design
- [ ] Regra de obrigatoriedade de leitura confirmada com o negócio
- [ ] Endpoints de termos virtualizados (HU-16)

### Definition of Done
- [ ] Modal aprovado em revisão de IDS e de acessibilidade
- [ ] Aceite gravado e verificado ponta a ponta
- [ ] Testes de componente no pipeline

## HU-15 — Tagueamento da jornada

**Épico:** C · Experiência da jornada · **Perfil:** Frontend · **Estimativa de referência:** 3 pontos (6 h) · **Cards de origem:** FE-05

### Dor
Sem tagueamento a jornada entra em produção cega: não há como medir onde o cliente abandona nem justificar melhorias com dado.

### História
> Como time de negócios, quero a jornada instrumentada com eventos de analytics para que eu consiga medir conversão e identificar pontos de abandono.

### Critérios de aceite
- [ ] Eventos implementados conforme a techspec do time de design
- [ ] Disparo validado na ferramenta de analytics em ambiente de desenvolvimento
- [ ] Cobertura dos eventos de entrada, aceite de termos e conclusão
- [ ] Nenhum dado sensível trafegando nos eventos

### Definition of Ready
- [ ] Techspec de tagueamento entregue pelo time de design
- [ ] Componentes da jornada implementados (HU-13, HU-14)
- [ ] Acesso à ferramenta de analytics liberado

### Definition of Done
- [ ] Eventos visíveis no painel de analytics
- [ ] Validação conjunta com o time de dados
- [ ] Documentação dos eventos publicada

## HU-20 — Telemetria e padronização de logs

**Épico:** E · Observabilidade · **Perfil:** SRE + Backend · **Estimativa de referência:** 5 pontos (10 h) · **Cards de origem:** OB-01, OB-02

### Dor
Sem telemetria, qualquer incidente vira investigação manual em logs sem formato comum, e o tempo de diagnóstico se estende.

### História
> Como time de sustentação, quero métricas, traces e logs padronizados chegando ao Datadog para que o diagnóstico de incidentes não dependa de acesso manual às máquinas.

### Critérios de aceite
- [ ] Datadog integrado ao BFF e à peça habilitadora
- [ ] Traces distribuídos cobrindo a chamada ponta a ponta
- [ ] Logs em formato estruturado, com correlação por identificador de requisição
- [ ] Nenhum dado sensível registrado em log

### Definition of Ready
- [ ] Containers no ar (HU-05)
- [ ] Padrão de log definido com arquitetura
- [ ] Licença e acesso ao Datadog liberados

### Definition of Done
- [ ] Métricas e traces visíveis no Datadog
- [ ] Busca por identificador de requisição funcionando
- [ ] Revisão de dados sensíveis aprovada

---

# Onda 3 · Integração e entrega (semanas 7–9)

## HU-17 — Cenários e automação de testes (TAAC)

**Épico:** D · Qualidade e integração · **Perfil:** QA · **Estimativa de referência:** 8 pontos (16 h) · **Cards de origem:** QA-02, QA-05

### Dor
Sem suíte automatizada, cada entrega exige regressão manual, o que encarece as ondas seguintes e atrasa a liberação.

### História
> Como squad, quero os cenários mapeados e automatizados em TAAC para que cada entrega seja validada pelo pipeline sem regressão manual.

### Critérios de aceite
- [ ] Cenários mapeados e priorizados por criticidade
- [ ] Suíte TAAC cobrindo o fluxo principal da jornada
- [ ] Execução automática no pipeline a cada merge
- [ ] Relatório de execução acessível ao squad

### Definition of Ready
- [ ] Jornada funcional ponta a ponta (HU-13, HU-14)
- [ ] Integração concluída (HU-18)
- [ ] Ambiente de execução da suíte disponível

### Definition of Done
- [ ] Suíte verde em três execuções consecutivas
- [ ] Tempo de execução dentro do limite acordado
- [ ] Cenários revisados com o time de negócios

## HU-18 — Integração com a Peça Centralizadora

**Épico:** D · Qualidade e integração · **Perfil:** Backend + QA · **Estimativa de referência:** 8 pontos (16 h) · **Cards de origem:** BE-21, QA-04

### Dor
Até esta história a jornada roda sobre dados virtualizados. Sem a integração real, nada do que foi construído entrega valor ao cliente.

### História
> Como cliente, quero que a jornada opere sobre os dados reais da Peça Centralizadora para que a transação que eu realizar tenha efeito de verdade.

### Critérios de aceite
- [ ] BFF integrado à Peça Centralizadora em ambiente de desenvolvimento
- [ ] Cenários de sucesso, erro e indisponibilidade validados pelo QA
- [ ] Timeout, retry e circuit breaker configurados
- [ ] Nenhuma dependência de virtualização no caminho principal

### Definition of Ready
- [ ] Endpoints do BFF concluídos (HU-12)
- [ ] Acesso à Peça Centralizadora liberado nos ambientes
- [ ] Contrato de integração confirmado com o time responsável

### Definition of Done
- [ ] Fluxo completo executado com dados reais
- [ ] Cenários de falha validados e documentados
- [ ] Aprovação formal do QA registrada

## HU-19 — Pacote de segurança e pentest

**Épico:** D · Qualidade e integração · **Perfil:** QA · **Estimativa de referência:** 3 pontos (6 h) · **Cards de origem:** QA-06

### Dor
A jornada movimenta dados sensíveis e não pode ir a produção sem avaliação de segurança. A fila do time de segurança costuma ser longa.

### História
> Como responsável pela jornada, quero o pacote de pentest preparado e a tarefa aberta com antecedência para que a avaliação de segurança não bloqueie a liberação.

### Critérios de aceite
- [ ] Pacote com escopo, arquitetura, endpoints e credenciais de teste
- [ ] Tarefa aberta com o time de segurança e data reservada
- [ ] Ambiente de teste disponível para a equipe de segurança
- [ ] Plano de tratamento dos achados acordado

### Definition of Ready
- [ ] Jornada integrada e estável (HU-18)
- [ ] Documentação de arquitetura atualizada
- [ ] Contato do time de segurança identificado

### Definition of Done
- [ ] Pentest agendado com data confirmada
- [ ] Ambiente validado pela equipe de segurança
- [ ] Responsável pelo tratamento dos achados definido

## HU-21 — Dashboards e alarmes operacionais

**Épico:** E · Observabilidade · **Perfil:** SRE / Infra · **Estimativa de referência:** 5 pontos (10 h) · **Cards de origem:** OB-03, OB-04

### Dor
Telemetria sem painel e sem alarme não é observabilidade: a falha continua sendo descoberta pelo cliente antes do time.

### História
> Como time de sustentação, quero dashboards e alarmes configurados para que uma degradação seja detectada pelo time antes de virar reclamação do cliente.

### Critérios de aceite
- [ ] Dashboard com latência, taxa de erro, throughput e saúde dos containers
- [ ] Alarmes com limiares acordados e destinatários definidos
- [ ] Alarme testado com disparo controlado
- [ ] Runbook associado a cada alarme

### Definition of Ready
- [ ] Telemetria chegando ao Datadog (HU-20)
- [ ] Limiares de SLO acordados com o negócio
- [ ] Canal de notificação definido

### Definition of Done
- [ ] Dashboard revisado com o squad e a sustentação
- [ ] Alarmes disparando para o canal correto
- [ ] Runbooks publicados

---

## Definition of Ready e Definition of Done gerais

Aplicam-se a toda história, além dos critérios específicos listados acima.

### DoR geral
- História escrita no formato "como / quero / para que", com dor explícita
- Critérios de aceite acordados entre produto, desenvolvimento e QA
- Dependências identificadas e desbloqueadas
- Estimativa votada em Planning Poker
- Sem dependência externa pendente no momento de entrar na onda

### DoD geral
- Código revisado por par e integrado à branch principal
- Testes unitários acima de 90% de cobertura e pipeline verde
- Critérios de aceite verificados pelo QA
- Documentação e contrato atualizados
- Telemetria e log da funcionalidade disponíveis
- Demonstração feita ao time de negócios
