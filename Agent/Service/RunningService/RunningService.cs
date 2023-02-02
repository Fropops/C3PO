using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Service
{
    public abstract class RunningService : IRunningService
    {
        public abstract string ServiceName { get; }
        public enum RunningStatus
        {
            Running,
            Stoped
        }

        public RunningStatus Status { get; set; } = RunningStatus.Stoped;

        public int MinimumDelay { get; set; } = 10;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public virtual async void Start()
        {
            this.Status = RunningStatus.Running;
            while (!_tokenSource.IsCancellationRequested)
            {
                this.Process();
                await Task.Delay(this.MinimumDelay);
            }
            this.Status = RunningStatus.Stoped;
        }

        public virtual async void Stop()
        {
            if (this.Status != RunningStatus.Running)
                return;

            this._tokenSource.Cancel();
        }

        public virtual void Process()
        {

        }
    }
}
