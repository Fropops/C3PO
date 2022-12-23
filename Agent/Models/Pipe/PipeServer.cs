using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Models
{
    public static class NamedPipeServerStreamExtension
    {
        public static bool WaitForConnection(this NamedPipeServerStream server, CancellationToken token)
        {
            using (token.Register(server.Close))
            {
                try
                {
                    server.WaitForConnection();
                    return true;
                }
                catch (System.IO.IOException ex)
                {
                    // Token was canceled - swallow the exception and return null
                    if (token.IsCancellationRequested)
                        return false;
                    throw ex;
                }
            }
        }

    }

    public abstract class PipeServer
    {
        protected readonly CancellationToken _cancel;

        private readonly CancellationTokenSource _cancelSource;

        public string PipeName { get; private set; }

        public PipeCommModule PipeCommModule { get; private set; }

        public PipeServer(string pipeName, PipeCommModule commModule)
        {
            PipeName = pipeName;
            PipeCommModule = commModule;

            _cancelSource = new CancellationTokenSource();

            _cancel = _cancelSource.Token;
        }

        public void Start()
        {
            //Console.WriteLine("[thread: {0}] -> Starting server listener.", Thread.CurrentThread.ManagedThreadId);

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
