namespace FamilySuper.Core.Interfaces;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    byte[] EncryptBytes(byte[] plainBytes);
    byte[] DecryptBytes(byte[] cipherBytes);
}
