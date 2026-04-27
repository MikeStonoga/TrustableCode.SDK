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
| 4. Transicoes governadas | Parcial | `GovernedTransition` executa pre-condicoes/invariantes, aplica estado, retorna resultado, declara eventos/evidencias e suporta politica basica de repeticao. Transicoes e pre-condicoes de dominio devem ser classes especializadas que usam primitives genericos internamente. |
| 5. Invariantes fortes | Parcial | `BusinessInvariantRule` e invariantes com codigo estavel, severidade, descriptor, regra executavel e evidencia estruturada de violacao. `TransitionPrecondition` herda dessa base para unificar o modelo conceitual. Ainda faltam mais exemplos de integracao com observabilidade. |
| 6. Fronteiras e admissao | Parcial | `BusinessAdmission` aceita/rejeita input externo antes de converter para intencao de negocio e emite evidencia estruturada de rejeicao. Sample `Ordering` cobre fronteiras de criacao, pagamento, preparacao, envio, entrega e cancelamento. Ainda falta helper dedicado para testes de admissao. |
| 7. Efeitos colaterais e idempotencia | Parcial | `GovernedSideEffect` executa efeitos com chave de idempotencia e evidencia estruturada. `GovernedSideEffectLifecycle` diferencia efeitos planejados, persistidos, publicados, confirmados e compensados. Ainda falta integrar outbox/worker real. |
| 8. Observabilidade como evidencia | Parcial | Sinks, recorder, adapter `ActivitySource`, adapter `ILogger` e convencoes de campos documentadas. Ainda falta consolidar com cenarios distribuidos. |
| 9. Pacote de contexto para agentes | Parcial | `AgentContextPacket` gera markdown inicial para agentes e revisores. Ainda falta template completo por area critica e integracao com samples. |
| 10. Samples alinhados ao livro | Parcial | Sample `Ordering` agora cobre criacao via factory e fluxo principal do pedido ate entrega/cancelamento. Ainda faltam exemplos por apendice: unsafe, trustable manual e trustable usando SDK. |
| 11. Testes para confianca | Parcial | `TrustableChecks` fornece checks neutros de framework para transicoes, admissoes, invariantes, side effects e evidencia. Ainda faltam builders de cenarios e exemplos de uso em docs. |
| 12. Packaging e publicacao | Pendente | NuGet metadata, README de pacote e pipeline de release. |

## Decisoes Iniciais

- `v1/` deve permanecer como snapshot preservado da implementacao atual.
- `v2/` deve evoluir sem carregar todas as decisoes da v1 automaticamente.
- O primeiro pacote da v2 se chama `TrustableCode.SDK.TrustableModeling`.
- O primeiro artefato conceitual da v2 e o `TrustableModelDescriptor`.
- O SDK deve produzir contexto legivel para humanos e agentes, nao apenas tipos de runtime.

## Proxima Etapa

Evoluir `AgentContextPacket` para renderizar um pacote mais completo por area critica, incluindo fluxos felizes, rejeicoes esperadas e pontos de observabilidade.

Depois disso, documentar os helpers de teste com exemplos curtos baseados no sample `Ordering`.

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

## Implementado Na Iteracao De Evidencia E Side Effects

- `BusinessEvidence` criado como evidencia estruturada comum da v2.
- `InvariantEvaluation` passou a gerar evidencia estruturada de violacao.
- `AdmissionResult` passou a carregar evidencia estruturada de rejeicao.
- `BusinessAdmission` passou a emitir evidencia para regras de fronteira rejeitadas.
- `TransitionExecutionResult` passou a carregar evidencia estruturada de rejeicao.
- `GovernedTransition` passou a emitir evidencia para rejeicoes por estado invalido e invariantes violados.
- `GovernedSideEffect<TContext>` criado para executar efeitos externos com idempotencia e evidencia.
- `IIdempotencyLedger` e `InMemoryIdempotencyLedger` criados.
- Sample `Ordering` ganhou `NotifyFulfillmentSideEffect` e `FulfillmentNotification`.
- Testes validam evidencia estruturada de admissao, invariantes, transicoes rejeitadas e side effects idempotentes.

## Implementado Na Iteracao De Observabilidade

- `IBusinessEvidenceSink` criado como destino de evidencia estruturada.
- `InMemoryBusinessEvidenceSink` criado para testes, samples e diagnostico local.
- `CompositeBusinessEvidenceSink` criado para distribuir evidencia para multiplos destinos.
- `BusinessEvidenceRecorder` criado para gravar colecoes de evidencia sem expor detalhes do sink ao dominio.
- Sample `Ordering` ganhou `OrderingEvidencePublisher`.
- Testes validam captura em memoria, composicao de sinks e publicacao da evidencia do `Order`.

## Implementado Na Iteracao De Tracing

- `ActivitySourceBusinessEvidenceSink` criado para emitir `BusinessEvidence` como activities/spans.
- Tags estaveis adicionadas para nome, tipo, mensagem, correlacao, timestamp e metadata.
- Teste com `ActivityListener` valida que evidencia de negocio chega ao tracing com tags semanticas.

## Implementado Na Iteracao De Logging

