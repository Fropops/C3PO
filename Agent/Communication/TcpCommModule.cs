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

namespace Agent.Communication
{
    public class TcpCommModule : CommModule
    {
        public TcpCommModule(ConnexionUrl conn, IMessageService messManager, IFileService fileService, IProxyService proxyService) : base(conn, messManager, fileService, proxyService)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new
            RemoteCertificateValidationCallback
            (
               delegate
               {
                   return true;
               }
            );

           
        }


        protected override async Task<List<MessageTask>> ChekIn(List<MessageResult> results)
        {
            var client = new TcpClient(this.Connexion.Address, this.Connexion.Port);
            if (!client.IsAlive())
                return null;

            client.SendData(results.Serialize());
            
            var responseContent = client.ReceivedMessage(true);
            return responseContent.Deserialize<List<MessageTask>>();
        }

    }
}
