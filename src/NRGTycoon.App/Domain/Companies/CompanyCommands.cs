using Quasar.Cqrs;
using Quasar.Core;

namespace NRGTycoon.App.Domain.Companies;

/// <summary>
/// Command to create a new company for a player.
/// </summary>
public sealed record CreateCompanyCommand(
    Guid OwnerId,
    string Name) : ICommand<Result<Guid>>;

/// <summary>
/// Command to update company name.
/// </summary>
public sealed record UpdateCompanyNameCommand(
    Guid OwnerId,
    string NewName) : ICommand<Result>;

/// <summary>
/// Command to record a balance movement.
/// </summary>
public sealed record RecordBalanceMovementCommand(
    Guid OwnerId,
    decimal Amount,
    string Description) : ICommand<Result>;

/// <summary>
/// Query to get company dashboard data.
/// </summary>
public sealed record GetCompanyDashboardQuery(Guid OwnerId) : IQuery<CompanyDashboardDto?>;

/// <summary>
/// DTO for dashboard response.
/// </summary>
public sealed record CompanyDashboardDto(
    Guid CompanyId,
    string Name,
    decimal Balance,
    decimal Oil,
    decimal Gas,
    decimal Uranium,
    DateTimeOffset CreatedAt,
    IReadOnlyList<BalanceMovementDto> RecentMovements);

/// <summary>
/// DTO for balance movement.
/// </summary>
public sealed record BalanceMovementDto(
    Guid Id,
    decimal Amount,
    string Description,
    DateTimeOffset Timestamp);
