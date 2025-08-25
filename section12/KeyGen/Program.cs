using System.Security.Cryptography;

var resKey = RSA.Create();
var privateKey = resKey.ExportRSAPrivateKey();
File.WriteAllBytes("../Server/key", privateKey);