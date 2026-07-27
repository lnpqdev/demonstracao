# Prompt para GitHub Copilot (Claude Sonnet)

> Cole o conteúdo abaixo no chat do Copilot (modo Agent, de preferência) com o repositório aberto e o arquivo `pre-contratacao-fundos.md` presente no workspace. Ajuste os itens entre `[colchetes]` antes de enviar.

---

Você vai implementar um novo serviço a partir da documentação técnica deste repositório. Siga as etapas na ordem e não pule a fase de leitura.

## Etapa 1 — Leitura obrigatória

Antes de escrever qualquer código, leia integralmente:

1. `docs/pre-contratacao-fundos.md` (desenho técnico v0.2 do SVPT) — este é o documento fonte de verdade.
2. Todos os demais arquivos de documentação do repositório (`docs/`, `README.md`, ADRs, contratos OpenAPI existentes) que ajudem a entender convenções de projeto, stack e padrões de código já adotados.

Depois de ler, **resuma em poucas linhas o que você entendeu do escopo do SVPT e liste suas premissas** antes de começar a implementar. Se algo no documento conflitar com o que existe no repositório, pergunte antes de decidir sozinho.

## Etapa 2 — O que construir

Implemente o **SVPT (Serviço de Validação de Perfil e Termos)** exatamente com o escopo estrito da v0.2:

**Endpoints (contrato da seção 08 do documento):**

| Método | Rota | Função |
|---|---|---|
| GET | `/v1/validacao-perfil-termos/clientes/{id}/status` | Status consolidado: perfil (API 4), Receita Federal (gap), termos pendentes (API 3) |
| GET | `/v1/validacao-perfil-termos/clientes/{id}/termos` | Termos pendentes por tipo (TCQ, TDI, risco, desenquadramento) |
| POST | `/v1/validacao-perfil-termos/clientes/{id}/termos/{tipo}/assinar` | Aciona assinatura, delegando persistência à API 3 |
| GET | `/v1/validacao-perfil-termos/clientes/{id}/perfil` | Espelho direto do status de perfil da API 4 |

**Formato de resposta do status:** siga o rascunho JSON da seção 08 do documento (campos `clienteId`, `perfil`, `receitaFederal`, `termos[]`, cada um com `status` e `origem`).

**Integrações:**
- API 4 (Suitability/Enquadramento): fonte única de verdade do perfil — o SVPT consulta e repassa, nunca recalcula.
- API 3 (Termos): listagem de termos assinados/pendentes e persistência da assinatura.
- Receita Federal: **não existe API hoje**. Implemente o campo retornando sempre `{ "status": "NAO_DISPONIVEL", "motivo": "sem integração ainda" }` — nunca omita o campo. Isole essa lógica atrás de uma interface/porta para que a integração futura seja plugável sem mudar o contrato.
- Como não há contrato real das APIs 3 e 4 no documento, crie clients atrás de interfaces com implementações mock/stub claramente marcadas com `TODO`, e documente as premissas de contrato que você assumiu.

## Etapa 3 — Regras invioláveis (regra de ouro do documento)

1. **O serviço é informativo, nunca bloqueante.** Nenhuma resposta pode conter campo `bloquear`, `pode_prosseguir` ou equivalente. Só status de fato: `PENDENTE`, `OK`, `NAO_ASSINADO`, `ASSINADO`, `NAO_APLICAVEL`, `NAO_DISPONIVEL`, `IGNORADO`. Quem decide UX/bloqueio é sempre a Vitrine ou o Transacional.
2. **Escopo fechado.** Não implemente nada de: catálogo/benchmark (API 7/1), saldo/posições (API 6/1), elegibilidade CVM50/entes públicos (API 5), simulação/efetivação (API 1/2), máquina de estados de contratação. Se perceber que "faria sentido" adicionar algo dessas áreas, não adicione — isso foi exatamente o erro de escopo da v0.1 que a v0.2 corrige.
3. **Assinatura idempotente.** O POST de assinatura deve ser idempotente: assinar um termo já assinado não pode duplicar o registro nem retornar erro 5xx (retorne o estado atual). Documento cita esse risco na seção 11, item 6.
4. **Enums estáveis.** Nomes de campos e valores de enum do contrato devem ser centralizados (constantes/enums tipados), pois Vitrine e Transacional dependem deles para decidir UX de forma independente.

## Etapa 4 — Stack e padrões

- Stack: `[preencha: ex. Java 21 + Spring Boot / Node + NestJS / .NET 8 — ou escreva "siga a stack predominante do repositório"]`
- Siga a estrutura de pastas, convenções de nome e padrões de camadas já existentes no repositório (controller/handler → service → client/gateway).
- Gere a especificação **OpenAPI 3** dos 4 endpoints.
- Escreva **testes unitários** cobrindo: consolidação de status, campo Receita Federal sempre presente como `NAO_DISPONIVEL`, idempotência da assinatura, e ausência de qualquer campo de bloqueio na resposta.
- Adicione um `README` do serviço explicando o escopo estrito, o gap da Receita Federal e as pendências de validação (seção 11 do documento) como TODOs rastreáveis.

## Etapa 5 — Entrega

Ao final, apresente:
1. A lista de arquivos criados/alterados com um resumo de cada um.
2. As premissas assumidas sobre os contratos das APIs 3 e 4 (que precisam ser validadas com os times donos).
3. Os pontos da seção 11 do documento que continuam pendentes de decisão humana e onde eles estão marcados no código.

Não invente integrações reais para a Receita Federal, não crie campos de decisão de navegação, e não expanda o escopo além dos 4 endpoints.
