using Agent.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class PipeCommModule : CommModule
    {
        public List<PipeLink> Links { get; set; } = new List<PipeLink>();

        public string PipeName { get; protected set; }

        protected PipeServer Server { get; set; }

        private CancellationTokenSource _tokenSource;
        public PipeCommModule(MessageService messageService, FileService fileService) : base(messageService, fileService)
        {
        }

        public void Init(string pipeName = null)
        {
            this.PipeName = pipeName;
            this.IsInitialized = true;
            if (!string.IsNullOrEmpty(pipeName))
                this.Server = new CryptoPipeServer(pipeName, this);
        }

        public override async void Stop()
        {
            if (!this.IsRunning)
                return;
            if (this.Server != null)
                this.Server.Stop();

            this._tokenSource.Cancel();
        }

        public override async void Start()
        {
            this.IsRunning = false;
            if (!this.IsInitialized)
                return;

            if (this.Server != null)
                this.Server.Start();

            _tokenSource = new CancellationTokenSource();

            //Console.WriteLine("Pipe Comm Module - start");

            //int count = 0;
            this.IsRunning = true;
            while (!_tokenSource.IsCancellationRequested)
            {
                //Console.WriteLine($"{count++} Pipe Comm Module - process");

                foreach (var link in this.Links)
                {
                    try
                    {
                        var targets = new List<string>(link.Relays);

                        var tasks = this.MessageService.GetMessageTasksToRelay(targets);

                        //Console.WriteLine($"Creating client");
                        PipeClient client = new CryptoPipeClient(link.Hostname, link.AgentId);

                        var ret = client.SendAndReceive(tasks);

                        this.MessageService.EnqueueResults(ret.Item1);
                        link.Relays = ret.Item2;
#if DEBUG
                        Console.WriteLine($"Relaying to \\\\{link.Hostname}\\{link.AgentId} :");
                        foreach (var mt in tasks)
                        {
                            Console.WriteLine($"In ({mt.Header.Owner})");
                            foreach (var t in mt.Items)
                                Console.WriteLine($"Task ({mt.Header.Owner}) : {t.Command} ");
                        }
                        foreach (var mr in ret.Item1)
                        {
                            Console.WriteLine($"Out ({mr.Header.Owner})");
                            foreach (var r in mr.Items)
                                Console.WriteLine($"Result ({mr.Header.Owner}) : {r.Status} ");
                        }
                        var relays = string.Join(",", ret.Item2);
                        Console.WriteLine($"Relays {relays}");
                        Console.WriteLine();
#endif
                        link.Error = null;
                        link.Status = true;
                        link.LastSeen = DateTime.Now;

                    }
                    catch (Exception ex)
                    {
                        link.Error = ex.Message;
                        link.Status = false;
#if DEBUG
                        Console.WriteLine(ex.ToString());
#endif
                    }
                }


                await Task.Delay(this.GetDelay());
            }

            //Console.WriteLine("Pipe Comm Module - stop");

            this.IsRunning = false;
        }


    }
}
