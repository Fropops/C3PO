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
                if(!pivotService.TCPServers.Any() && !pivotService.HTTPServers.Any())
                {
                    context.Result.Result = "No pivots configured!";
                    return;
                }

                var list = new SharpSploitResultList<ListPivotsResult>();
                foreach (var pivot in pivotService.TCPServers)
                {
                    list.Add(new ListPivotsResult()
                    {
                        Type = pivot.Type,
                        BindTo = "0.0.0.0:" + pivot.Port.ToString(),
                    });

                }

                foreach (var pivot in pivotService.HTTPServers)
                {
                    list.Add(new ListPivotsResult()
                    {
                        Type = pivot.Type,
                        BindTo = "0.0.0.0:" + pivot.Port.ToString(),
                    });

                }
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
                    pivotService.AddTCPServer(port, false);
                    context.Result.Result = $"TCP pivot started on port {port}";
                }

                if (task.SplittedArgs[1].ToLower() == "tcps")
                {
                    var port = int.Parse(task.SplittedArgs[2]);
                    if (pivotService.IsPivotRunningOnPort(port))
                    {
                        context.Result.Result = $"A pivot is already running on port {port}";
                        return;
                    }
                    pivotService.AddTCPServer(port);
                    context.Result.Result = $"TCPS pivot started on port {port}";
                }

                if (task.SplittedArgs[1].ToLower() == "http")
                {
                    var port = int.Parse(task.SplittedArgs[2]);
                    if (pivotService.IsPivotRunningOnPort(port))
                    {
                        context.Result.Result = $"A pivot is already running on port {port}";
                        return;
                    }
                    pivotService.AddHTTPServer(port, false);
                    context.Result.Result = $"HTTP pivot started on port {port}";
                }

                if (task.SplittedArgs[1].ToLower() == "https")
                {
                    var port = int.Parse(task.SplittedArgs[2]);
                    if (pivotService.IsPivotRunningOnPort(port))
                    {
                        context.Result.Result = $"A pivot is already running on port {port}";
                        return;
                    }
                    context.Result.Result = $"HTTPS pivot is not supported";
                }


            }

            if (task.SplittedArgs[0] == StopVerb)
            {
                if (task.SplittedArgs[1].ToLower() == "tcp" || task.SplittedArgs[1].ToLower() == "tcps" || task.SplittedArgs[1].ToLower() == "http" || task.SplittedArgs[1].ToLower() == "https")
                {
                    var port = int.Parse(task.SplittedArgs[2]);
                    if (!pivotService.RemoveTCPServer(port))
                    {
                        context.Result.Result = $"No pivot is not running on port {port}";
                        return;
                    }
                    else
                    {
                        context.Result.Result = $"Pivot stopped on port {port}";
                    }
                }


                if (!pivotService.HasPivots())
                    pivotService.Stop();
            }
        }


        public sealed class ListPivotsResult : SharpSploitResult
        {

            public string Type { get; set; }
            public string BindTo { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Type), Value = Type },
                new SharpSploitResultProperty { Name = nameof(BindTo), Value = BindTo },
            };
        }
    }
}
