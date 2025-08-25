using System.Security.Cryptography;

namespace CookiesAndTokens;

public class KeyManager
{
    public KeyManager()
    {
        RSAKey = RSA.Create();

        if (File.Exists("./key"))
        {
            RSAKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);
        }
        else
        {
            var privateKey = RSAKey.ExportRSAPrivateKey();
            File.WriteAllBytes("key", privateKey);
        }
    }

    public RSA RSAKey { get; set; }
}