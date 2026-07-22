# Prompts — Levantamento "As Is" de Validações na Contratação de Investimentos

> **Objetivo:** analisar os projetos existentes e extrair o "As Is" das validações
> executadas na contratação de investimentos, para subsidiar o desenho de uma nova
> peça centralizadora de validações.
> **Produto desta versão:** FUNDOS (versão 1).
> **Ferramentas-alvo:** Devin e/ou Copilot (corpo central único + bloco de ajuste por ferramenta).

---

## Prompt central (use em qualquer ferramenta)

```markdown
# PROMPT — Levantamento "As Is" de Validações na Contratação de Investimentos
# PRODUTO: FUNDOS (versão 1)

## 1. PAPEL
Você é um analista de arquitetura de software. Sua tarefa é ler os repositórios/
projetos indicados e produzir uma DOCUMENTAÇÃO "AS IS" (estado atual, sem propor
melhorias) das validações executadas na contratação de investimentos em FUNDOS.
Esta documentação será usada para desenhar uma nova peça centralizadora de
validações, portanto precisa ser precisa, rastreável e fiel ao código atual.

## 2. ESCOPO
- Analise SOMENTE o fluxo de contratação/aplicação em FUNDOS.
- Ignore outros produtos (renda fixa, previdência, COE, ações etc.), a menos que
  compartilhem código com o fluxo de fundos — nesse caso, apenas cite o ponto de
  compartilhamento.
- Foque no "As Is": documente o que EXISTE hoje, não o que deveria existir.

## 3. REGRAS DE EXECUÇÃO (obrigatórias)
- NÃO invente. Toda afirmação sobre uma validação, sistema ou regra deve apontar
  a evidência: `caminho/do/arquivo.ext:linha` (ou nome da classe/método/função).
- Se algo não puder ser confirmado no código, marque como `⚠️ NÃO CONFIRMADO`
  e explique o que faltou para confirmar.
- Não confie apenas em nomes de variáveis/métodos: confirme lendo a lógica.
- Diferencie validação SÍNCRONA (bloqueia a contratação) de ASSÍNCRONA/informativa.
- Preserve os nomes reais de sistemas, capacidades, endpoints e campos como
  aparecem no código (não traduza nem padronize).

## 4. METODOLOGIA (siga nesta ordem — end-to-end)

### Etapa 0 — Mapa dos projetos
Liste os repositórios/módulos envolvidos no fluxo de fundos, o papel de cada um
(orquestrador, BFF, microsserviço de validação, gateway, etc.) e como se comunicam
(REST, mensageria, gRPC). Produza um diagrama textual do fluxo de chamadas.

### Etapa 1 — Jornada de contratação (as etapas)
Identifique e ordene as ETAPAS da contratação de um fundo, do início ao fim
(ex.: seleção do produto → elegibilidade do cliente → suitability/perfil →
validações regulatórias → aceite de termos → montagem do payload → envio/boletagem
→ confirmação). Use os nomes reais das etapas encontradas no código.

### Etapa 2 — Sistemas e capacidades por etapa
Para CADA etapa, liste os sistemas/serviços/capacidades consultados, o que cada
chamada verifica ou retorna, e se é bloqueante.

### Etapa 3 — Catálogo de validações
Classifique cada validação encontrada em uma destas categorias:
- **Cliente** (elegibilidade, situação, restrições)
- **Cadastro** (dados cadastrais completos/válidos)
- **Perfil** (suitability, adequação perfil x produto)
- **CVM 50** (adequação regulatória / investidor)
- **Termos** (aceites, assinaturas, ciências)
- **Produto (Fundo)** — validações específicas do fundo (horário/janela, valor
  mínimo, saldo, público-alvo, restrição de distribuição, carência, etc.)

### Etapa 4 — Centralizado vs Distribuído
Para cada responsabilidade/validação, indique se HOJE ela está CENTRALIZADA
(em um único serviço/componente reutilizável) ou DISTRIBUÍDA (replicada/espalhada
entre projetos). Aponte duplicidades e divergências de regra entre projetos.

### Etapa 5 — Mapeamento de dados → payload + regras
Para o payload final da contratação (e para os payloads intermediários relevantes),
mapeie cada CAMPO: nome no payload, origem do dado (sistema/tela/campo de origem),
transformação aplicada e regra de negócio associada (obrigatoriedade, formato,
domínio de valores, condições).

## 5. FORMATO DE SAÍDA
Entregue um único documento Markdown com esta estrutura:

1. **Resumo executivo** (5–10 linhas)
2. **Mapa dos projetos** (papéis + diagrama de fluxo textual)
3. **Jornada end-to-end** (etapas numeradas)
4. **Sistemas e capacidades por etapa** — tabela:
   | Etapa | Sistema/Capacidade | O que valida/retorna | Síncrono? | Bloqueante? | Evidência |
5. **Catálogo de validações** — uma tabela por categoria (Cliente, Cadastro,
   Perfil, CVM50, Termos, Produto):
   | Validação | Descrição | Regra/Condição | Projeto(s) | Síncrono? | Centralizada/Distribuída | Evidência |
6. **Centralizado vs Distribuído** — tabela consolidada + lista de duplicidades e
   divergências encontradas.
7. **Mapeamento de dados → payload** — tabela:
   | Campo no payload | Origem (sistema/campo) | Transformação | Regra de negócio | Obrigatório? | Evidência |
8. **Lacunas e pontos NÃO CONFIRMADOS** (lista).
9. **Glossário** (siglas e capacidades citadas).

## 6. CRITÉRIOS DE CONCLUSÃO (Definition of Done)
- Todas as etapas do fluxo de fundos mapeadas do início ao fim.
- Toda validação classificada em uma das 6 categorias e marcada como
  centralizada ou distribuída.
- Todo campo do payload final rastreado até sua origem.
- Toda afirmação com evidência (arquivo:linha) ou marcada como NÃO CONFIRMADO.

## 7. ENTRADAS QUE VOU FORNECER
- Repositórios/branches: <PREENCHER>
- Ponto de entrada do fluxo de fundos (endpoint/serviço/tela): <PREENCHER>
- Ambiente/documentação de apoio (se houver): <PREENCHER>
```

