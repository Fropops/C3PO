using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Requests
{
    public class FileWebHost
    {
        public string FileName { get; set; }
        public byte[] Data { get; set; }
    }
}
