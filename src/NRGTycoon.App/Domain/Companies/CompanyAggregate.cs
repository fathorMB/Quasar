using Quasar.Domain;

namespace NRGTycoon.App.Domain.Companies;

/// <summary>
/// Constants for the Company domain.
/// </summary>
public static class CompanyConstants
{
    /// <summary>
    /// Initial balance given to new companies.
    /// </summary>
    public const decimal InitialBalance = 10_000m;
}

/// <summary>
/// Aggregate root for player companies.
/// Each player has exactly one company - the stream ID is the OwnerId.
/// </summary>
public sealed class CompanyAggregate : AggregateRoot
{
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public decimal Oil { get; private set; }
    public decimal Gas { get; private set; }
    public decimal Uranium { get; private set; }
    public Guid OwnerId { get; private set; }
    public bool HasCompany { get; private set; }

    /// <summary>
    /// Creates a new company for a player.
    /// </summary>
    public void Create(Guid companyId, Guid ownerId, string name)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company ID cannot be empty", nameof(companyId));
        
        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner ID cannot be empty", nameof(ownerId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Company name cannot be empty", nameof(name));

        // Check if company already exists (idempotency)
        if (HasCompany)
            return;

        // IMPORTANT: Set the aggregate ID here to match the stream ID (OwnerId)
        Id = ownerId;

        ApplyChange(new CompanyCreated(
            companyId,
            ownerId,
            name.Trim(),
            CompanyConstants.InitialBalance,
            DateTimeOffset.UtcNow));

        // Record initial capital as first balance movement
        ApplyChange(new BalanceMovementRecorded(
            companyId,
            Guid.NewGuid(),
            CompanyConstants.InitialBalance,
            "Initial Capital",
            DateTimeOffset.UtcNow));

        // Initial resources could be added here if needed, 
        // for now they default to 0 in CompanyCreated (which is handled by state init)
    }

    /// <summary>
    /// Updates the company name.
    /// </summary>
    public void UpdateName(string newName)
    {
        if (!HasCompany)
            throw new InvalidOperationException("Company does not exist.");

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Company name cannot be empty", nameof(newName));

        ApplyChange(new CompanyNameUpdated(CompanyId, newName.Trim(), DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Records a balance movement (credit if positive, debit if negative).
    /// </summary>
    public void RecordBalanceMovement(decimal amount, string description)
    {
        if (!HasCompany)
            throw new InvalidOperationException("Company does not exist.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        ApplyChange(new BalanceMovementRecorded(
            CompanyId,
            Guid.NewGuid(),
            amount,
            description.Trim(),
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Dispatches events to the appropriate handler method.
    /// </summary>
    protected override void When(IDomainEvent @event)
    {
        switch (@event)
        {
            case CompanyCreated e:
                Apply(e);
                break;
            case CompanyNameUpdated e:
                Apply(e);
                break;
            case BalanceMovementRecorded e:
                Apply(e);
                break;
        }
    }

    // Event handlers
    private void Apply(CompanyCreated @event)
    {
        HasCompany = true;
        CompanyId = @event.CompanyId;
        Id = @event.OwnerId;  // Stream ID is OwnerId
        OwnerId = @event.OwnerId;
        Name = @event.Name;
        // Balance is initialized to 0 here. Initial capital is handled by the subsequent BalanceMovementRecorded event.
        Balance = 0;
        Oil = 0;
        Gas = 0;
        Uranium = 0;
    }

    private void Apply(CompanyNameUpdated @event)
    {
        Name = @event.NewName;
    }

    private void Apply(BalanceMovementRecorded @event)
    {
        Balance += @event.Amount;
    }
}
