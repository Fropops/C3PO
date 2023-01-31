using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Agent.Service.RunningService;

namespace Agent.Commands
{
    public class PivotCommand : AgentCommand
    {
        const string StartVerb = "start";
        const string StopVerb = "stop";
        const string ShowVerb = "show";

        const string TcpPivot = "tcp";

        public override string Name => "pivot";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            var pivotService = ServiceProvider.GetService<IPivotService>();
            if (task.SplittedArgs[0] == ShowVerb)
            {
                var list = new SharpSploitResultList<ListPivotsResult>();
                //foreach (var pivot in pivotService.Ge)
                //{
                    //list.Add(new ListPivotsResult()
                    //{
                    //	Type = service.Ty,
                    //	Status = service.Status == RunningStatus.Running ? "[Running]" : "[Off]"
                    //});

                //}
                context.Result.Result = list.ToString();
                return;
            }


            if (task.SplittedArgs[0] == StartVerb)
            {
                if (pivotService.Status == RunningStatus.Stoped)
                {
                    pivotService.Start();
                }

                if (task.SplittedArgs[1].ToLower() == "tcp")
                {
                    var port = int.Parse(task.SplittedArgs[2]);
                    if (pivotService.IsPivotRunningOnPort(port))
                    {
                        context.Result.Result = $"A pivot is already running on port {port}";
                        return;
                    }
                    pivotService.AddTCPServer(port);
                    context.Result.Result = $"TCP pivot started on port {port}";
                }


            }

            if (task.SplittedArgs[0] == StopVerb)
            {
                if (task.SplittedArgs[1].ToLower() == "tcp")
                {
                    var port = int.Parse(task.SplittedArgs[2]);
                    if (!pivotService.RemoveTCPServer(port))
                    {
                        context.Result.Result = $"A TCP pivot is not running on port {port}";
                        return;
                    }
                    else
                    {
                        context.Result.Result = $"A TCP pivot stopped on port {port}";
                    }
                }


                if (!pivotService.HasPivots())
                    pivotService.Stop();
            }
        }


        public sealed class ListPivotsResult : SharpSploitResult
        {

            public string Type { get; set; }
            public string EndPoint { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Type), Value = Type },
                new SharpSploitResultProperty { Name = nameof(EndPoint), Value = EndPoint },
            };
        }
    }
}
