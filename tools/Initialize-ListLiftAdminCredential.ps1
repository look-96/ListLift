param(
    [string]$ApiProject = (Join-Path $PSScriptRoot '..\src\ListLift.API'),
    [string]$CredentialPath = (Join-Path $env:LOCALAPPDATA 'ListLift\AdminCredential.dpapi'),
    [switch]$Reset
)

$ErrorActionPreference = 'Stop'
$resolvedPath = [System.IO.Path]::GetFullPath($CredentialPath)
if ((Test-Path -LiteralPath $resolvedPath) -and -not $Reset) {
    throw "Credential already exists at '$resolvedPath'. Use -Reset only after securely removing access to the old credential."
}

[System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($resolvedPath)) | Out-Null
$bytes = [byte[]]::new(32)
$randomGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $randomGenerator.GetBytes($bytes)
}
finally {
    $randomGenerator.Dispose()
}
$credential = [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
$secureCredential = ConvertTo-SecureString -String $credential -AsPlainText -Force
$encrypted = ConvertFrom-SecureString -SecureString $secureCredential
[System.IO.File]::WriteAllText($resolvedPath, $encrypted)

$sha256 = [System.Security.Cryptography.SHA256]::Create()
try {
    $hashBytes = $sha256.ComputeHash([Text.Encoding]::UTF8.GetBytes($credential))
    $hash = [BitConverter]::ToString($hashBytes).Replace('-', '')
}
finally {
    $sha256.Dispose()
}
dotnet user-secrets set 'Admin:CredentialHash' $hash --project $ApiProject
if ($LASTEXITCODE -ne 0) {
    Remove-Item -LiteralPath $resolvedPath -Force
    throw 'Could not save the admin credential hash to API User Secrets.'
}

Write-Host "Laptop-bound credential saved to $resolvedPath"
Write-Host 'The API User Secrets store now contains only the SHA-256 hash.'
Write-Host 'Keep the encrypted file private. It is decryptable only by your Windows account on this laptop.'
