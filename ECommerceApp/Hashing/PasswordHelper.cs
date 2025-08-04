using Microsoft.AspNetCore.Identity;

public static class PasswordHelper
{
    public static readonly PasswordHasher<LoginDatabase> passwordHasher = new();

    public static string HashPassword(string password)
    {
        return passwordHasher.HashPassword(new LoginDatabase(), password);
    }
    
    public static bool VerifyPassword(string hashedPassword, string inputPassword)
    {
        var result = passwordHasher.VerifyHashedPassword(new LoginDatabase(), hashedPassword, inputPassword);
        return result == PasswordVerificationResult.Success;
    }
}