using Microsoft.EntityFrameworkCore;
using NRGTycoon.App.Domain.Companies;
using NRGTycoon.App.ReadModels;
using Quasar.Cqrs;
using Quasar.Persistence.Relational.EfCore;

namespace NRGTycoon.App.Handlers.Companies;

/// <summary>
/// Query handler for company dashboard data.
/// </summary>
public sealed class GetCompanyDashboardHandler : IQueryHandler<GetCompanyDashboardQuery, CompanyDashboardDto?>
{
    private readonly IDbContextFactory<ReadModelContext<NRGTycoonReadModelStore>> _contextFactory;

    public GetCompanyDashboardHandler(IDbContextFactory<ReadModelContext<NRGTycoonReadModelStore>> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<CompanyDashboardDto?> Handle(GetCompanyDashboardQuery query, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var company = await context.Set<CompanyReadModel>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.OwnerId == query.OwnerId, cancellationToken);
        
        if (company == null)
            return null;
        
        // Fetch all movements for the company and sort in-memory
        // SQLite limitation: Does not support DateTimeOffset in ORDER BY clauses
        var allMovements = await context.Set<BalanceMovementReadModel>()
            .AsNoTracking()
            .Where(m => m.CompanyId == company.Id)
            .ToListAsync(cancellationToken);

        var recentMovements = allMovements
            .OrderByDescending(m => m.Timestamp)
            .Take(10)
            .Select(m => new BalanceMovementDto(m.Id, m.Amount, m.Description, m.Timestamp))
            .ToList();
        
        return new CompanyDashboardDto(
            company.Id,
            company.Name,
            company.Balance,
            company.Oil,
            company.Gas,
            company.Uranium,
            company.CreatedAt,
            recentMovements);
    }
}