---

## Bloco de ajuste — Devin

> Cole junto com o prompt central.

```markdown
# MODO DE TRABALHO (Devin)
- Explore os repositórios de forma autônoma; comece pelo ponto de entrada do fluxo
  de fundos e siga as chamadas (call graph) até o envio do payload.
- Trabalhe de forma incremental: entregue primeiro o Mapa de projetos (Etapa 0) e a
  Jornada (Etapa 1) para eu validar, depois avance para as demais etapas.
- Salve o resultado como `docs/asis-validacoes-fundos.md` no repositório.
- Não altere código de produção; esta é uma tarefa somente de análise/documentação.
```

---

## Bloco de ajuste — Copilot (Chat / coding agent)

> Cole junto com o prompt central.

```markdown
# MODO DE TRABALHO (Copilot)
- Use o workspace/contexto aberto. Se algum repositório não estiver no contexto,
  liste o que precisa e pare para eu anexar antes de prosseguir (não presuma).
- Responda uma SEÇÃO por vez, na ordem da metodologia, sempre com os trechos de
  código (com caminho e linha) que sustentam cada afirmação.
- Ao final, consolide tudo em um único Markdown no formato da seção 5.
```

---

## Notas de uso

- **Devin** é autônomo e aguenta o levantamento inteiro de uma vez; **Copilot**
  rende mais se você o fizer entregar **seção por seção**. O corpo central é
  idêntico — mantenha uma fonte única e troque apenas o bloco de ajuste final.
- Para replicar em outros produtos (renda fixa, previdência, COE, ações), duplique
  o prompt central, troque o cabeçalho `PRODUTO`, o **Escopo (seção 2)** e as
  validações específicas da categoria **"Produto"** na Etapa 3.
- Rode primeiro o de fundos e ajuste este template com base no resultado antes de
  gerar as variantes dos demais produtos.
```
