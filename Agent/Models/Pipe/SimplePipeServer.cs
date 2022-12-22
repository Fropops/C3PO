
    using System;
using System.Collections.Generic;
using System.IO;
    using System.IO.Pipes;
using System.Linq;
using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

namespace Agent.Models
{
    public class SimplePipeServer : PipeServer
    {
       

        public SimplePipeServer(string pipeName, PipeCommModule commModule) : base(pipeName, commModule)
        {
        }

    
        protected override async Task Listener()
        {
            using (NamedPipeServerStream server = new NamedPipeServerStream(this.PipeName, PipeDirection.InOut, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                //Console.WriteLine("\r\n[thread: {0}] -> Waiting for client.", Thread.CurrentThread.ManagedThreadId);

                await Task.Factory.FromAsync(server.BeginWaitForConnection, server.EndWaitForConnection, null);

                //Console.WriteLine("Pipe Client connected.", Thread.CurrentThread.ManagedThreadId);

                var reader = new StreamReader(server);
 
                var writer = new StreamWriter(server);

                var b64tasks = reader.ReadLine();
                var tasks = Convert.FromBase64String(b64tasks).Deserialize<List<MessageTask>>();
                this.PipeCommModule.MessageService.EnqueueTasks(tasks);
                //Console.WriteLine("Pipe Client Tasks received", Thread.CurrentThread.ManagedThreadId);

                var agentId = this.PipeCommModule.MessageService.AgentMetaData.Id;

                var results = this.PipeCommModule.MessageService.GetMessageResultsToRelay();

                if (!results.Any(t => t.Header.Owner == agentId))
                {
                    //add a checkin message
                    var messageResult = new MessageResult();
                    messageResult.Header.Owner = agentId;
                    messageResult.FileChunk = this.PipeCommModule.FileService.GetChunkToSend();
                    results.Add(messageResult);
                }


                foreach(var resMess in results)
                    resMess.Header.Path.Insert(0,agentId);


                string b64results = Convert.ToBase64String(results.Serialize());
                writer.WriteLine(b64results);
                writer.Flush();
                //Console.WriteLine("Pipe Client result sent", Thread.CurrentThread.ManagedThreadId);

                //Get all relays
                var allrelays = this.PipeCommModule.Links.SelectMany(l => l.Relays).ToList();
                allrelays.Add(this.PipeCommModule.MessageService.AgentMetaData.Id);
                string b64Relays = Convert.ToBase64String(allrelays.Serialize());
                writer.WriteLine(b64Relays);
                writer.Flush();
                //Console.WriteLine("Pipe Client Relay sent.", Thread.CurrentThread.ManagedThreadId);

                if (server.IsConnected)
                {
                    //Console.WriteLine("Pipe Client Disconnected.", Thread.CurrentThread.ManagedThreadId);
                    server.Disconnect();
                }

                //Console.WriteLine("Pipe Client Leaved.", Thread.CurrentThread.ManagedThreadId);
            }
        }
    }
}