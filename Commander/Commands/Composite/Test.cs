using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Common;
using Common.Payload;

namespace Commander.Commands.Composite;

public class TestCommandOptions
{
    public bool verbose { get; set; }
}
public class TestCommand : CompositeCommand<TestCommandOptions>
{

    public override string Description => "UAC Bypass using FodHelper";
    public override string Name => "test";
    public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

    public override RootCommand Command => new RootCommand(this.Description)
        {
             new Option<string>(new[] { "--key", "-k" }, () => "c2s", "Name of the key to use"),
             new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
             new Option<string>(new[] { "--pipe", "-n" }, () => "local","Name of the pipe used to pivot."),
             new Option<string>(new[] { "--file", "-f" }, () => null,"Name of payload."),
             new Option<string>(new[] { "--path", "-p" }, () => "c:\\windows\\tasks","Name of the folder to upload the payload."),
             new Option(new[] { "--inject", "-i" }, "Îf the payload should be an injector"),
             new Option<int?>(new[] { "--injectDelay", "-id" },() => null, "Delay before injection (AV evasion)"),
             new Option<string>(new[] { "--injectProcess", "-ip" },() => null, "Process path used for injection"),
             new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
        };

    protected override async Task<bool> CreateComposition(CommandContext<TestCommandOptions> context)
    {
        //var agent = context.Executor.CurrentAgent;

        //var endpoint = ConnexionUrl.FromString($"pipe://127.0.0.1:toto");

        //var options = new PayloadGenerationOptions()
        //{
        //    Architecture =  PayloadArchitecture.x64,
        //    Endpoint = endpoint,
        //    IsDebug = false,
        //    IsVerbose = context.Options.verbose,
        //    ServerKey = context.Config.ServerConfig.Key,
        //    Type = PayloadType.Service,
        //    IsInjected = false
        //};

        //context.Terminal.WriteInfo($"[>] Generating Payload!");
        //var pay = context.GeneratePayloadAndDisplay(options, context.Options.verbose);
        //if (pay == null)
        //{
        //    context.Terminal.WriteError($"[X] Generation Failed!");
        //    return false;
        //}
        //else
        //    context.Terminal.WriteSuccess($"[+] Generation succeed!");

        //context.Terminal.WriteLine($"Preparing to upload the file...");

        //string fileName = "test.tmp";
        //string path = @"c:\users\olivier\test.tmp";
        //var fileId = await context.UploadAndDisplay(pay, "test.tst", "Uploading Payload");
        //await context.CommModule.TaskAgentToDownloadFile(agent.Metadata.Id, fileId);

        //this.Echo($"[>] Downloading file {fileName} to {path}...");
        //this.Dowload(fileName, fileId, path);
        //this.Delay(1);

        this.Step($"Waiting {30}s to evade antivirus");
        this.Delay(30);

        this.Step($"Waiting {30}s to evade antivirus");
        this.Delay(30);

        this.Step($"Waiting {30}s to evade antivirus");
        this.Delay(30);

        this.Step($"Waiting {30}s to evade antivirus");
        this.Delay(30);


        this.Echo($"[*] Execution done!");
        this.Echo(Environment.NewLine);

        return true;
    }
}

