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
| 9. Pacote de contexto para agentes | Concluido | `AgentContextPacket` gera markdown com ordem de leitura, fluxo esperado, detalhes de transicoes, fronteiras, rejeicoes, side effects, evidencias e checklist de mudanca. Sample `Ordering` inclui export pronto. |
| 10. Samples alinhados ao livro | Parcial | Sample `Ordering` agora cobre criacao via factory e fluxo principal do pedido ate entrega/cancelamento. Ainda faltam exemplos por apendice: unsafe, trustable manual e trustable usando SDK. |
| 11. Testes para confianca | Parcial | `TrustableChecks` fornece checks neutros de framework para transicoes, admissoes, invariantes, side effects e evidencia. Exemplos documentados em `docs/testing-helpers.md`. Builder de cenario do sample `Ordering` reduz montagem repetitiva. |
| 12. Packaging e publicacao | Parcial | Metadata NuGet e README de pacote preparados. Ainda falta validar pipeline de release e publicar. |

## Decisoes Iniciais

- `v1/` deve permanecer como snapshot preservado da implementacao atual.
- `v2/` deve evoluir sem carregar todas as decisoes da v1 automaticamente.
- O primeiro pacote da v2 se chama `TrustableCode.SDK.TrustableModeling`.
- O primeiro artefato conceitual da v2 e o `TrustableModelDescriptor`.
- O SDK deve produzir contexto legivel para humanos e agentes, nao apenas tipos de runtime.

## Proxima Etapa

Avaliar o escopo de packaging e publicacao da v2: metadata NuGet, README de pacote e pipeline de release.

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

## Implementado Na Iteracao De Agent Context Enriquecido

- `AgentContextPacket.ToMarkdown()` passou a renderizar estados iniciais/terminais.
- Secao de ordem sugerida de leitura adicionada para orientar humanos e agentes antes de alterar codigo.
- Secao de fluxo esperado de estados adicionada a partir das transicoes declaradas.
- Transicoes passaram a incluir precondicoes, eventos produzidos e evidencias produzidas no markdown.
- Fronteiras passaram a incluir regras de admissao e evidencias de rejeicao no markdown.
- Side effects passaram a exibir consistencia, idempotencia e compensacao.
- Mapa de rejeicao/observacao e checklist de mudanca adicionados.
- Teste do contexto de agente protege as novas secoes principais usando o sample `Ordering`.

## Implementado Na Iteracao De Docs Dos Helpers De Teste

- `docs/testing-helpers.md` criado com exemplos curtos para transicao aplicada, transicao rejeitada, admissao, invariante, side effect e evidencia.
- `v2/README.md` passou a apontar para a documentacao dos helpers de teste.

## Implementado Na Iteracao De Export De Contexto Para Agentes

- `agent-context.md` exportado no sample `Ordering` com o markdown gerado por `AgentContextPacket`.
- `README.md` do sample passou a apontar para o contexto exportado.
- Teste garante que o export do sample permanece sincronizado com `OrderFulfillmentTrustableModel.Descriptor`.

## Implementado Na Iteracao De Builders De Cenario De Teste

- `OrderingScenarioBuilder` criado no projeto de testes para centralizar montagem comum do sample `Ordering`.
- Testes de transicao, admissao e checks passaram a usar o builder para requests, requirements e ordens reidratadas.
- O plano vivo passou a apontar packaging/publicacao como proxima frente pendente da v2.

## Implementado Na Iteracao De Organizacao Do Sample Ordering

- Requests externos movidos para `ExternalRequests/`.
- Admissions movidas para `Admissions/`.
- Invariantes e publicacao de evidencia movidas para pastas dedicadas.
- `class-reference.md` criado para explicar cada classe do sample e o fluxo pratico de uso do SDK.

## Implementado Na Iteracao De Service De Aplicacao Do Ordering

- `OrderingApplicationService` criado como ponto de entrada pratico do sample.
- Service compoe request externa, admissao, requirement, aggregate, transicao governada, publicacao de evidencia e lifecycle de side effect.
- `OrderingApplicationResult` criado para expor resultado de admissao, status da transicao, eventos, evidencias e lifecycle opcional.
- `OrderingEvidencePublisher` passou a publicar apenas evidencias ainda nao publicadas por pedido.
- Testes cobrem criacao aceita, rejeicao de admissao, preparacao com lifecycle de notificacao e cancelamento rejeitado apos envio.

## Implementado Na Iteracao De Ergonomia Do Service De Aplicacao

- `AdmittedTransitionResult` criado para representar admissao seguida de transicao governada.
- `TrustableAdmissionFlow.ExecuteTransition` criado para reduzir repeticao em services de aplicacao.
- `SideEffectLifecycleFlow.PlanPersistAndPublish` criado para o fluxo comum de side effect planejado, persistido e publicado.
- `OrderingApplicationService` passou a usar os helpers novos para manter o fluxo explicito com menos cerimonia.
- Testes cobrem os helpers de admissao/transicao e lifecycle de side effect.

