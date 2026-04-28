using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustableCode.SDK.Samples.Ordering.Api.Persistence;

namespace TrustableCode.SDK.Samples.Ordering.Api.Controllers;

[ApiController]
[Route("api/diagnostics")]
[Produces("application/json")]
[Tags("Diagnostics")]
public sealed class DiagnosticsController(OrderingDbContext db) : ControllerBase
{
    [HttpGet("outbox")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderingOutboxMessageEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderingOutboxMessageEntity>>> Outbox()
        => Ok(await db.OutboxMessages
            .OrderBy(message => message.EnqueuedAt)
            .ToArrayAsync());

    [HttpGet("evidence")]
    [ProducesResponseType(typeof(IReadOnlyList<BusinessEvidenceEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BusinessEvidenceEntity>>> Evidence()
        => Ok(await db.BusinessEvidence
            .OrderBy(evidence => evidence.ObservedAt)
            .ToArrayAsync());
}
