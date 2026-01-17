namespace NRGTycoon.App;

/// <summary>
/// Options for seeding the default admin user.
/// </summary>
public sealed class AdminSeedOptions
{
    public string Username { get; set; } = "admin";
    public string Email { get; set; } = "admin@nrgtycoon.local";
    public string Password { get; set; } = "ChangeMe123!";
    public string RoleName { get; set; } = "administrator";
    public IList<string> Permissions { get; set; } = new List<string> { "identity.manage", "game.admin" };
}

/// <summary>
/// Request to create a new company.
/// </summary>
public record CreateCompanyRequest(string Name);

/// <summary>
/// Request to update company name.
/// </summary>
public record UpdateCompanyNameRequest(Guid CompanyId, string NewName);
