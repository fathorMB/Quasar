using Quasar.Cqrs;
using Quasar.Domain;
using Quasar.EventSourcing.Abstractions;
using System.Security.Cryptography;

namespace Quasar.Identity;

public sealed record UserRegistered(Guid UserId, string Username, string Email) : IEvent;
public sealed record UserPasswordSet(Guid UserId, string PasswordHash, string PasswordSalt) : IEvent;

public sealed class UserAggregate : AggregateRoot
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public void Register(Guid id, string username, string email)
    {
        Id = id;
        ApplyChange(new UserRegistered(id, username, email));
    }

    public void SetPassword(string passwordHash, string salt)
    {
        ApplyChange(new UserPasswordSet(Id, passwordHash, salt));
    }

    protected override void When(IDomainEvent @event)
    {
        switch (@event)
        {
            case UserRegistered e:
                Id = e.UserId;
                Username = e.Username;
                Email = e.Email;
                break;
        }
    }
}

public sealed record RegisterUserCommand(string Username, string Email, string Password) : ICommand<Guid>;

public interface IPasswordHasher
{
    (string hash, string salt) Hash(string password);
    bool Verify(string password, string hash, string salt);
}

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    public (string hash, string salt) Hash(string password)
    {
        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var hash = Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(salt), 100_000, HashAlgorithmName.SHA256, 32));
        return (hash, salt);
    }

    public bool Verify(string password, string hash, string salt)
    {
        var check = Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(salt), 100_000, HashAlgorithmName.SHA256, 32));
        return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(hash), Convert.FromBase64String(check));
    }
}

public sealed class RegisterUserHandler : ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly IEventSourcedRepository<UserAggregate> _repo;
    private readonly IPasswordHasher _hasher;
    public RegisterUserHandler(IEventSourcedRepository<UserAggregate> repo, IPasswordHasher hasher)
    { _repo = repo; _hasher = hasher; }

    public async Task<Guid> Handle(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = new UserAggregate();
        var id = Guid.NewGuid();
        user.Register(id, command.Username, command.Email);
        var (hash, salt) = _hasher.Hash(command.Password);
        user.SetPassword(hash, salt);
        await _repo.SaveAsync(user, cancellationToken);
        return id;
    }
}

