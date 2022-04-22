using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Response
{
    public class FileDescriptorResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Length { get; set; }
        public int ChunkSize { get; set; }
        public int ChunkCount { get; set; }
    }
}
