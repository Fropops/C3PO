using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Models
{
    public class Listener
    {
        public string Protocol { get; set; }
        public string Name { get; set; }
        public int BindPort { get; set; }
        public int PublicPort { get; set; }
    }
}
