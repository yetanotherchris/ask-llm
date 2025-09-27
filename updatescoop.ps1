param(
    [Parameter(Mandatory = $true)]
    [string]
    $Version
)

# Look for the renamed Windows executable in the current directory
$searchPattern = "askllm-v$Version-win-x64.exe"
$askLlmFile = Get-ChildItem -Path $PSScriptRoot -File | Where-Object { $_.Name -eq $searchPattern } | Select-Object -First 1

if (-not $askLlmFile) {
    throw "Unable to locate $searchPattern in the current directory."
}

$filePath = $askLlmFile.FullName
Write-Output "File found: $filePath, getting hash..."
$hash = (Get-FileHash -Path $filePath -Algorithm SHA256).Hash
Write-Output "Hash: $hash"

$manifest = @{
    version = $Version
    architecture = @{
        '64bit' = @{
            url = "https://github.com/yetanotherchris/ask-llm/releases/download/v$Version/askllm-v$Version-win-x64.exe"
            bin = @("askllm.exe")
            hash = $hash
            extract_dir = ""
            pre_install = @("Rename-Item `"`$dir\askllm-v$Version-win-x64.exe`" `"askllm.exe`"")
        }
    }
    homepage = "https://github.com/yetanotherchris/ask-llm"
    license = "MIT License"
    description = "Ask any large language model from your terminal via OpenAI-compatible APIs."
}

Write-Output "Creating ask-llm.json for version $Version..."
$manifest | ConvertTo-Json -Depth 5 | Out-File -FilePath "ask-llm.json" -Encoding utf8
