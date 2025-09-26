param(
    [Parameter(Mandatory = $true)]
    [string]
    $Version
)

$searchRoot = Join-Path -Path $PSScriptRoot -ChildPath 'src/AskLlm/bin/Release'
if (-not (Test-Path -Path $searchRoot)) {
    throw "Publish output not found at $searchRoot"
}

$searchPattern = "*publish/askllm.exe"
$askLlmFile = Get-ChildItem -Path $searchRoot -Force -Recurse -File | Where-Object { $_.FullName -like $searchPattern } | Select-Object -First 1

if (-not $askLlmFile) {
    throw "Unable to locate askllm.exe in the publish output."
}

$filePath = $askLlmFile.FullName
Write-Output "File found: $filePath, getting hash..."
$hash = (Get-FileHash -Path $filePath -Algorithm SHA256).Hash
Write-Output "Hash: $hash"

$manifest = @{
    version = $Version
    architecture = @{
        '64bit' = @{
            url = "https://github.com/yetanotherchris/ask-llm/releases/download/v$Version/askllm.exe"
            bin = @("askllm.exe")
            hash = $hash
        }
    }
    homepage = "https://github.com/yetanotherchris/ask-llm"
    license = "MIT License"
    description = "Ask any large language model from your terminal via OpenAI-compatible APIs."
}

Write-Output "Creating ask-llm.json for version $Version..."
$manifest | ConvertTo-Json -Depth 5 | Out-File -FilePath "ask-llm.json" -Encoding utf8
