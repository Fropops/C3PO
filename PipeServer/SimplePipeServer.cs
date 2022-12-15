
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

namespace PipeServer
{
    public class SimplePipeServer : PipeServer
    {
       

        public SimplePipeServer(string pipeName) : base(pipeName)
        {
        }

    
        protected override async Task Listener()
        {
            using (NamedPipeServerStream server = new NamedPipeServerStream(this.PipeName, PipeDirection.InOut, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                Console.WriteLine("\r\n[thread: {0}] -> Waiting for client.", Thread.CurrentThread.ManagedThreadId);

                await Task.Factory.FromAsync(server.BeginWaitForConnection, server.EndWaitForConnection, null);

                Console.WriteLine("[thread: {0}] -> Client connected.", Thread.CurrentThread.ManagedThreadId);

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