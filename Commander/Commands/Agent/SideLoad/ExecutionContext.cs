using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.SideLoad
{
    public class ExecutionContext
    {
        //TaskId
        public string id { get; set; }
        //ServerIp
        public string i { get; set; }
        //ServerPort
        public int p { get; set; }
        //ssl
        public string s { get; set; }
        //Parameters
        public string a { get; set; }
    }
}
