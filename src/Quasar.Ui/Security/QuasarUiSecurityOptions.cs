using System;

namespace Quasar.Ui.Security;

public sealed class QuasarUiSecurityOptions
{
    private bool _requireAuthentication;

    public bool AutoDetectAuthentication { get; set; } = true;

    public TimeSpan SessionDuration { get; set; } = TimeSpan.FromHours(8);

    public bool RequireAuthentication
    {
        get => _requireAuthentication;
        set
        {
            _requireAuthentication = value;
            RequireAuthenticationExplicitlySet = true;
        }
    }

    internal bool RequireAuthenticationExplicitlySet { get; private set; }
}
