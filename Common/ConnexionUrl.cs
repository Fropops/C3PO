﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ConnexionUrl
    {
        public ConnexionType Protocol { get; set; }

        public string ProtocolString
        {
            get
            {
                var str = this.Protocol.ToString().ToLower();
                if (this.IsSecure)
                    str += "s";
                return str;
            }
        }
        public string Address { get; set; }
        public int Port { get; set; }

        public string PipeName { get; set; }
        public bool IsSecure { get; set; }

        public bool IsValid { get; set; }

        public static ConnexionUrl FromString(string connStr)
        {
            ConnexionUrl conn = new ConnexionUrl();
            try
            {
                var sep = new List<string> { "://" }.ToArray();
                var tab = connStr.Split(sep, StringSplitOptions.None);

                var protocol = tab[0].ToLower();
                var part = tab[1];

                var parmTab = part.Split(':');
                var address = parmTab[0];

                var complement = string.Empty;
                if (parmTab.Length > 2)
                    return conn;

                if (parmTab.Length > 1)
                    complement = parmTab[1];

                if (protocol == "http")
                {
                    conn.Protocol = ConnexionType.Http;
                    conn.IsSecure = false;
                    conn.Address = address;
                    conn.Port = string.IsNullOrEmpty(complement.Trim()) ? 80 : int.Parse(complement);
                    conn.IsValid = true;
                    return conn;
                }

                if (protocol == "https")
                {
                    conn.Protocol = ConnexionType.Http;
                    conn.IsSecure = true;
                    conn.Address = address;
                    conn.Port = string.IsNullOrEmpty(complement.Trim()) ? 443 : int.Parse(complement);
                    conn.IsValid = true;
                    return conn;
                }

                if (protocol == "tcp")
                {
                    conn.Protocol = ConnexionType.Tcp;
                    conn.IsSecure = false;
                    conn.Address = address;
                    conn.Port = int.Parse(complement);
                    conn.IsValid = true;
                    return conn;
                }

                if (protocol == "pipe")
                {
                    conn.Protocol = ConnexionType.NamedPipe;
                    conn.IsSecure = false;
                    conn.Address = address;
                    conn.PipeName = complement.Trim();
                    conn.IsValid = !string.IsNullOrEmpty(complement.Trim());
                    return conn;
                }
            }
            catch
            {

            }
            return conn;
        }

        public override string ToString()
        {
            switch (this.Protocol)
            {
                case ConnexionType.Http:
                    {
                        var prot = "http";
                        if (this.IsSecure)
                            prot += "s";
                        return $"{prot}://{this.Address}:{this.Port}";
                    }
                case ConnexionType.NamedPipe:
                    {
                        var prot = "pipe";
                        return $"{prot}://{this.Address}:{this.PipeName}";
                    }
                    break;
                case ConnexionType.Tcp:
                    {
                        var prot = "tcp";
                        return $"{prot}://{this.Address}:{this.Port}";
                    }
            }
            return string.Empty;
        }
    }

    public enum ConnexionType
    {
        Http,
        Tcp,
        NamedPipe,
    }
}