## Implementado Na Iteracao De Guia De Application Service

- `docs/application-service-pattern.md` criado com o fluxo recomendado para usar a v2 em camada de aplicacao.
- Guia documenta `ExternalRequest -> BusinessAdmission -> Requirement -> GovernedTransition -> BusinessEvidence -> SideEffectLifecycle`.
- Exemplos curtos adicionados para `TrustableAdmissionFlow.ExecuteTransition` e `PlanPersistAndPublish`.
- `v2/README.md` passou a apontar para o guia de application service.

## Implementado Na Iteracao De Preparacao Do Pacote NuGet

- Metadata do pacote `TrustableCode.SDK.TrustableModeling` refinada com titulo, repositorio, tags, descricao e release notes de preview.
- README especifico do pacote criado em `src/TrustableCode.SDK.TrustableModeling/README.md`.
- Pacote passou a incluir o README local do projeto em vez do README geral da v2.

## Implementado Na Iteracao De Builder De Admissao

- `BusinessAdmissionBuilder<TInput, TAccepted>` criado para declarar regras de fronteira com menos boilerplate.
- `BusinessAdmission<TInput, TAccepted>.Create(name)` adicionado como entrada fluente.
- Builder suporta `Require`, `RejectWhen`, `AcceptWith` e `Build`.
- Admissions do sample `Ordering` e `OrderFactory` passaram a usar o builder.
- Docs e README de pacote passaram a mostrar o estilo fluente de admissao.

## Implementado Na Iteracao De Builder De Transicao

- `GovernedTransitionBuilder<TState, TContext>` criado para declarar transicoes governadas com menos boilerplate.
- `GovernedTransition<TState, TContext>.Create(name)` adicionado como entrada fluente.
- Builder suporta `From`, `To`, `ReadState`, `ApplyState`, `Require`, `Preserve`, `ProducesEvent`, `ProducesEvidence`, `TreatRepetitionAsAlreadyApplied` e `Build`.
- Transicoes do sample `Ordering` passaram a usar o builder fluente.
- Docs explicam `ApplyState(order.ApplyStatus)` como callback controlado chamado apenas depois da aprovacao da transicao.

## Implementado Na Iteracao De Estado Governado

- `GovernedState<TState>` criado para representar estado que deve mudar apenas por transicoes aprovadas.
- `GovernedTransitionBuilder.State(...)` adicionado como atalho para leitura e aplicacao de estado governado.
- Sample `Ordering` passou a expor `Order.Status` como leitura de `StatusState.Current`.
- Transicoes do sample passaram a usar `.State(order.StatusState)` em vez de callbacks manuais `ReadState` e `ApplyState`.
- Docs atualizadas para explicar `GovernedState` e `ApplyApproved`.

## Implementado Na Iteracao De Rehydration Explicita

- `OrderPersistenceSnapshot` criado no sample `Ordering` para representar estado confiavel vindo de persistencia.
- `Order.Rehydrate(OrderPersistenceSnapshot)` adicionado como caminho explicito para recriar agregados ja existentes.
- Testes passaram a usar snapshot persistido via `OrderingScenarioBuilder`.
- Docs do sample esclarecem que rehydration nao e criacao de negocio e nao produz evidencia de criacao.

## Implementado Na Iteracao De Persistencia E Outbox Em Memoria

- `IOrderSnapshotStore` e `InMemoryOrderSnapshotStore` criados no sample `Ordering`.
- `IOrderingOutbox`, `InMemoryOrderingOutbox` e `OrderingOutboxMessage` criados para demonstrar outbox simples.
- `PersistedOrderingApplicationService` criado para carregar snapshot, reidratar aggregate, executar transicao, salvar snapshot atualizado e enfileirar eventos produzidos.
- Testes cobrem preparacao persistida com snapshot/outbox/evidencia e rejeicao sem salvar nem enfileirar evento.

## Implementado Na Iteracao De Guia De Fluxo Persistido

- `docs/persisted-application-flow.md` criado para documentar snapshot, rehydration, save, outbox e evidencia.
- Guia separa explicitamente o que deve ficar na aplicacao e o que deve ficar no SDK.
- `v2/README.md` e `docs/application-service-pattern.md` passaram a apontar para o guia persistido.

## Implementado Na Iteracao De README Do Sample Ordering

- README do sample `Ordering` reescrito como jornada pratica por tarefas.
- Secoes adicionadas para criacao de pedido, operacao em memoria, fluxo persistido, estado governado, fronteiras, evidencia e outbox.
- `class-reference.md` permanece como referencia detalhada classe a classe.
