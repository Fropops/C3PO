
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipeServer
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

    public class SimplePipeServer : PipeServer
    {


        public SimplePipeServer(string pipeName) : base(pipeName)
        {
        }

        protected override void RunServer()
        {

            using (NamedPipeServerStream server = new NamedPipeServerStream(this.PipeName, PipeDirection.InOut, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                while (!_cancel.IsCancellationRequested)
                {
                    Console.WriteLine("\r\n[thread: {0}] -> Waiting for client.", Thread.CurrentThread.ManagedThreadId);

                    if (!server.WaitForConnection(this._cancel))
                        return;

                    var reader = new StreamReader(server);
                    Console.WriteLine($"received : " + reader.ReadLine());

                    var writer = new StreamWriter(server);
                    writer.WriteLine($"Send to client");
                    writer.Flush();


                    if (server.IsConnected)
                        server.Disconnect();
                }
            }
        }
    }
}