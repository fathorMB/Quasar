using Microsoft.EntityFrameworkCore;
using NRGTycoon.App.Domain.Companies;
using NRGTycoon.App.ReadModels;
using Quasar.Persistence.Relational.EfCore;
using Quasar.Projections.Abstractions;

namespace NRGTycoon.App.Projections;

/// <summary>
/// Projection that updates Company read models from domain events.
/// </summary>
public sealed class CompanyProjection :
    IProjection<CompanyCreated>,
    IProjection<CompanyNameUpdated>,
    IProjection<BalanceMovementRecorded>,
    ILiveProjection<CompanyCreated>,
    ILiveProjection<CompanyNameUpdated>,
    ILiveProjection<BalanceMovementRecorded>
{
    private readonly IDbContextFactory<ReadModelContext<NRGTycoonReadModelStore>> _contextFactory;
    private readonly Microsoft.Extensions.Logging.ILogger<CompanyProjection> _logger;

    public CompanyProjection(
        IDbContextFactory<ReadModelContext<NRGTycoonReadModelStore>> contextFactory,
        Microsoft.Extensions.Logging.ILogger<CompanyProjection> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task HandleAsync(CompanyCreated @event, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var existing = await context.Set<CompanyReadModel>()
            .FirstOrDefaultAsync(c => c.Id == @event.CompanyId, cancellationToken);
        
        if (existing != null)
            return; // Idempotency
        
        context.Set<CompanyReadModel>().Add(new CompanyReadModel
        {
            Id = @event.CompanyId,
            OwnerId = @event.OwnerId,
            Name = @event.Name,
            Balance = 0, // Balance initialized to 0, will be updated by BalanceMovementRecorded
            Oil = 0,
            Gas = 0,
            Uranium = 0,
            CreatedAt = @event.CreatedAt,
            UpdatedAt = @event.CreatedAt
        });
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(CompanyNameUpdated @event, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var company = await context.Set<CompanyReadModel>()
            .FirstOrDefaultAsync(c => c.Id == @event.CompanyId, cancellationToken);
        
        if (company == null)
            return;
        
        company.Name = @event.NewName;
        company.UpdatedAt = @event.UpdatedAt;
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(BalanceMovementRecorded @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling BalanceMovementRecorded: {Description} ({Amount} $)", @event.Description, @event.Amount);
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        // Add balance movement record
        var existingMovement = await context.Set<BalanceMovementReadModel>()
            .FirstOrDefaultAsync(m => m.Id == @event.MovementId, cancellationToken);
        
        if (existingMovement == null)
        {
            context.Set<BalanceMovementReadModel>().Add(new BalanceMovementReadModel
            {
                Id = @event.MovementId,
                CompanyId = @event.CompanyId,
                Amount = @event.Amount,
                Description = @event.Description,
                Timestamp = @event.Timestamp
            });

            // Update company balance ONLY if movement is new
            var company = await context.Set<CompanyReadModel>()
                .FirstOrDefaultAsync(c => c.Id == @event.CompanyId, cancellationToken);
            
            if (company != null)
            {
                company.Balance += @event.Amount;
                company.UpdatedAt = @event.Timestamp;
            }
        
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
