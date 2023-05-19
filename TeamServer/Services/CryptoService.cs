using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Linq;
using System;

namespace TeamServer.Services;

public interface ICryptoService
{
    public string Key { get; }
    byte[] Encrypt(byte[] src);
    byte[] Encrypt(string src);
    byte[] Decrypt(byte[] src);
    string DecryptAsString(byte[] src);
    byte[] Decrypt(string src);
    string DecryptAsString(string src);
    string DecryptFromBase64(string src);
    string EncryptAsBase64(string src);

}

public class CryptoService : ICryptoService
{
    private readonly IConfiguration _configService;

    public string Key { get; private set; }
    private RijndaelManaged rijndael;
    public CryptoService(IConfiguration configService)
    {
        _configService = configService;
        
        Key = configService.GetValue<string>("ServerKey");

        //Logger.Log($"Usgin ServerKey = {Key}");

        var keyBytes = System.Text.Encoding.UTF8.GetBytes(Key);

        byte[] key = keyBytes.Take(32).ToArray();
        byte[] iv = keyBytes.Take(16).ToArray();

        rijndael = new RijndaelManaged();
        rijndael.KeySize = 256;
        rijndael.BlockSize = 128;
        rijndael.Key = key;
        rijndael.IV = iv;
        rijndael.Padding = PaddingMode.PKCS7;
    }

    public byte[] Encrypt(byte[] src)
    {
        return rijndael.CreateEncryptor().TransformFinalBlock(src, 0, src.Length);
    }

    public byte[] Encrypt(string src)
    {
        return Encrypt(System.Text.Encoding.UTF8.GetBytes(src));
    }

    public byte[] Decrypt(byte[] src)
    {
        return rijndael.CreateDecryptor().TransformFinalBlock(src, 0, src.Length);
    }

    public string DecryptAsString(byte[] src)
    {
        return System.Text.Encoding.UTF8.GetString(Decrypt(src));
    }

    public byte[] Decrypt(string src)
    {
        return Decrypt(System.Text.Encoding.UTF8.GetBytes(src));
    }

    public string DecryptAsString(string src)
    {
        return DecryptAsString(System.Text.Encoding.UTF8.GetBytes(src));
    }

    public string DecryptFromBase64(string src)
    {
        return this.DecryptAsString(Convert.FromBase64String(src));
    }

    public string EncryptAsBase64(string src)
    {
        return Convert.ToBase64String(this.Encrypt(src));
    }
}