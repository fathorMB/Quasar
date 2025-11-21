using System.Security.Cryptography;

namespace Quasar.Security;

/// <summary>
/// Generates cryptographically secure random passwords.
/// </summary>
public static class PasswordGenerator
{
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%^&*()_-+=<>?";
    private const string All = Lowercase + Uppercase + Digits + Symbols;

    /// <summary>
    /// Generates a cryptographically secure random password.
    /// </summary>
    /// <param name="length">Length of the password (minimum 8 characters).</param>
    /// <param name="includeSymbols">Whether to include special symbols.</param>
    /// <returns>A secure random password.</returns>
    public static string Generate(int length = 16, bool includeSymbols = true)
    {
        if (length < 8)
        {
            throw new ArgumentException("Password length must be at least 8 characters", nameof(length));
        }

        var chars = includeSymbols ? All : Lowercase + Uppercase + Digits;
        var password = new char[length];

        // Ensure at least one of each required character type
        password[0] = Lowercase[RandomNumberGenerator.GetInt32(Lowercase.Length)];
        password[1] = Uppercase[RandomNumberGenerator.GetInt32(Uppercase.Length)];
        password[2] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];

        if (includeSymbols)
        {
            password[3] = Symbols[RandomNumberGenerator.GetInt32(Symbols.Length)];
        }

        // Fill remaining positions with random characters
        for (int i = (includeSymbols ? 4 : 3); i < length; i++)
        {
            password[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }

        // Shuffle to avoid predictable patterns
        Shuffle(password);

        return new string(password);
    }

    private static void Shuffle(char[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
