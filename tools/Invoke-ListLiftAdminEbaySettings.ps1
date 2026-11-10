param(
    [Parameter(Mandatory)]
    [ValidateSet('Get', 'Set', 'Delete')]
    [string]$Action,
    [string]$ApiBaseUrl = 'https://localhost:7155',
    [string]$CredentialPath = (Join-Path $env:LOCALAPPDATA 'ListLift\AdminCredential.dpapi')
)

$ErrorActionPreference = 'Stop'
$credentialFile = [System.IO.Path]::GetFullPath($CredentialPath)
if (-not (Test-Path -LiteralPath $credentialFile)) { throw 'Admin credential file was not found on this laptop.' }
$secureCredential = ConvertTo-SecureString ([System.IO.File]::ReadAllText($credentialFile))
$credential = [System.Net.NetworkCredential]::new('', $secureCredential).Password
$email = Read-Host 'ListLift account email'
$passwordSecure = Read-Host 'ListLift account password' -AsSecureString
$password = [System.Net.NetworkCredential]::new('', $passwordSecure).Password
$loginBody = @{ email = $email; password = $password } | ConvertTo-Json
$login = Invoke-RestMethod -Method Post -Uri "$($ApiBaseUrl.TrimEnd('/'))/auth/login?useCookies=false" -ContentType 'application/json' -Body $loginBody
$accessToken = $login.accessToken
if ([string]::IsNullOrWhiteSpace($accessToken)) { throw 'ListLift login did not return an access token.' }
$password = $null
$loginBody = $null

$headers = @{
    Authorization = "Bearer $accessToken"
    'X-ListLift-Admin-Key' = $credential
}
$uri = "$($ApiBaseUrl.TrimEnd('/'))/api/admin/ebay-settings"
try {
    switch ($Action) {
        'Get' { Invoke-RestMethod -Method Get -Uri $uri -Headers $headers }
        'Set' {
            $clientId = Read-Host 'eBay Production Client ID'
            $clientSecretSecure = Read-Host 'eBay Production Client Secret' -AsSecureString
            $clientSecret = [System.Net.NetworkCredential]::new('', $clientSecretSecure).Password
            $body = @{ clientId = $clientId; clientSecret = $clientSecret } | ConvertTo-Json
            Invoke-RestMethod -Method Put -Uri $uri -Headers $headers -ContentType 'application/json' -Body $body
            Write-Host 'eBay application settings saved.'
            $clientSecret = $null
            $body = $null
        }
        'Delete' {
            Invoke-RestMethod -Method Delete -Uri $uri -Headers $headers
            Write-Host 'eBay application settings removed.'
        }
    }
}
finally {
    $credential = $null
    $accessToken = $null
    $headers.Clear()
}
