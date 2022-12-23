
//    using System;
//    using System.IO;
//    using System.IO.Pipes;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//    using System.Threading;
//    using System.Threading.Tasks;

//namespace PipeServer
//{
//    public class CryptoPipeServer : PipeServer
//    {
//        public CryptoPipeServer(string pipeName) : base(pipeName)
//        {
//        }

//        protected override async Task Listener()
//        {
//            using (NamedPipeServerStream server = new NamedPipeServerStream(this.PipeName, PipeDirection.InOut, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
//            {
//                //Console.WriteLine("\r\n[thread: {0}] -> Waiting for client.", Thread.CurrentThread.ManagedThreadId);

//                await Task.Factory.FromAsync(server.BeginWaitForConnection, server.EndWaitForConnection, null);

//                //Console.WriteLine("[thread: {0}] -> Client connected.", Thread.CurrentThread.ManagedThreadId);


//                // Create a new RSA crypto provider
//                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
//                rsa.KeySize = 2048; // Set the key size to 2048 bits
//                rsa.ExportParameters(true); // Export the private key

//                //Console.WriteLine("Public key: {0}", rsa.ToXmlString(false));
//                //Console.WriteLine("Private key: {0}", rsa.ToXmlString(true));



//                var reader = new StreamReader(server);
//                var writer = new StreamWriter(server);

//                //Sends the public key
//                writer.WriteLine(rsa.ToXmlString(false));
//                writer.Flush();

//                //Read the encoded generated symetric key
//                var b64EncryptedKey = reader.ReadLine();
//                var receivedKey = Convert.FromBase64String(b64EncryptedKey);
//                byte[] decryptedKey = rsa.Decrypt(receivedKey, false);

//                //Read the encoded generated symetric IV
//                var b64EncryptedIv = reader.ReadLine();
//                var receivedIv = Convert.FromBase64String(b64EncryptedIv);
//                byte[] decryptedIv = rsa.Decrypt(receivedIv, false);

//                // Console.WriteLine($"B64 Key = {Convert.ToBase64String(decryptedKey)}");

//                //Console.WriteLine("KEY = " + string.Join(",", decryptedKey.Select(i => i.ToString())));
//                //Console.WriteLine("IV = " + string.Join(",", decryptedIv.Select(i => i.ToString())));

//                RijndaelManaged rijndael = new RijndaelManaged();
//                rijndael.KeySize = 256;
//                rijndael.BlockSize = 128;
//                rijndael.Key = decryptedKey;
//                rijndael.IV = decryptedIv;
//                rijndael.Padding = PaddingMode.Zeros;

                

//                this.EncryptAndSendMessage(server, rijndael, "Agent Tasks");

//                Console.WriteLine(this.ReceiveMessageAndDecrypt(server, rijndael));


//                if (server.IsConnected)
//                    server.Disconnect();
//            }

            
//        }
//        private void EncryptAndSendMessage(NamedPipeServerStream server, RijndaelManaged rijndael, string message)
//        {
//            var messageBytes = Encoding.UTF8.GetBytes(message);
//            //Console.WriteLine($"messageBytes = " + string.Join(",", messageBytes.Select(i => i.ToString())));
//            var encryptedMessageBytes = rijndael.CreateEncryptor().TransformFinalBlock(messageBytes, 0, messageBytes.Length);
//            var b64EncryptedMessage = Convert.ToBase64String(encryptedMessageBytes);
//            var writer = new StreamWriter(server);
//            //Console.WriteLine($"B64 Key = {Convert.ToBase64String(rijndael.Key)}");
//            //Console.WriteLine($"B64 Sent Message = {b64EncryptedMessage}");
//            writer.WriteLine(b64EncryptedMessage);
//            writer.Flush();
//        }

//        private string ReceiveMessageAndDecrypt(NamedPipeServerStream server, RijndaelManaged rijndael)
//        {
//            var reader = new StreamReader(server);
//            var b64EncryptedMessage = reader.ReadLine();
//            //Console.WriteLine($"B64 Key = {Convert.ToBase64String(rijndael.Key)}");
//            //Console.WriteLine($"B64 Received Message = {b64EncryptedMessage}");
//            var encryptedMessageBytes = Convert.FromBase64String(b64EncryptedMessage);
//            byte[] decryptedBytes = rijndael.CreateDecryptor().TransformFinalBlock(encryptedMessageBytes, 0, encryptedMessageBytes.Length);
//            //Console.WriteLine($"decryptedBytes = " + string.Join(",", decryptedBytes.Select(i => i.ToString())));
//            string decrypted = Encoding.UTF8.GetString(decryptedBytes);

//            return decrypted;
//        }
//    }
//}