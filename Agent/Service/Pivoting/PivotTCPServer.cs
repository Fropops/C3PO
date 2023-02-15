using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Communication;
using Agent.Helpers;
using Agent.Models;

namespace Agent.Service.Pivoting
{
    public class PivotTCPServer : PivotServer
    {

        public PivotTCPServer(ConnexionUrl conn) : base(conn)
        {
        }


        public override async Task Start()
        {
            try
            {
                this.Status = RunningService.RunningStatus.Running;

                var listener = new TcpListener(IPAddress.Parse(Connexion.Address), this.Connexion.Port);
                listener.Start(100);

                while (!_tokenSource.IsCancellationRequested)
                {
                    // this blocks until a connection is received or token is cancelled
                    var client = await listener.AcceptTcpClientAsync(_tokenSource);

                    // do something with the connected client
                    var thread = new Thread(async () => await HandleClient(client));
                    thread.Start();
                }
                // handle client in new thread

                listener.Stop();
            }
            finally
            {
                this.Status = RunningService.RunningStatus.Stoped;
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            if (client == null)
                return;

            var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
            var id = endpoint.Address + ":" + endpoint.Port;

            try
            {
                while (!_tokenSource.IsCancellationRequested && client.IsAlive())
                {

                    // read from client
                    if (!client.DataAvailable())
                        continue;

                    if (this.Connexion.IsSecure)
                        HandleSecureClient(client);
                    else
                        HandleNonSecureClient(client);
                }

                // sos cpu
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }
            finally
            {
                Debug.WriteLine($"TCP Pivot {id} : disconnected");
            }
        }

        private void HandleNonSecureClient(TcpClient client)
        {
            var req = client.ReceivedData();
            //Convert.FromBase64String(b64results).Deserialize<List<MessageResult>>();
            var responses = req.Deserialize<List<MessageResult>>();
            _messageService.EnqueueResults(responses);

            var relays = this.ExtractRelays(responses);

            Debug.WriteLine($"TCP Pivot {Connexion.ToString()} Sending task to Relays {string.Join(",", relays)}");
            var tasks = this._messageService.GetMessageTasksToRelay(relays);
            client.SendMessage(tasks.Serialize());
        }

        private void HandleSecureClient(TcpClient client)
        {
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

            Debug.WriteLine($"TCPS Pivot : EncryptedKeyIV = {encryptedKeyIV.Length} " + string.Join(",", encryptedKeyIV.Select(a => ((int)a).ToString())));
            client.SendMessage(encryptedKeyIV);

            var req = client.ReceivedData();
            //Debug.WriteLine("TCPS Pivot : Encrypted Message (read " + req.Length + ") = " + string.Join(",", req.Select(a => ((int)a).ToString())));
            byte[] decryptedBytes = rijndael.CreateDecryptor().TransformFinalBlock(req, 0, req.Length);
            var responses = decryptedBytes.Deserialize<List<MessageResult>>();
            _messageService.EnqueueResults(responses);

            var relays = this.ExtractRelays(responses);

            Debug.WriteLine($"TCPS Pivot {Connexion.ToString()} Sending task to Relays {string.Join(",", relays)}");
            var tasks = this._messageService.GetMessageTasksToRelay(relays);

            var meesage = tasks.Serialize();
            var encryptedMessageBytes = rijndael.CreateEncryptor().TransformFinalBlock(meesage, 0, meesage.Length);
            client.SendMessage(encryptedMessageBytes);
        }
    }
}
