using Microsoft.EntityFrameworkCore;
using Quasar.EventSourcing.Abstractions;
using Quasar.Projections.Abstractions;

namespace Quasar.Identity.Persistence.Relational.EfCore;

public sealed class IdentityProjections :
    IProjection<UserRegistered>,
    IProjection<UserPasswordSet>
{
    private readonly IdentityReadModelContext _db;
    public IdentityProjections(IdentityReadModelContext db) => _db = db;

    public async Task HandleAsync(UserRegistered @event, CancellationToken cancellationToken = default)
    {
        var exists = await _db.Users.AnyAsync(x => x.Id == @event.UserId, cancellationToken);
        if (!exists)
        {
            _db.Users.Add(new IdentityUserReadModel
            {
                Id = @event.UserId,
                Username = @event.Username,
                Email = @event.Email
            });
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleAsync(UserPasswordSet @event, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == @event.UserId, cancellationToken);
        if (user is null)
        {
            user = new IdentityUserReadModel { Id = @event.UserId };
            _db.Users.Add(user);
        }
        user.PasswordHash = @event.PasswordHash;
        user.PasswordSalt = @event.PasswordSalt;
        await _db.SaveChangesAsync(cancellationToken);
    }
}

