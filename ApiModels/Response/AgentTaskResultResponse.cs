using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Response
{
    public class AgentTaskResultResponse
    {
        public string Id { get; set; }
        public string Result { get; set; }
        public string Info { get; set; }
        public int Status { get; set; }

        public List<TaskFileResult> Files { get; set; } = new List<TaskFileResult>();

    }

    public class TaskFileResult
    {
        public string FileId { get; set; }
        public string FileName { get; set; }
        public bool IsDownloaded { get; set; }
    }
}
