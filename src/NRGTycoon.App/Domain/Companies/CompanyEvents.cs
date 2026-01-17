using Quasar.EventSourcing.Abstractions;

namespace NRGTycoon.App.Domain.Companies;

/// <summary>
/// Event raised when a new company is created for a player.
/// </summary>
public sealed record CompanyCreated(
    Guid CompanyId,
    Guid OwnerId,
    string Name,
    decimal InitialBalance,
    DateTimeOffset CreatedAt) : IEvent;

/// <summary>
/// Event raised when company name is updated.
/// </summary>
public sealed record CompanyNameUpdated(
    Guid CompanyId,
    string NewName,
    DateTimeOffset UpdatedAt) : IEvent;

/// <summary>
/// Event raised when a balance movement is recorded (credit or debit).
/// </summary>
public sealed record BalanceMovementRecorded(
    Guid CompanyId,
    Guid MovementId,
    decimal Amount,
    string Description,
    DateTimeOffset Timestamp) : IEvent;