- `LoggerBusinessEvidenceSink` criado para emitir `BusinessEvidence` via `ILogger`.
- Dependencia `Microsoft.Extensions.Logging.Abstractions` adicionada ao pacote core.
- Campos estruturados adicionados com a mesma convencao semantica usada em tracing.
- Rejeicoes de fronteira e violacoes de invariantes sao logadas como `Warning`; demais evidencias como `Information`.
- Teste com logger fake valida nivel, event id, mensagem e metadata estruturada.

## Implementado Na Iteracao De Convencoes De Evidencia

- `BusinessEvidenceFields` criado como fonte de verdade para nomes de campos em logs/traces.
- `ActivitySourceBusinessEvidenceSink` e `LoggerBusinessEvidenceSink` passaram a usar as constantes compartilhadas.
- `docs/evidence-conventions.md` criado com campos, mapping de severidade, padrao de trace naming e orientacoes de metadata.
- `v2/README.md` aponta para as convencoes de evidencia.

## Implementado Na Iteracao De Lifecycle De Side Effects

- `SideEffectLifecycleStatus` criado para representar `Planned`, `Persisted`, `Published`, `Confirmed`, `CompensationRequired` e `Compensated`.
- `SideEffectLifecycleRecord` criado para guardar estado atual, chave de idempotencia, ultima evidencia e historico de evidencias.
- `ISideEffectLifecycleStore` criado como contrato de persistencia do lifecycle.
- `InMemorySideEffectLifecycleStore` criado para testes, samples e diagnostico local.
- `GovernedSideEffectLifecycle<TContext>` criado para planejar e avancar o lifecycle de efeitos externos com evidencia estruturada.
- Sample `Ordering` ganhou `NotifyFulfillmentLifecycle`, classe especializada que encapsula o primitive generico.
- Testes validam plano, persistencia, publicacao, confirmacao, reutilizacao por idempotencia e compensacao.

## Implementado Na Iteracao De Ordering Completo

- `OrderStatus` passou a modelar o fluxo principal: `PlacedAwaitingPayment`, `PaidAwaitingFulfillment`, `FulfilledReadyForShipping`, `ShippedWaitingDelivery`, `Delivered` e `Cancelled`.
- `OrderFactory` criado para admitir criacao de pedido e impedir status inicial arbitrario vindo de fronteira externa.
- Construcao direta de `Order` foi fechada para o sample; `Order.Rehydrate` explicita cenarios de estado ja persistido.
- `ExternalCreateOrderRequest`, `OrderCreationRequirement` e `OrderLine` criados para representar criacao com significado de negocio.
- Transicoes especializadas criadas para `CapturePayment`, `ShipOrder`, `DeliverOrder` e `CancelOrder`.
- `PrepareOrderForShippingTransition` atualizado para partir de `PaidAwaitingFulfillment`.
- `OrderFulfillmentTrustableModel` atualizado para descrever criacao, pagamento, preparacao, envio, entrega e cancelamento.
- Testes cobrem criacao via factory, rejeicao de status inicial arbitrario, pagamento, fluxo feliz completo ate entrega e rejeicao de cancelamento apos envio.

## Implementado Na Iteracao De Fronteiras Do Ordering

- Estados do pedido passaram a usar o padrao "estado atual + espera operacional": `PlacedAwaitingPayment`, `PaidAwaitingFulfillment`, `FulfilledReadyForShipping`, `ShippedWaitingDelivery`, `Delivered` e `Cancelled`.
- Requirements do sample foram movidos para `v2/samples/TrustableCode.SDK.Samples.Ordering/Requirements`.
- Admissions especializadas criadas para `CapturePayment`, `ShipOrder`, `DeliverOrder` e `CancelOrder`.
- Requests externas criadas para pagamento, envio, entrega e cancelamento.
- Descriptor do `Ordering` atualizado com fronteiras e evidencias de rejeicao para pagamento, envio, entrega e cancelamento.
- Testes validam admissions aceitas/rejeitadas e confirmam a nova nomenclatura dos estados no contexto de agente.

## Implementado Na Iteracao De Helpers De Teste

- `TrustableCheck` criado como resultado neutro de framework, com lista de falhas e `ThrowIfFailed`.
- `TrustableChecks` criado com helpers para transicoes aplicadas/rejeitadas, admissoes aceitas/rejeitadas, invariantes preservados/violados, side effects e evidencia nomeada.
- Testes usam o sample `Ordering` para validar checks positivos e diagnostico de falha.

## Implementado Na Iteracao De Preconditions Especializadas

- `BusinessInvariantRule<TContext>` criado como base comum para regras de negocio executaveis.
- `BusinessInvariant<TContext>` passou a herdar de `BusinessInvariantRule<TContext>`.
- `TransitionPrecondition<TState, TContext>` passou a herdar de `BusinessInvariantRule<TransitionContext<TState, TContext>>`.
- Construtor de `TransitionPrecondition` ficou protegido para desencorajar uso direto em aplicacoes.
- Sample `Ordering` ganhou preconditions especializadas em `Transitions/Preconditions`.
- Transicoes do sample passaram a usar classes como `PaymentMustBeCapturedPrecondition`, `CarrierRequiredPrecondition` e `OrderMustBeCancellablePrecondition` em vez de instanciar a precondition generica.
- Teste valida que uma precondition especializada pode ser avaliada como regra de invariante de negocio.
