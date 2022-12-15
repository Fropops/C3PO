using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography;

namespace PipeClient
{


    class Program
    {
        //Crypto
        static void Main(string[] args)
        {
            var rand = new Random();
            int id = rand.Next(10000);
            Console.WriteLine("Named Pipe Client");

            // Connect to the server using a unique pipe name
            var pipeClient = new NamedPipeClientStream("192.168.56.1", "Test", PipeDirection.InOut);
            pipeClient.Connect();

            // Send a message to the server
            var writer = new StreamWriter(pipeClient);
            var reader = new StreamReader(pipeClient);

            Console.WriteLine("waiting public key");
            string xmlPubKey = reader.ReadLine();

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPubKey);

            //Generate symmetric Key
            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.KeySize = 256; // Set the key size to 256 bits
            rijndael.BlockSize = 128;
            rijndael.GenerateKey(); // Generate a random key
            rijndael.Padding = PaddingMode.Zeros;

            Console.WriteLine("KEY = " + string.Join(",", rijndael.Key.Select(i => i.ToString())));
            Console.WriteLine("IV = " + string.Join(",", rijndael.IV.Select(i => i.ToString())));
            //Console.WriteLine(" = " + string.Join(",", rijndael..Select(i => i.ToString())));

            //Console.WriteLine($"B64 Key = {Convert.ToBase64String(rijndael.Key)}");

            // Encrypt the symmetric key using the public key
            byte[] encryptedKey = rsa.Encrypt(rijndael.Key, false);
            var b64EncryptedKey = Convert.ToBase64String(encryptedKey);

            byte[] encryptedIV = rsa.Encrypt(rijndael.IV, false);
            var b64EncryptedIV = Convert.ToBase64String(encryptedIV);

            //Console.WriteLine("Sending symetric key");
            writer.WriteLine(b64EncryptedKey);
            writer.Flush();

            //Console.WriteLine("Sending symetric IV");
            writer.WriteLine(b64EncryptedIV);
            writer.Flush();

            Console.WriteLine("Receiving message:");
            Console.WriteLine(ReceiveMessageAndDecrypt(pipeClient, rijndael));
            Console.WriteLine("Sending message.");
            EncryptAndSendMessage(pipeClient, rijndael, $"Agent Results {id}");

            // Close the client
            pipeClient.Close();
        }

        private static void EncryptAndSendMessage(NamedPipeClientStream stream, RijndaelManaged rijndael, string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            //Console.WriteLine("messageBytes = " + string.Join(",", messageBytes.Select(i => i.ToString())));
            var encryptedMessageBytes = rijndael.CreateEncryptor().TransformFinalBlock(messageBytes, 0, messageBytes.Length);
            var b64EncryptedMessage = Convert.ToBase64String(encryptedMessageBytes);
            //Console.WriteLine($"B64 Key = {Convert.ToBase64String(rijndael.Key)}");
            //Console.WriteLine($"B64 Sent Message = {b64EncryptedMessage}");
            
            var writer = new StreamWriter(stream);
            writer.WriteLine(b64EncryptedMessage);
            writer.Flush();
        }

        private static string ReceiveMessageAndDecrypt(NamedPipeClientStream stream, RijndaelManaged rijndael)
        {
            var reader = new StreamReader(stream);
            var b64EncryptedMessage = reader.ReadLine();
            //Console.WriteLine($"B64 Key = {Convert.ToBase64String(rijndael.Key)}");
            //Console.WriteLine($"B64 Received Message = {b64EncryptedMessage}");
            var encryptedMessageBytes = Convert.FromBase64String(b64EncryptedMessage);
            byte[] decryptedBytes = rijndael.CreateDecryptor().TransformFinalBlock(encryptedMessageBytes, 0, encryptedMessageBytes.Length);
            //Console.WriteLine($"decryptedBytes = " + string.Join(",", decryptedBytes.Select(i => i.ToString())));
            string decrypted = Encoding.UTF8.GetString(decryptedBytes);

            return decrypted;
        }

        //Simple
        //static void Main(string[] args)
        //{
        //    var rand = new Random();
        //    int id = rand.Next(10000);
        //    Console.WriteLine("Named Pipe Client");

        //    // Connect to the server using a unique pipe name
        //    var pipeClient = new NamedPipeClientStream("192.168.56.1", "Test", PipeDirection.InOut);
        //    pipeClient.Connect();

        //    // Send a message to the server
        //    var writer = new StreamWriter(pipeClient);
        //    writer.WriteLine($"Hello from the client {id}!");
        //    writer.Flush();

        //    var reader = new StreamReader(pipeClient);
        //    Console.WriteLine(reader.ReadLine());

        //    // Close the client
        //    pipeClient.Close();
        //}
    }
}
