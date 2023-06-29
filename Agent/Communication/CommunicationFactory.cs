﻿using Agent.Models;
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
        internal static EgressCommunicator CreateEgressCommunicator(ConnexionUrl conn)
        {
            if (!conn.IsValid)
                return null;
            switch (conn.Protocol)
            {
                case ConnexionType.Http:
                    return new HttpCommmunicator(conn);
                //case ConnexionType.Tcp:
                //    return new TcpCommModule(conn);
                //case ConnexionType.NamedPipe:
                //    return new PipeCommModule(conn);
            }
            return null;
        }
    }
}
