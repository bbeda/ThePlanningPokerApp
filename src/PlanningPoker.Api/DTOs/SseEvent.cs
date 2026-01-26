namespace PlanningPoker.Api.DTOs;

public record SseEvent(
    string Type,
    object Data,
    string? EventId = null,
    int? Retry = null
);
