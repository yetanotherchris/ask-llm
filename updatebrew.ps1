param(
    [Parameter(Mandatory = $true)]
    [string]
    $Version
)

# Look for the macOS executables in the current directory
$osxX64Pattern = "askllm-v$Version-osx-x64"
$osxArm64Pattern = "askllm-v$Version-osx-arm64"
$linuxX64Pattern = "askllm-v$Version-linux-x64"

$osxX64File = Get-ChildItem -Path $PSScriptRoot -File | Where-Object { $_.Name -eq $osxX64Pattern } | Select-Object -First 1
$osxArm64File = Get-ChildItem -Path $PSScriptRoot -File | Where-Object { $_.Name -eq $osxArm64Pattern } | Select-Object -First 1
$linuxX64File = Get-ChildItem -Path $PSScriptRoot -File | Where-Object { $_.Name -eq $linuxX64Pattern } | Select-Object -First 1

if (-not $osxX64File) {
    throw "Unable to locate $osxX64Pattern in the current directory."
}
if (-not $osxArm64File) {
    throw "Unable to locate $osxArm64Pattern in the current directory."
}
if (-not $linuxX64File) {
    throw "Unable to locate $linuxX64Pattern in the current directory."
}

Write-Output "Getting hashes for Homebrew formula..."
$osxX64Hash = (Get-FileHash -Path $osxX64File.FullName -Algorithm SHA256).Hash
$osxArm64Hash = (Get-FileHash -Path $osxArm64File.FullName -Algorithm SHA256).Hash
$linuxX64Hash = (Get-FileHash -Path $linuxX64File.FullName -Algorithm SHA256).Hash

Write-Output "macOS x64 Hash: $osxX64Hash"
Write-Output "macOS ARM64 Hash: $osxArm64Hash"
Write-Output "Linux x64 Hash: $linuxX64Hash"

# Read the current formula file
$formulaPath = "Formula/ask-llm.rb"
$formulaContent = Get-Content -Path $formulaPath -Raw

# Update version
$formulaContent = $formulaContent -replace 'version "[\d\.]+"', "version `"$Version`""

# Update URLs
$formulaContent = $formulaContent -replace 'url "https://github\.com/yetanotherchris/ask-llm/releases/download/v[\d\.]+/askllm-v[\d\.]+-osx-arm64"', "url `"https://github.com/yetanotherchris/ask-llm/releases/download/v$Version/askllm-v$Version-osx-arm64`""
$formulaContent = $formulaContent -replace 'url "https://github\.com/yetanotherchris/ask-llm/releases/download/v[\d\.]+/askllm-v[\d\.]+-osx-x64"', "url `"https://github.com/yetanotherchris/ask-llm/releases/download/v$Version/askllm-v$Version-osx-x64`""
$formulaContent = $formulaContent -replace 'url "https://github\.com/yetanotherchris/ask-llm/releases/download/v[\d\.]+/askllm-v[\d\.]+-linux-x64"', "url `"https://github.com/yetanotherchris/ask-llm/releases/download/v$Version/askllm-v$Version-linux-x64`""

# Update hashes
$formulaContent = $formulaContent -replace 'sha256 "PLACEHOLDER_ARM64_HASH"', "sha256 `"$osxArm64Hash`""
$formulaContent = $formulaContent -replace 'sha256 "PLACEHOLDER_X64_HASH"', "sha256 `"$osxX64Hash`""
$formulaContent = $formulaContent -replace 'sha256 "PLACEHOLDER_LINUX_HASH"', "sha256 `"$linuxX64Hash`""

# Update install section to use correct filenames
$formulaContent = $formulaContent -replace 'bin\.install "askllm-v[\d\.]+-osx-arm64"', "bin.install `"askllm-v$Version-osx-arm64`""
$formulaContent = $formulaContent -replace 'bin\.install "askllm-v[\d\.]+-osx-x64"', "bin.install `"askllm-v$Version-osx-x64`""
$formulaContent = $formulaContent -replace 'bin\.install "askllm-v[\d\.]+-linux-x64"', "bin.install `"askllm-v$Version-linux-x64`""

Write-Output "Creating Formula/ask-llm.rb for version $Version..."
$formulaContent | Out-File -FilePath $formulaPath -Encoding utf8
