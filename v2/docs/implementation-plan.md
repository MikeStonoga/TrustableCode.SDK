# TrustableCode.SDK v2 - Plano Vivo

## Objetivo

Construir a v2 do SDK como uma ponte direta entre o livro *Trustable Code* e o trabalho real de desenvolvedores e agentes de IA.

A v2 deve ajudar uma pessoa ou agente a ler uma area critica do sistema por:

- estado explicito
- transicoes governadas
- invariantes preservados
- fronteiras que rejeitam significado invalido
- efeitos colaterais controlados
- evidencia observavel
- contexto semantico suficiente para mudancas assistidas por IA

## Status Geral

| Etapa | Status | Resultado esperado |
| --- | --- | --- |
| 1. Preservar v1 | Concluido | Conteudo atual movido para `v1/`. |
| 2. Criar estrutura v2 | Concluido | Solution, projeto core, testes e docs iniciais em `v2/`. |
| 3. Trustable Model Descriptor | Parcial | API inicial implementada e validada em um primeiro sample `Ordering`. Ainda falta validar ergonomia com comportamento executavel. |
| 4. Transicoes governadas | Parcial | `GovernedTransition` executa pre-condicoes/invariantes, aplica estado, retorna resultado, declara eventos/evidencias e suporta politica basica de repeticao. Transicoes de dominio devem ser classes especializadas que usam `GovernedTransition` internamente. |
| 5. Invariantes fortes | Parcial | Invariantes com codigo estavel, severidade, descriptor e regra executavel implementados. Ainda faltam sugestoes de teste e evidencia estruturada de violacao. |
| 6. Fronteiras e admissao | Parcial | `BusinessAdmission` aceita/rejeita input externo antes de converter para intencao de negocio. Ainda falta evidencia estruturada de rejeicao e integracao com observabilidade. |
| 7. Efeitos colaterais e idempotencia | Pendente | Contratos para efeitos planejados, persistidos, publicados, confirmados e compensados. |
| 8. Observabilidade como evidencia | Pendente | Sinks e contratos orientados a evidencia de negocio, nao ruido tecnico. |
| 9. Pacote de contexto para agentes | Parcial | `AgentContextPacket` gera markdown inicial para agentes e revisores. Ainda falta template completo por area critica e integracao com samples. |
| 10. Samples alinhados ao livro | Parcial | Primeiro sample semantico `Ordering` criado. Ainda faltam exemplos por apendice: unsafe, trustable manual e trustable usando SDK. |
| 11. Testes para confianca | Pendente | Helpers para testar invariantes, transicoes, fronteiras, idempotencia e evidencia. |
| 12. Packaging e publicacao | Pendente | NuGet metadata, README de pacote e pipeline de release. |

## Decisoes Iniciais

- `v1/` deve permanecer como snapshot preservado da implementacao atual.
- `v2/` deve evoluir sem carregar todas as decisoes da v1 automaticamente.
- O primeiro pacote da v2 se chama `TrustableCode.SDK.TrustableModeling`.
- O primeiro artefato conceitual da v2 e o `TrustableModelDescriptor`.
- O SDK deve produzir contexto legivel para humanos e agentes, nao apenas tipos de runtime.

## Proxima Etapa

Evoluir evidencia estruturada para violacoes de invariantes e rejeicoes de fronteira.

Depois disso, iniciar efeitos colaterais governados e idempotencia como primitives proprias, conectadas aos resultados de transicao.

## Implementado Nesta Iteracao

- Repositorio reorganizado em `v1/` e `v2/`.
- Conteudo atual do SDK preservado em `v1/`.
- Solution inicial criada em `v2/TrustableCode.SDK.v2.sln`.
- Pacote inicial criado em `v2/src/TrustableCode.SDK.TrustableModeling`.
- Testes iniciais criados em `v2/tests/TrustableCode.SDK.TrustableModeling.Tests`.
- `TrustableModelDescriptor` criado como envelope semantico do modelo.
- Descritores iniciais criados para estado, transicao, invariante, fronteira, efeito colateral e evidencia.
- `AgentContextPacket` criado para gerar markdown de contexto para agentes e revisores.
- Testes confirmam criacao do descriptor e geracao de contexto.

## Implementado Na Iteracao Seguinte

- Projeto `v2/samples/TrustableCode.SDK.Samples.Ordering` criado.
- Sample `OrderFulfillmentTrustableModel` criado com estados, transicoes, invariantes, fronteiras, efeitos colaterais, evidencias e non-goals.
- Testes passaram a consumir o descriptor do sample real.
- `dotnet test TrustableCode.SDK.v2.sln` validou a solution da v2 com o novo sample.

## Implementado Na Iteracao De Transicoes Governadas

- `GovernedTransition<TState, TContext>` criado.
- `TransitionPrecondition<TState, TContext>` criado para nomear regras antes da mudanca de estado.
- `TransitionExecutionResult<TState>` criado para relatar estado anterior, estado atual, status, rejeicoes, eventos e evidencias declaradas.
- `TransitionRepetitionPolicy` criado com suporte inicial a idempotencia por estado alvo ja aplicado.
- Sample `Ordering` ganhou `Order`, `OrderStatus` e `PrepareOrderForShippingRequirement`.
- `Order.PrepareForShipping` passou a executar a transicao por `GovernedTransition`.
- Testes validam transicao aplicada, rejeicao por pre-condicoes e repeticao idempotente.

## Implementado Na Iteracao De Invariantes E Admissao

- `BusinessInvariant<TContext>` criado para conectar `InvariantDescriptor` a regra executavel.
- `InvariantSet<TContext>` criado para avaliar conjuntos de invariantes e retornar apenas violacoes.
- `TransitionContext<TState, TInput>` criado para expor estado atual, estado alvo e input durante avaliacao da transicao.
- `GovernedTransition` passou a aceitar invariantes executaveis alem de pre-condicoes.
- `BusinessAdmission<TInput, TAccepted>` criado para converter input externo em intencao admitida somente depois das regras de fronteira.
- `AdmissionRule<TInput>` e `AdmissionResult<TAccepted>` criados.
- Sample `Ordering` ganhou `PrepareOrderForShippingAdmission`, `ExternalPrepareOrderForShippingRequest` e `OrderFulfillmentInvariants`.
- `PrepareOrderForShippingTransition` criado como classe especializada de dominio, recebendo `Order` no construtor.
- `Order.PrepareForShipping` passou a chamar `new PrepareOrderForShippingTransition(this)`.
- Testes validam admissao aceita, rejeicao por status arbitrario, rejeicao sem correlacao e violacoes de invariantes.
