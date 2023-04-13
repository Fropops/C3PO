using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Communication
{
    public static class CommunicationFactory
    {
        public static CommModule CreateCommunicator(ConnexionUrl conn,string serverKey)
        {
            if (!conn.IsValid)
                return null;
            switch (conn.Protocol)
            {
                case ConnexionType.Http:
                    return new HttpCommModule(conn, serverKey, ServiceProvider.GetService<IMessageService>(), ServiceProvider.GetService<IFileService>(), ServiceProvider.GetService<IProxyService>());
                case ConnexionType.Tcp:
                    return new TcpCommModule(conn, serverKey, ServiceProvider.GetService<IMessageService>(), ServiceProvider.GetService<IFileService>(), ServiceProvider.GetService<IProxyService>());
                case ConnexionType.NamedPipe:
                    return new PipeCommModule(conn, serverKey, ServiceProvider.GetService<IMessageService>(), ServiceProvider.GetService<IFileService>(), ServiceProvider.GetService<IProxyService>());
            }
            return null;
        }
    }
}
