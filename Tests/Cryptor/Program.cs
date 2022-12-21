using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cryptor
{
    class Program
    {

        public static string[] SplitIntoChunks(string input, int chunkSize)
        {
            int stringLength = input.Length;

            // Calculate the number of chunks we will need.
            int chunkCount = stringLength / chunkSize;
            if (stringLength % chunkSize > 0)
            {
                chunkCount++;
            }

            // Initialize the array to hold the chunks.
            string[] chunks = new string[chunkCount];

            // Split the input string into the array.
            for (int i = 0; i < chunkCount; i++)
            {
                int startIndex = i * chunkSize;
                int length = Math.Min(chunkSize, stringLength - startIndex);
                chunks[i] = input.Substring(startIndex, length);
            }

            return chunks;
        }
        static void Main(string[] args)
        {
            string b64 = string.Empty;
            using (var reader = new StreamReader(File.OpenRead(@"e:\Share\tmp\C2\Server\Listener\Local\Agent.b64")))
            {
                b64 = reader.ReadToEnd();
            }

            using (var writer = new StreamWriter(File.OpenWrite(@"e:\Share\tmp\C2\Server\Listener\Local\Agent.b64.split")))
                foreach (var chunk in SplitIntoChunks(b64, 100))
                {
                    writer.WriteLine("b64 = b64 & \"" + chunk + "\"");
                    writer.Flush();
                }



            /*string data = "Hello, World!";



            StringBuilder sb = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 32; i++)
                {
                    // Génération d'un nombre aléatoire compris entre 0 et 9 ou entre A et Z
                    byte[] randomByte = new byte[1];
                    rng.GetBytes(randomByte);
                    char randomChar = (char)(randomByte[0] % (10 + 26));
                    if (randomChar < 10)
                    {
                        randomChar = (char)(randomChar + '0');
                    }
                    else
                    {
                        randomChar = (char)(randomChar - 10 + 'A');
                    }
                    sb.Append(randomChar);
                }
            }

            // Récupération de la chaîne aléatoire
            string randomString = sb.ToString();
            Console.WriteLine(randomString);



            // La clef symétrique
            byte[] key = Encoding.UTF8.GetBytes(randomString);

            // Création d'un objet de type Aes
            using (Aes aes = Aes.Create())
            {
                // Initialisation de l'objet avec la clef et la longueur de clef 256 bits
                aes.KeySize = 256;
                aes.Key = key;

                // Création d'un objet de type ICryptoTransform pour l'encodage
                ICryptoTransform encryptor = aes.CreateEncryptor();

                // Transformation de la chaîne en tableau d'octets
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                // Encodage de la chaîne en base64
                string encodedData = Convert.ToBase64String(encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length));

                Console.WriteLine(encodedData);
            }*/
        }
    }
}
