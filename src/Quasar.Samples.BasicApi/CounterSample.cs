using Quasar.Core;
using Quasar.Cqrs;
using Quasar.Domain;
using Quasar.EventSourcing.Abstractions;
using Quasar.Persistence.Abstractions;
using Quasar.Security;
using System.Collections.Generic;

namespace Quasar.Samples.BasicApi;

// Event
public sealed record CounterIncremented(int Amount) : IEvent;

// Shopping cart events
public sealed record CartItemAdded(Guid ProductId, int Quantity) : IEvent;
public sealed record CartItemRemoved(Guid ProductId, int Quantity) : IEvent;

// Aggregate
public sealed class CounterAggregate : AggregateRoot
{
    public int Count { get; private set; }
    public CounterAggregate() { Id = SampleConfig.CounterStreamId; }

    public void Increment(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        ApplyChange(new CounterIncremented(amount));
    }

    private void When(CounterIncremented e)
    {
        Count += e.Amount;
    }
}

public sealed class ShoppingCartAggregate : AggregateRoot
{
    private readonly Dictionary<Guid, int> _lines = new();
    public int TotalItems => _lines.Values.Sum();

    public ShoppingCartAggregate() { Id = SampleConfig.CartStreamId; }

    public void Add(Guid productId, int qty)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        ApplyChange(new CartItemAdded(productId, qty));
    }

    public void Remove(Guid productId, int qty)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        ApplyChange(new CartItemRemoved(productId, qty));
    }

    private void When(CartItemAdded a)
    {
        _lines[a.ProductId] = _lines.TryGetValue(a.ProductId, out var q) ? q + a.Quantity : a.Quantity;
    }

    private void When(CartItemRemoved r)
    {
        if (_lines.TryGetValue(r.ProductId, out var curr))
        {
            var next = Math.Max(0, curr - r.Quantity);
            if (next == 0) _lines.Remove(r.ProductId);
            else _lines[r.ProductId] = next;
        }
    }
}
// Command
public sealed record IncrementCounterCommand(Guid SubjectId, int Amount) : ICommand<Result<int>>, IAuthorizableRequest
{
    public string Action => "counter.increment";
    public string Resource => $"counter:{SampleConfig.CounterStreamId}";
}

// Query
public sealed record GetCounterQuery() : IQuery<int>;

public sealed record AddCartItemCommand(Guid SubjectId, Guid ProductId, int Quantity) : ICommand<Result<int>>, IAuthorizableRequest
{
    public string Action => "cart.add";
    public string Resource => $"cart:{SampleConfig.CartStreamId}";
}

public sealed record GetCartQuery() : IQuery<CartReadModel>;

// Validator
public sealed class IncrementCounterValidator : IValidator<IncrementCounterCommand>
{
    public Task<List<Error>> ValidateAsync(IncrementCounterCommand instance, CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();
        if (instance.Amount <= 0)
        {
            errors.Add(new Error("validation.amount", "Amount must be > 0"));
        }
        return Task.FromResult(errors);
    }
}

// Handlers
public sealed class IncrementCounterHandler : ICommandHandler<IncrementCounterCommand, Result<int>>
{
    private readonly IEventSourcedRepository<CounterAggregate> _repo;
    public IncrementCounterHandler(IEventSourcedRepository<CounterAggregate> repo) => _repo = repo;

    public async Task<Result<int>> Handle(IncrementCounterCommand command, CancellationToken cancellationToken = default)
    {
        var agg = await _repo.GetAsync(SampleConfig.CounterStreamId, cancellationToken);
        if (agg.Id == Guid.Empty) agg = new CounterAggregate();
        agg.Increment(command.Amount);
        await _repo.SaveAsync(agg, cancellationToken);
        return Result<int>.Success(agg.Count);
    }
}

public sealed class GetCounterHandler : IQueryHandler<GetCounterQuery, int>
{
    private readonly IEventStore _store;
    public GetCounterHandler(IEventStore store) => _store = store;

    public async Task<int> Handle(GetCounterQuery query, CancellationToken cancellationToken = default)
    {
        var events = await _store.ReadStreamAsync(SampleConfig.CounterStreamId, 0, cancellationToken);
        var agg = new CounterAggregate();
        agg.Rehydrate(events.Select(e => e.Event));
        return agg.Count;
    }
}

public sealed class AddCartItemValidator : IValidator<AddCartItemCommand>
{
    public Task<List<Error>> ValidateAsync(AddCartItemCommand instance, CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();
        if (instance.Quantity <= 0)
        {
            errors.Add(new Error("validation.quantity", "Quantity must be > 0"));
        }
        if (instance.ProductId == Guid.Empty)
        {
            errors.Add(new Error("validation.product_id", "ProductId required"));
        }
        return Task.FromResult(errors);
    }
}

public sealed class AddCartItemHandler : ICommandHandler<AddCartItemCommand, Result<int>>
{
    private readonly IEventSourcedRepository<ShoppingCartAggregate> _repo;
    public AddCartItemHandler(IEventSourcedRepository<ShoppingCartAggregate> repo) => _repo = repo;

    public async Task<Result<int>> Handle(AddCartItemCommand command, CancellationToken cancellationToken = default)
    {
        var agg = await _repo.GetAsync(SampleConfig.CartStreamId, cancellationToken);
        if (agg.Id == Guid.Empty) agg = new ShoppingCartAggregate();
        agg.Add(command.ProductId, command.Quantity);
        await _repo.SaveAsync(agg, cancellationToken);
        return Result<int>.Success(agg.TotalItems);
    }
}

public sealed class GetCartHandler : IQueryHandler<GetCartQuery, CartReadModel>
{
    private readonly IReadRepository<CartReadModel> _repo;
    public GetCartHandler(IReadRepository<CartReadModel> repo) => _repo = repo;
    public async Task<CartReadModel> Handle(GetCartQuery query, CancellationToken cancellationToken = default)
    {
        var m = await _repo.GetByIdAsync(SampleConfig.CartStreamId, cancellationToken);
        return m ?? new CartReadModel { Id = SampleConfig.CartStreamId, TotalItems = 0 };
    }
}

// In-memory Authorization
public sealed class InMemoryAuthorizationService : IAuthorizationService
{
    public Task<bool> AuthorizeAsync(Guid subjectId, string action, string resource, CancellationToken cancellationToken = default)
    {
        // Demo policy: allow if subject is present and action targets counter, cart, or checkout flows
        var allowed = subjectId != Guid.Empty &&
                      (action.StartsWith("counter", StringComparison.Ordinal) ||
                       action.StartsWith("cart", StringComparison.Ordinal) ||
                       action.StartsWith("checkout", StringComparison.Ordinal));
        return Task.FromResult(allowed);
    }
}

internal static class SampleConfig
{
    public static readonly Guid CounterStreamId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    public static readonly Guid CartStreamId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-ffffffffffff");
    public static readonly Guid SensorStreamId = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff");
    public static readonly Guid DemoUserId = Guid.Parse("a01c4be3-17f8-4cb9-a379-4c597a5e0714");
    public static readonly Guid DemoRoleId = Guid.Parse("b01c4be3-17f8-4cb9-a379-4c597a5e0714");
    public const string DemoRoleName = "CartAdmin";
    public const string CartAddPermission = "cart.add";
}
