using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander
{
    public class CommanderConfig
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string ApiKey { get; set; }

        public int Delay { get; set; } = 500;
        public string EndPoint => this.Address + ":" + this.Port;

        public string Session { get; private set; }

        public CommanderConfig()
        {
            this.Session = Guid.NewGuid().ToString();
        }
    }
}
