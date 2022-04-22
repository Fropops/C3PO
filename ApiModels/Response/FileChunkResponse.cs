using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Response
{
    public class FileChunckResponse
    {
        public string FileId { get; set; }
        public int Index { get; set; }
        public string Data { get; set; }
    }
}
