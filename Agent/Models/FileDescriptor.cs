using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class FileDescriptor
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public int ChunkCount { get; set; }
    }
}
