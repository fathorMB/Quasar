using Microsoft.EntityFrameworkCore;
using Quasar.EventSourcing.Abstractions;
using Quasar.Persistence.Relational.EfCore;
using Quasar.Projections.Abstractions;

namespace Quasar.Samples.BasicApi;

public sealed class CounterProjection : IProjection<CounterIncremented>
{
    private readonly ReadModelContext<SampleReadModelStore> _db;
    private readonly DbSet<CounterReadModel> _counters;

    public CounterProjection(ReadModelContext<SampleReadModelStore> db)
    {
        _db = db;
        _counters = db.Set<CounterReadModel>();
    }

    public async Task HandleAsync(CounterIncremented @event, CancellationToken cancellationToken = default)
    {
        var id = SampleConfig.CounterStreamId;
        var entity = await _counters.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            entity = new CounterReadModel { Id = id, Count = 0 };
            await _counters.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        }
        entity.Count += @event.Amount;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

public sealed class ShoppingCartProjection :
    IProjection<CartItemAdded>,
    IProjection<CartItemRemoved>
{
    private readonly ReadModelContext<SampleReadModelStore> _db;
    private readonly DbSet<CartReadModel> _carts;
    private readonly DbSet<CartProductLine> _cartProducts;

    public ShoppingCartProjection(ReadModelContext<SampleReadModelStore> db)
    {
        _db = db;
        _carts = db.Set<CartReadModel>();
        _cartProducts = db.Set<CartProductLine>();
    }

    public async Task HandleAsync(CartItemAdded @event, CancellationToken cancellationToken = default)
    {
        var id = SampleConfig.CartStreamId;
        var cart = await _carts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cart is null)
        {
            cart = new CartReadModel { Id = id, TotalItems = @event.Quantity };
            await _carts.AddAsync(cart, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            cart.TotalItems += @event.Quantity;
        }

        var line = await _cartProducts.FirstOrDefaultAsync(x => x.ProductId == @event.ProductId, cancellationToken);
        if (line is null)
        {
            line = new CartProductLine { ProductId = @event.ProductId, Quantity = @event.Quantity };
            await _cartProducts.AddAsync(line, cancellationToken).ConfigureAwait(false);
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
        var cart = await _carts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cart is null)
        {
            cart = new CartReadModel { Id = id, TotalItems = 0 };
            await _carts.AddAsync(cart, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            cart.TotalItems = Math.Max(0, cart.TotalItems - @event.Quantity);
        }

        var line = await _cartProducts.FirstOrDefaultAsync(x => x.ProductId == @event.ProductId, cancellationToken);
        if (line is null)
        {
            line = new CartProductLine { ProductId = @event.ProductId, Quantity = 0 };
            await _cartProducts.AddAsync(line, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            line.Quantity = Math.Max(0, line.Quantity - @event.Quantity);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
