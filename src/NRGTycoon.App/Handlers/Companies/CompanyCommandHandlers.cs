using NRGTycoon.App.Domain.Companies;
using Quasar.Cqrs;
using Quasar.Core;
using Quasar.EventSourcing.Abstractions;

namespace NRGTycoon.App.Handlers.Companies;

/// <summary>
/// Handler for creating a new company.
/// </summary>
public sealed class CreateCompanyHandler : ICommandHandler<CreateCompanyCommand, Result<Guid>>
{
    private readonly IEventSourcedRepository<CompanyAggregate> _repo;

    public CreateCompanyHandler(IEventSourcedRepository<CompanyAggregate> repo)
    {
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(CreateCompanyCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use owner ID as the stream ID (one company per user)
            var aggregate = await _repo.GetAsync(command.OwnerId, cancellationToken);
            
            var companyId = Guid.NewGuid();
            aggregate.Create(companyId, command.OwnerId, command.Name);
            
            await _repo.SaveAsync(aggregate, cancellationToken);
            return Result<Guid>.Success(companyId);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(new Error("CreateCompany.Failed", ex.Message));
        }
    }
}

/// <summary>
/// Handler for updating company name.
/// </summary>
public sealed class UpdateCompanyNameHandler : ICommandHandler<UpdateCompanyNameCommand, Result>
{
    private readonly IEventSourcedRepository<CompanyAggregate> _repo;

    public UpdateCompanyNameHandler(IEventSourcedRepository<CompanyAggregate> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(UpdateCompanyNameCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var aggregate = await _repo.GetAsync(command.OwnerId, cancellationToken);
            aggregate.UpdateName(command.NewName);
            await _repo.SaveAsync(aggregate, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("UpdateCompanyName.Failed", ex.Message));
        }
    }
}

/// <summary>
/// Handler for recording balance movement.
/// </summary>
public sealed class RecordBalanceMovementHandler : ICommandHandler<RecordBalanceMovementCommand, Result>
{
    private readonly IEventSourcedRepository<CompanyAggregate> _repo;

    public RecordBalanceMovementHandler(IEventSourcedRepository<CompanyAggregate> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(RecordBalanceMovementCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var aggregate = await _repo.GetAsync(command.OwnerId, cancellationToken);
            aggregate.RecordBalanceMovement(command.Amount, command.Description);
            await _repo.SaveAsync(aggregate, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("RecordBalanceMovement.Failed", ex.Message));
        }
    }
}
