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
        public static CommModule CreateCommunicator(ConnexionUrl conn)
        {
            if (!conn.IsValid)
                return null;
            switch (conn.Protocol)
            {
                case ConnexionType.Http:
                    return new HttpCommModule(conn, ServiceProvider.GetService<IMessageService>(), ServiceProvider.GetService<IFileService>(), ServiceProvider.GetService<IProxyService>());
                case ConnexionType.Tcp:
                    if (conn.IsSecure)
                        return null;
                    else
                        return new TcpCommModule(conn, ServiceProvider.GetService<IMessageService>(), ServiceProvider.GetService<IFileService>(), ServiceProvider.GetService<IProxyService>());
            }
            return null;
        }
    }
}
