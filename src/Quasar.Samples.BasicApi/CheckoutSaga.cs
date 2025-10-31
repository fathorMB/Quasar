using Microsoft.Extensions.Logging;
using Quasar.Core;
using Quasar.Cqrs;
using Quasar.Sagas;
using Quasar.Security;

namespace Quasar.Samples.BasicApi;

/// <summary>
/// Command that begins a new checkout saga for the provided cart.
/// </summary>
public sealed record BeginCheckoutCommand(Guid SubjectId, Guid CheckoutId, Guid CartId, decimal TotalAmount) : ICommand<Result<Guid>>, IAuthorizableRequest
{
    public string Action => "checkout.start";
    public string Resource => $"checkout:{CheckoutId}";
}

/// <summary>
/// Command that confirms the payment step of the checkout saga.
/// </summary>
public sealed record ConfirmCheckoutPaymentCommand(Guid SubjectId, Guid CheckoutId, string PaymentReference) : ICommand<Result>, IAuthorizableRequest
{
    public string Action => "checkout.confirm";
    public string Resource => $"checkout:{CheckoutId}";
}

/// <summary>
/// Command that marks the checkout as fulfilled and completes the saga.
/// </summary>
public sealed record MarkCheckoutFulfilledCommand(Guid SubjectId, Guid CheckoutId, string? TrackingNumber) : ICommand<Result>, IAuthorizableRequest
{
    public string Action => "checkout.fulfill";
    public string Resource => $"checkout:{CheckoutId}";
}

/// <summary>
/// Handles orchestration logic for the sample checkout workflow.
/// </summary>
public sealed class CheckoutSaga :
    ISagaStartedBy<BeginCheckoutCommand, CheckoutSagaState>,
    ISagaHandles<ConfirmCheckoutPaymentCommand, CheckoutSagaState>,
    ISagaHandles<MarkCheckoutFulfilledCommand, CheckoutSagaState>
{
    private readonly ILogger<CheckoutSaga> _logger;
    private readonly CheckoutSagaMonitor _monitor;

    public CheckoutSaga(ILogger<CheckoutSaga> logger, CheckoutSagaMonitor monitor)
    {
        _logger = logger;
        _monitor = monitor;
    }

    public Task<SagaExecutionResult> HandleAsync(SagaContext<CheckoutSagaState> context, BeginCheckoutCommand message, CancellationToken cancellationToken = default)
    {
        var state = context.State;
        state.CheckoutId = message.CheckoutId;
        state.CartId = message.CartId;
        state.TotalAmount = message.TotalAmount;
        state.StartedAtUtc = DateTimeOffset.UtcNow;

        _logger.LogInformation("Checkout saga {CheckoutId} started for cart {CartId} with total {Total}.", state.Id, state.CartId, state.TotalAmount);

        _monitor.Record(state);
        _monitor.RecordSnapshot(CheckoutStatusResponse.FromState(state));
        return Task.FromResult(SagaExecutionResult.Continue(state));
    }

    public Task<SagaExecutionResult> HandleAsync(SagaContext<CheckoutSagaState> context, ConfirmCheckoutPaymentCommand message, CancellationToken cancellationToken = default)
    {
        var state = context.State;
        if (state.PaymentConfirmed)
        {
            _logger.LogDebug("Checkout saga {CheckoutId} already has a confirmed payment.", state.Id);
            return Task.FromResult(SagaExecutionResult.Continue(state));
        }

        state.PaymentConfirmed = true;
        state.PaymentReference = message.PaymentReference;
        state.PaymentConfirmedAtUtc = DateTimeOffset.UtcNow;

        _logger.LogInformation("Checkout saga {CheckoutId} payment confirmed with reference {Reference}.", state.Id, state.PaymentReference);

        _monitor.Record(state);
        _monitor.RecordSnapshot(CheckoutStatusResponse.FromState(state));
        return Task.FromResult(SagaExecutionResult.Continue(state));
    }

    public Task<SagaExecutionResult> HandleAsync(SagaContext<CheckoutSagaState> context, MarkCheckoutFulfilledCommand message, CancellationToken cancellationToken = default)
    {
        var state = context.State;
        state.FulfillmentRequested = true;
        state.TrackingNumber = message.TrackingNumber;
        state.CompletedAtUtc = DateTimeOffset.UtcNow;

        _logger.LogInformation("Checkout saga {CheckoutId} fulfilled. Tracking {TrackingNumber}.", state.Id, state.TrackingNumber ?? "<none>");

        _monitor.Record(state);
        _monitor.RecordSnapshot(CheckoutStatusResponse.FromState(state));
        return Task.FromResult(SagaExecutionResult.Completed(state));
    }
}

/// <summary>
/// Sample saga state persisted by the framework.
/// </summary>
public sealed class CheckoutSagaState : SagaState
{
    public Guid CheckoutId { get; set; }
    public Guid CartId { get; set; }
    public decimal TotalAmount { get; set; }
    public bool PaymentConfirmed { get; set; }
    public string? PaymentReference { get; set; }
    public DateTimeOffset? PaymentConfirmedAtUtc { get; set; }
    public bool FulfillmentRequested { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTimeOffset? StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
}

public sealed class BeginCheckoutHandler : ICommandHandler<BeginCheckoutCommand, Result<Guid>>
{
    private readonly ILogger<BeginCheckoutHandler> _logger;

    public BeginCheckoutHandler(ILogger<BeginCheckoutHandler> logger) => _logger = logger;

    public Task<Result<Guid>> Handle(BeginCheckoutCommand command, CancellationToken cancellationToken = default)
    {
        if (command.CheckoutId == Guid.Empty)
        {
            return Task.FromResult(Result<Guid>.Failure(new Error("checkout.invalid_id", "CheckoutId must be specified.")));
        }

        if (command.TotalAmount <= 0)
        {
            return Task.FromResult(Result<Guid>.Failure(new Error("checkout.invalid_total", "TotalAmount must be greater than zero.")));
        }

        _logger.LogInformation("Begin checkout command acknowledged for checkout {CheckoutId}.", command.CheckoutId);
        return Task.FromResult(Result<Guid>.Success(command.CheckoutId));
    }
}

public sealed class ConfirmCheckoutPaymentHandler : ICommandHandler<ConfirmCheckoutPaymentCommand, Result>
{
    private readonly ILogger<ConfirmCheckoutPaymentHandler> _logger;

    public ConfirmCheckoutPaymentHandler(ILogger<ConfirmCheckoutPaymentHandler> logger) => _logger = logger;

    public Task<Result> Handle(ConfirmCheckoutPaymentCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.PaymentReference))
        {
            return Task.FromResult(Result.Failure(new Error("checkout.payment_reference_required", "Payment reference is required.")));
        }

        _logger.LogInformation("Payment confirmation received for checkout {CheckoutId}.", command.CheckoutId);
        return Task.FromResult(Result.Success());
    }
}

public sealed class MarkCheckoutFulfilledHandler : ICommandHandler<MarkCheckoutFulfilledCommand, Result>
{
    private readonly ILogger<MarkCheckoutFulfilledHandler> _logger;

    public MarkCheckoutFulfilledHandler(ILogger<MarkCheckoutFulfilledHandler> logger) => _logger = logger;

    public Task<Result> Handle(MarkCheckoutFulfilledCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fulfillment request acknowledged for checkout {CheckoutId}.", command.CheckoutId);
        return Task.FromResult(Result.Success());
    }
}
