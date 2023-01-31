using Agent.Communication;
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

        public override string Name => "pivot";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            var pivotService = ServiceProvider.GetService<IPivotService>();
            if (task.SplittedArgs[0] == ShowVerb)
            {
                if (!pivotService.Pivots.Any())
                {
                    context.Result.Result = "No pivots configured!";
                    return;
                }

                var list = new SharpSploitResultList<ListPivotsResult>();
                foreach (var pivot in pivotService.Pivots)
                {
                    list.Add(new ListPivotsResult()
                    {
                        Type = pivot.Connexion.ProtocolString,
                        BindTo = pivot.Connexion.ToString(),
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

                var url = task.SplittedArgs[1].ToLower();
                var conn = ConnexionUrl.FromString(url);
                if (!conn.IsValid)
                {
                    context.Result.Result = $"Pivot binding is not valid !";
                    return;
                }

                if (conn.IsSecure && conn.Protocol == ConnexionType.Http)
                {
                    context.Result.Result = $"Pivot Https is not supported !";
                    return;
                }



                if(!pivotService.AddPivot(conn))
                {
                    context.Result.Result = $"Cannot start pivot {conn}!";
                    return;
                }


                context.Result.Result = $"Pivot {conn.ToString()} started";

            }

            if (task.SplittedArgs[0] == StopVerb)
            {
                var url = task.SplittedArgs[1].ToLower();
                var conn = ConnexionUrl.FromString(url);
                pivotService.RemovePivot(conn);
                context.Result.Result = $"Pivot {conn} stopped.";
            }
            
            if (!pivotService.HasPivots())
                    pivotService.Stop();
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
