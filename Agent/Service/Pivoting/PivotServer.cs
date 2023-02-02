using Agent.Communication;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Agent.Service.RunningService;

namespace Agent.Service.Pivoting
{
    public abstract class PivotServer
    {
        public ConnexionUrl Connexion { get; private set; }

        protected IMessageService _messageService;

        public RunningStatus Status = RunningStatus.Stoped;

        public PivotServer(ConnexionUrl conn)
        {
            Connexion = conn;
            _messageService = ServiceProvider.GetService<IMessageService>();

        }

        protected readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public abstract Task Start();

        public void Stop()
        {
            _tokenSource.Cancel();
        }

        protected List<string> ExtractRelays(List<MessageResult> responses)
        {
            List<string> relays = new List<string>();
            foreach (var mr in responses)
            {
                if (!relays.Contains(mr.Header.Owner))
                    relays.Add(mr.Header.Owner);
            }
            return relays;
        }
    }
}
