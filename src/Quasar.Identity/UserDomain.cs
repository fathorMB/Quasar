using System.Security.Cryptography;
using Quasar.Cqrs;
using Quasar.Domain;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.Identity;

public sealed record UserRegistered(Guid UserId, string Username, string Email) : IEvent;
public sealed record UserPasswordSet(Guid UserId, string PasswordHash, string PasswordSalt) : IEvent;
public sealed record UserRoleAssigned(Guid UserId, Guid RoleId) : IEvent;
public sealed record UserRoleRevoked(Guid UserId, Guid RoleId) : IEvent;
public sealed record UserDeleted(Guid UserId, DateTime DeletedAtUtc) : IEvent;

public sealed record RoleCreated(Guid RoleId, string Name) : IEvent;
public sealed record RoleRenamed(Guid RoleId, string Name) : IEvent;
public sealed record RolePermissionGranted(Guid RoleId, string Permission) : IEvent;
public sealed record RolePermissionRevoked(Guid RoleId, string Permission) : IEvent;
public sealed record RoleDeleted(Guid RoleId, DateTime DeletedAtUtc) : IEvent;

public sealed class UserAggregate : AggregateRoot
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    private readonly HashSet<Guid> _roles = new();

    public void Register(Guid id, string username, string email)
    {
        Id = id;
        ApplyChange(new UserRegistered(id, username, email));
    }

    public void SetPassword(string passwordHash, string salt)
    {
        ApplyChange(new UserPasswordSet(Id, passwordHash, salt));
    }

    public void AssignRole(Guid roleId)
    {
        if (_roles.Add(roleId))
            ApplyChange(new UserRoleAssigned(Id, roleId));
    }

    public void RevokeRole(Guid roleId)
    {
        if (_roles.Remove(roleId))
            ApplyChange(new UserRoleRevoked(Id, roleId));
    }

    public void Delete(DateTime deletedAtUtc)
    {
        ApplyChange(new UserDeleted(Id, deletedAtUtc));
    }

    private void When(UserRegistered e)
    {
        Id = e.UserId;
        Username = e.Username;
        Email = e.Email;
    }

    private void When(UserPasswordSet e) { }

    private void When(UserRoleAssigned e)
    {
        _roles.Add(e.RoleId);
    }

    private void When(UserRoleRevoked e)
    {
        _roles.Remove(e.RoleId);
    }

    private void When(UserDeleted e) { }
}

public sealed class RoleAggregate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);

    public void Create(Guid id, string name)
    {
        Id = id;
        ApplyChange(new RoleCreated(id, name));
    }

    public void Rename(string name)
    {
        if (!string.Equals(Name, name, StringComparison.Ordinal))
            ApplyChange(new RoleRenamed(Id, name));
    }

    public void GrantPermission(string permission)
    {
        if (_permissions.Add(permission))
            ApplyChange(new RolePermissionGranted(Id, permission));
    }

    public void RevokePermission(string permission)
    {
        if (_permissions.Remove(permission))
            ApplyChange(new RolePermissionRevoked(Id, permission));
    }

    public void Delete(DateTime deletedAtUtc)
    {
        ApplyChange(new RoleDeleted(Id, deletedAtUtc));
    }

    private void When(RoleCreated e)
    {
        Id = e.RoleId;
        Name = e.Name;
    }

    private void When(RoleRenamed e)
    {
        Name = e.Name;
    }

    private void When(RolePermissionGranted e)
    {
        _permissions.Add(e.Permission);
    }

    private void When(RolePermissionRevoked e)
    {
        _permissions.Remove(e.Permission);
    }

    private void When(RoleDeleted e) { }
}

public sealed record RegisterUserCommand(string Username, string Email, string Password) : ICommand<Guid>;
public sealed record ResetPasswordResult(string Password, string PasswordHash, string PasswordSalt);
public sealed record ResetUserPasswordCommand(Guid UserId) : ICommand<ResetPasswordResult>;
public sealed record AssignRoleToUserCommand(Guid UserId, Guid RoleId) : ICommand<bool>;
public sealed record RevokeRoleFromUserCommand(Guid UserId, Guid RoleId) : ICommand<bool>;
public sealed record DeleteUserCommand(Guid UserId) : ICommand<bool>;

public sealed record CreateRoleCommand(string Name) : ICommand<Guid>;
public sealed record RenameRoleCommand(Guid RoleId, string Name) : ICommand<bool>;
public sealed record GrantRolePermissionCommand(Guid RoleId, string Permission) : ICommand<bool>;
public sealed record RevokeRolePermissionCommand(Guid RoleId, string Permission) : ICommand<bool>;
public sealed record DeleteRoleCommand(Guid RoleId) : ICommand<bool>;

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
    {
        _repo = repo;
        _hasher = hasher;
    }

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

public sealed class ResetUserPasswordHandler : ICommandHandler<ResetUserPasswordCommand, ResetPasswordResult>
{
    private readonly IEventSourcedRepository<UserAggregate> _repo;
    private readonly IPasswordHasher _hasher;

