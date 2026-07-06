using System.Security.Cryptography;
using System.Text;
using FamilySuper.Core.Interfaces;

namespace FamilySuper.Infrastructure;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(string? key = null)
    {
        var keyBytes = string.IsNullOrEmpty(key)
            ? SHA256.HashData(Encoding.UTF8.GetBytes("FamilySuper-Default-Key-Change-In-Production"))
            : SHA256.HashData(Encoding.UTF8.GetBytes(key));
        _key = keyBytes;
        _iv = MD5.HashData(keyBytes)[..16];
    }

    public string Encrypt(string plainText)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = EncryptBytes(plainBytes);
        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = DecryptBytes(cipherBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public byte[] EncryptBytes(byte[] plainBytes)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
    }

    public byte[] DecryptBytes(byte[] cipherBytes)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(hash)) return false;
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
