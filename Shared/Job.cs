using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared
{
    public class Job
    {
        [FieldOrder(0)]
        public string Name { get; set; }
        [FieldOrder(1)]
        public int Id { get; set; }
        [FieldOrder(2)]
        public int ProcessId { get; set; }

        public Job(int id, int processId, string name)
        {
            this.Id = id;
            this.Name = name;
            this.ProcessId = processId;
        }

        public Job()
        {

        }
    }
}
