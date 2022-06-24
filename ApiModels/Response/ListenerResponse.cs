using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Response
{
    public class ListenerResponse
    {
        public string Id { get; set; }
        public bool Secured { get; set; }
        public string Name { get; set; }
        public int BindPort { get; set; }
        public int PublicPort { get; set; }
        public string Ip { get; set; }
    }
}
