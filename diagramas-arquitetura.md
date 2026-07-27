# Diagramas de arquitetura — SVPT (Serviço de Validação de Perfil e Termos)

Extraído de `pre-contratacao-fundos.md` (v0.2), para uso isolado.

## Arquitetura proposta

```mermaid
flowchart LR
  subgraph VitrineDominio["Domínio Vitrine"]
    FEV["Frontend Vitrine"] --> BFFV["BFF Vitrine"]
    BFFV --> A7["API 7 · Catálogo"]
    BFFV --> A1a["API 1 · Fundos/Posições"]
    BFFV --> A6["API 6 · Saldo"]
  end

  subgraph TransDominio["Domínio Transacional"]
    FET["Frontend Transacional"] --> BFFT["BFF Transacional"]
    BFFT --> A2["API 2 · Contratação"]
    A2 --> A1b["API 1 · Simulação/Movimentos"]
  end

  subgraph SVPTBox["Serviço lateral (novo escopo)"]
    SVPT["SVPT · Validação de\nPerfil e Termos"]
  end

  BFFV -. "consulta pendência\n(somente leitura)" .-> SVPT
  BFFT -. "consulta pendência +\naciona assinatura" .-> SVPT

  SVPT --> A4["API 4 · Suitability/Enquadramento"]
  SVPT --> A3["API 3 · Termos/Suitability"]
  SVPT -. "gap: sem API hoje" .-> RF["Receita Federal /\nPendência cadastral"]
```

## Fluxograma de decisão

```mermaid
flowchart TD
  Start(["Cliente acessa a vitrine"]) --> Consulta["Vitrine consulta SVPT:\nperfil / RF / termos pendentes"]
  Consulta --> ChkPerfil{"Vitrine decide:\npossui perfil OU termo?"}
  ChkPerfil -- "Sim" --> VitrinePlena["Vitrine plena, sem banner"]
  ChkPerfil -- "Não" --> VitrineBanner["Vitrine + banner de pendência\n(Vitrine decide não bloquear seleção)"]

  VitrinePlena --> Selecao["Cliente seleciona produto\n(regra do catálogo, fora do SVPT)"]
  VitrineBanner --> Selecao

  Selecao --> FluxoTrans["Transacional inicia\n(API 2)"]
  FluxoTrans --> ConsultaTermos["Transacional consulta SVPT:\ntermos pendentes para este cliente/produto"]
  ConsultaTermos --> ChkStep{"Transacional decide:\napresenta step de\nperfil/termo?"}
  ChkStep -- "Sim" --> StepEnriquecimento["Step de enriquecimento:\nPreencher perfil · Assinar TCQ ·\nAssinar TDI · Seguir sem perfil\n(ações executadas via SVPT)"]
  ChkStep -- "Não / cliente já ok" --> Simulacao
  StepEnriquecimento --> Simulacao["Simulação do produto\n(domínio Transacional)"]

  Simulacao --> ChkDesenquadre["Transacional avalia\nresultado de enquadramento"]
  ChkDesenquadre --> ConsultaTermoDes["Transacional consulta SVPT:\nhá termo de ciência de\ndesenquadramento pendente?"]
  ConsultaTermoDes --> ChkExige{"Transacional decide:\nexige assinatura?"}
  ChkExige -- "Sim" --> AssinaDes["Assina via SVPT"]
  AssinaDes --> Conclui["Conclui contratação\n(API 2)"]
  ChkExige -- "Não" --> Conclui
```

## Sequência entre serviços

```mermaid
sequenceDiagram
  participant FE as Frontend Vitrine
  participant BFFV as BFF Vitrine
  participant SVPT as SVPT
  participant A4 as API 4 (Suitability)
  participant A3 as API 3 (Termos)
  participant BFFT as BFF Transacional
  participant A2 as API 2 (Contratação)

  FE->>BFFV: carregar vitrine
  BFFV->>SVPT: status de perfil/RF/termos do cliente
  SVPT->>A4: perfil de investidor
  SVPT->>A3: termos assinados
  SVPT-->>BFFV: PERFIL=PENDENTE, TERMOS=[TCQ: NAO_ASSINADO]
  BFFV-->>FE: vitrine (decide exibir banner)

  FE->>BFFT: inicia contratação do produto X
  BFFT->>A2: cria/consulta contratação
  BFFT->>SVPT: termos pendentes para o cliente
  SVPT-->>BFFT: TCQ pendente
  BFFT-->>FE: exibir step de assinatura (decisão do Transacional)
  FE->>SVPT: assinar termo TCQ
  SVPT->>A3: persistir assinatura
  A3-->>SVPT: ok
  SVPT-->>FE: TERMOS=[TCQ: ASSINADO]

  BFFT->>A2: segue simulação (fora do SVPT)
  BFFT->>SVPT: há termo de desenquadramento pendente?
  SVPT->>A4: consulta enquadramento
  SVPT-->>BFFT: desenquadrado, termo NAO_ASSINADO
  BFFT-->>FE: exigir assinatura (decisão do Transacional)
  FE->>SVPT: assinar termo de desenquadramento
  SVPT->>A3: persistir assinatura
  BFFT->>A2: efetivar contratação (2FA)
```
