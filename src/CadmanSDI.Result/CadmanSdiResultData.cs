namespace CadmanSDI.Result;

public sealed record CadmanSdiResultData(
    string? ProjectId,
    string? PartTypeId,
    string? JobStatus,
    string? JobStatusReason,
    string? PreparationJobStatus,
    string? PreparationJobStatusReason,
    string? BendingExecutionValid,
    string? BendingExecutionSummaryValid,
    string? BendingExecutionSummaryReason);