    public ResetUserPasswordHandler(IEventSourcedRepository<UserAggregate> repo, IPasswordHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<ResetPasswordResult> Handle(ResetUserPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repo.GetAsync(command.UserId, cancellationToken);
        if (user.Id == Guid.Empty)
        {
            throw new InvalidOperationException($"User {command.UserId} not found");
        }

        var newPassword = Quasar.Security.PasswordGenerator.Generate(16, includeSymbols: true);
        var (hash, salt) = _hasher.Hash(newPassword);
        user.SetPassword(hash, salt);

        await _repo.SaveAsync(user, cancellationToken);
        return new ResetPasswordResult(newPassword, hash, salt);
    }
}

public sealed class AssignRoleToUserHandler : ICommandHandler<AssignRoleToUserCommand, bool>
{
    private readonly IEventSourcedRepository<UserAggregate> _users;

    public AssignRoleToUserHandler(IEventSourcedRepository<UserAggregate> users)
    {
        _users = users;
    }

    public async Task<bool> Handle(AssignRoleToUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetAsync(command.UserId, cancellationToken);
        if (user.Id == Guid.Empty) return false;

        user.AssignRole(command.RoleId);
        await _users.SaveAsync(user, cancellationToken);
        return true;
    }
}

public sealed class RevokeRoleFromUserHandler : ICommandHandler<RevokeRoleFromUserCommand, bool>
{
    private readonly IEventSourcedRepository<UserAggregate> _users;

    public RevokeRoleFromUserHandler(IEventSourcedRepository<UserAggregate> users)
    {
        _users = users;
    }

    public async Task<bool> Handle(RevokeRoleFromUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetAsync(command.UserId, cancellationToken);
        if (user.Id == Guid.Empty) return false;

        user.RevokeRole(command.RoleId);
        await _users.SaveAsync(user, cancellationToken);
        return true;
    }
}

public sealed class DeleteUserHandler : ICommandHandler<DeleteUserCommand, bool>
{
    private readonly IEventSourcedRepository<UserAggregate> _users;

    public DeleteUserHandler(IEventSourcedRepository<UserAggregate> users)
    {
        _users = users;
    }

    public async Task<bool> Handle(DeleteUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetAsync(command.UserId, cancellationToken);
        if (user.Id == Guid.Empty) return false;

        user.Delete(DateTime.UtcNow);
        await _users.SaveAsync(user, cancellationToken);
        return true;
    }
}

public sealed class CreateRoleHandler : ICommandHandler<CreateRoleCommand, Guid>
{
    private readonly IEventSourcedRepository<RoleAggregate> _repo;

    public CreateRoleHandler(IEventSourcedRepository<RoleAggregate> repo)
    {
        _repo = repo;
    }

    public async Task<Guid> Handle(CreateRoleCommand command, CancellationToken cancellationToken = default)
    {
        var role = new RoleAggregate();
        var id = Guid.NewGuid();
        role.Create(id, command.Name);

        await _repo.SaveAsync(role, cancellationToken);
        return id;
    }
}

public sealed class RenameRoleHandler : ICommandHandler<RenameRoleCommand, bool>
{
    private readonly IEventSourcedRepository<RoleAggregate> _repo;

    public RenameRoleHandler(IEventSourcedRepository<RoleAggregate> repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(RenameRoleCommand command, CancellationToken cancellationToken = default)
    {
        var role = await _repo.GetAsync(command.RoleId, cancellationToken);
        if (role.Id == Guid.Empty) return false;

        role.Rename(command.Name);
        await _repo.SaveAsync(role, cancellationToken);
        return true;
    }
}

public sealed class GrantRolePermissionHandler : ICommandHandler<GrantRolePermissionCommand, bool>
{
    private readonly IEventSourcedRepository<RoleAggregate> _repo;

    public GrantRolePermissionHandler(IEventSourcedRepository<RoleAggregate> repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(GrantRolePermissionCommand command, CancellationToken cancellationToken = default)
    {
        var role = await _repo.GetAsync(command.RoleId, cancellationToken);
        if (role.Id == Guid.Empty) return false;

        role.GrantPermission(command.Permission);
        await _repo.SaveAsync(role, cancellationToken);
        return true;
    }
}

public sealed class RevokeRolePermissionHandler : ICommandHandler<RevokeRolePermissionCommand, bool>
{
    private readonly IEventSourcedRepository<RoleAggregate> _repo;

    public RevokeRolePermissionHandler(IEventSourcedRepository<RoleAggregate> repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(RevokeRolePermissionCommand command, CancellationToken cancellationToken = default)
    {
        var role = await _repo.GetAsync(command.RoleId, cancellationToken);
        if (role.Id == Guid.Empty) return false;

        role.RevokePermission(command.Permission);
        await _repo.SaveAsync(role, cancellationToken);
        return true;
    }
}

public sealed class DeleteRoleHandler : ICommandHandler<DeleteRoleCommand, bool>
{
    private readonly IEventSourcedRepository<RoleAggregate> _repo;

    public DeleteRoleHandler(IEventSourcedRepository<RoleAggregate> repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(DeleteRoleCommand command, CancellationToken cancellationToken = default)
    {
        var role = await _repo.GetAsync(command.RoleId, cancellationToken);
        if (role.Id == Guid.Empty) return false;

        role.Delete(DateTime.UtcNow);
        await _repo.SaveAsync(role, cancellationToken);
        return true;
    }
}
