using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class LinkCommand : AgentCommand
    {
        public override string Name => "link";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            if (task.SplittedArgs.Count() != 2)
            {
                var list = new SharpSploitResultList<ListLinkResult>();
                foreach (var link in context.Agent.PipeCommunicator.Links)
                {
                    list.Add(new ListLinkResult()
                    {
                        Pipe = link.Hostname + " => " + link.AgentId,
                        Status = link.Status ? "OK" : "ERROR",
                        Error = link.Error ?? string.Empty,
                        LastSeen = link.LastSeen?.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss")
                    });
                   
                }
                context.Result.Result = list.ToString();
                return;
            }

            context.Agent.PipeCommunicator.Links.Add(new PipeLink() { Hostname = task.SplittedArgs[0], AgentId = task.SplittedArgs[1] });
        }


        public sealed class ListLinkResult : SharpSploitResult
        {
            
            public string Pipe { get; set; }
            public string Status { get; set; }

            public string Error { get; set; }
            public string LastSeen { get; set; }



            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Pipe), Value = Pipe },
                new SharpSploitResultProperty { Name = nameof(Status), Value = Status },
                new SharpSploitResultProperty { Name = nameof(LastSeen), Value = LastSeen },
                new SharpSploitResultProperty { Name = nameof(Error), Value = Error },
            };
        }
    }
}
