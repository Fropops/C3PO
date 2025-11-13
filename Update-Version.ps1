function Update-ProjectVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath,

        [ValidateSet("major", "minor", "patch")]
        [string]$Increment = "patch"
    )

    if (Test-Path $ProjectPath -PathType Container) {
        $csproj = Get-ChildItem -Path $ProjectPath -Filter *.csproj | Select-Object -First 1
        if (-not $csproj) {
            Write-Error "? Aucun fichier .csproj trouvé dans $ProjectPath"
            return
        }
        $ProjectPath = $csproj.FullName
    }

    Write-Host "?? Lecture du fichier : $ProjectPath"
    $content = Get-Content $ProjectPath -Raw

    # Recherche de la balise <Version>
    if ($content -match "<Version>([\d\.]+)</Version>") {
        $version = $matches[1]
        Write-Host "?? Version actuelle : $version"
        $parts = $version.Split('.')

        while ($parts.Count -lt 3) { $parts += 0 }

        switch ($Increment) {
            "major" { $parts[0]++; $parts[1] = 0; $parts[2] = 0 }
            "minor" { $parts[1]++; $parts[2] = 0 }
            "patch" { $parts[2]++ }
        }

        $newVersion = ($parts -join '.')
        Write-Host "? Nouvelle version : $newVersion"

        $newContent = $content -replace "<Version>[\d\.]+</Version>", "<Version>$newVersion</Version>"
        Set-Content -Path $ProjectPath -Value $newContent -Encoding UTF8
    }
    else {
        Write-Host "??  Aucune balise <Version> trouvée, ajout dans le premier <PropertyGroup>."
        $content = $content -replace "(?<=<PropertyGroup>)", "`n    <Version>1.0.0</Version>"
        Set-Content -Path $ProjectPath -Value $content -Encoding UTF8
    }
}
