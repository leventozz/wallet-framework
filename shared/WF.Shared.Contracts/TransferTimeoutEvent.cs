using System;

namespace WF.Shared.Contracts;

public record TransferTimeoutEvent(Guid CorrelationId);
