using Quasar.EventSourcing.Abstractions;

namespace Quasar.Identity;

/// <summary>
/// Provides the event type map for Quasar Identity events.
/// </summary>
public static class IdentityEventTypeMap
{
    /// <summary>
    /// Creates an event type map containing all Quasar Identity domain events.
    /// </summary>
    public static IEventTypeMap Create()
    {
        return new DictionaryEventTypeMap(new[]
        {
            ("identity.user_registered", typeof(UserRegistered)),
            ("identity.user_password_set", typeof(UserPasswordSet)),
            ("identity.user_role_assigned", typeof(UserRoleAssigned)),
            ("identity.user_role_revoked", typeof(UserRoleRevoked)),
            ("identity.role_created", typeof(RoleCreated)),
            ("identity.role_renamed", typeof(RoleRenamed)),
            ("identity.role_permission_granted", typeof(RolePermissionGranted)),
            ("identity.role_permission_revoked", typeof(RolePermissionRevoked)),
            ("identity.user_deleted", typeof(UserDeleted)),
            ("identity.role_deleted", typeof(RoleDeleted))
        });
    }
}
