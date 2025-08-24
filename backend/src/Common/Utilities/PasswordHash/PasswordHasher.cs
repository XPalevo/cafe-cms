namespace Common.Utilities.PasswordHash;

using Common.Utilities.PasswordHash;
using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

public class PasswordHasher :  IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int DegreeOfParallelism = 2;
    private const int Iterations = 2;
    private const int MemorySize = 64 * 1024;
    
    private byte[] ComputeHash(string password, byte[] salt)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            Iterations = Iterations,
            MemorySize = MemorySize
        };

        return argon2.GetBytes(HashSize);
    }

    public string HashPassword(string password)
    {
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        byte[] hash = ComputeHash(password, salt);

        var combinedBytes = new byte[salt.Length + hash.Length];
        Array.Copy(salt, 0, combinedBytes, 0, salt.Length);
        Array.Copy(hash, 0, combinedBytes, salt.Length, hash.Length);

        return Convert.ToBase64String(combinedBytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        byte[] combinedBytes = Convert.FromBase64String(hashedPassword);

        byte[] salt = new byte[SaltSize];
        byte[] hash = new byte[HashSize];
        Array.Copy(combinedBytes, 0, salt, 0, SaltSize);
        Array.Copy(combinedBytes, SaltSize, hash, 0, HashSize);

        byte[] newHash = ComputeHash(password, salt);

        return CryptographicOperations.FixedTimeEquals(hash, newHash);
    }
}