using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipeServer
{
    public abstract class PipeServer
    {
        private readonly CancellationToken _cancel;

        private readonly CancellationTokenSource _cancelSource;

        public string PipeName { get; private set; }

        public PipeServer(string pipeName)
        {
            PipeName = pipeName;

            _cancelSource = new CancellationTokenSource();

            _cancel = _cancelSource.Token;
        }

        public async Task Start()
        {
            Console.WriteLine("[thread: {0}] -> Starting server listener.", Thread.CurrentThread.ManagedThreadId);

            while (!_cancel.IsCancellationRequested)
            {
                await Listener();
            }
        }

        public void Stop()
        {
            _cancelSource.Cancel();
        }

        protected abstract Task Listener();
        
    }
}
