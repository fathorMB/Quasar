using Microsoft.EntityFrameworkCore;
using Quasar.EventSourcing.Abstractions;
using Quasar.Projections.Abstractions;

namespace Quasar.Samples.BasicApi;

public sealed class CounterProjection : IProjection<CounterIncremented>
{
    private readonly SampleReadModelContext _db;

    public CounterProjection(SampleReadModelContext db)
    {
        _db = db;
    }

    public async Task HandleAsync(CounterIncremented @event, CancellationToken cancellationToken = default)
    {
        var id = SampleConfig.CounterStreamId;
        var entity = await _db.Counters.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            entity = new CounterReadModel { Id = id, Count = 0 };
            _db.Counters.Add(entity);
        }
        entity.Count += @event.Amount;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

public sealed class ShoppingCartProjection :
    IProjection<CartItemAdded>,
    IProjection<CartItemRemoved>
{
    private readonly SampleReadModelContext _db;
    public ShoppingCartProjection(SampleReadModelContext db) => _db = db;

    public async Task HandleAsync(CartItemAdded @event, CancellationToken cancellationToken = default)
    {
        var id = SampleConfig.CartStreamId;
        var cart = await _db.Carts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cart is null)
        {
            cart = new CartReadModel { Id = id, TotalItems = @event.Quantity };
            _db.Carts.Add(cart);
        }
        else
        {
            cart.TotalItems += @event.Quantity;
        }

        var line = await _db.CartProducts.FirstOrDefaultAsync(x => x.ProductId == @event.ProductId, cancellationToken);
        if (line is null)
        {
            line = new CartProductLine { ProductId = @event.ProductId, Quantity = @event.Quantity };
            _db.CartProducts.Add(line);
        }
        else
        {
            line.Quantity += @event.Quantity;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(CartItemRemoved @event, CancellationToken cancellationToken = default)
    {
        var id = SampleConfig.CartStreamId;
        var cart = await _db.Carts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cart is null)
        {
            cart = new CartReadModel { Id = id, TotalItems = 0 };
            _db.Carts.Add(cart);
        }
        else
        {
            cart.TotalItems = Math.Max(0, cart.TotalItems - @event.Quantity);
        }

        var line = await _db.CartProducts.FirstOrDefaultAsync(x => x.ProductId == @event.ProductId, cancellationToken);
        if (line is null)
        {
            line = new CartProductLine { ProductId = @event.ProductId, Quantity = 0 };
            _db.CartProducts.Add(line);
        }
        else
        {
            line.Quantity = Math.Max(0, line.Quantity - @event.Quantity);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
