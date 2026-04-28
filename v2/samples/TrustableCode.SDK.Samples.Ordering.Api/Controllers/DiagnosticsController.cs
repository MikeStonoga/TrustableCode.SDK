using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustableCode.SDK.Samples.Ordering.Api.Persistence;

namespace TrustableCode.SDK.Samples.Ordering.Api.Controllers;

[ApiController]
[Route("api/diagnostics")]
public sealed class DiagnosticsController(OrderingDbContext db) : ControllerBase
{
    [HttpGet("outbox")]
    public async Task<ActionResult<IReadOnlyList<OrderingOutboxMessageEntity>>> Outbox()
        => Ok(await db.OutboxMessages
            .OrderBy(message => message.EnqueuedAt)
            .ToArrayAsync());

    [HttpGet("evidence")]
    public async Task<ActionResult<IReadOnlyList<BusinessEvidenceEntity>>> Evidence()
        => Ok(await db.BusinessEvidence
            .OrderBy(evidence => evidence.ObservedAt)
            .ToArrayAsync());
}
