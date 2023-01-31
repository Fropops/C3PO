using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Agent.Service.RunningService;

namespace Agent.Service
{
    public interface IRunningService
    {
        string ServiceName { get; }

        RunningStatus Status { get; set; }

        void Start();

        void Stop();
    }
}
