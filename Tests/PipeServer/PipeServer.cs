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
        protected readonly CancellationToken _cancel;

        protected readonly CancellationTokenSource _cancelSource;

        public string PipeName { get; private set; }

        public PipeServer(string pipeName)
        {
            PipeName = pipeName;

            _cancelSource = new CancellationTokenSource();

            _cancel = _cancelSource.Token;
        }

        public void Start()
        {
            Console.WriteLine("[thread: {0}] -> Starting server listener.", Thread.CurrentThread.ManagedThreadId);

            var serverThread = new Thread(() => RunServer());
            serverThread.Start();
        }

        public void Stop()
        {
            _cancelSource.Cancel();
        }

        protected abstract void RunServer();

        
    }
}
