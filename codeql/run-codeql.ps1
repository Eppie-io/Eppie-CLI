$now = $(Get-Date -Format "yyyy-MM-ddTHH-mm-ss")
$codeql = "codeql.exe"
$db = ".\eppie-database"
$output_dir = ".\output"
$output = Join-Path $output_dir "eppie-cli-${now}.sarif"
$project = ".\..\src\Eppie.CLI"

# if codeql.exe does not exist, install it
if (-not (Get-Command $codeql -ErrorAction SilentlyContinue)) { 
    "CodeQL can't launch." | Write-Host -ForegroundColor Red
    "1. Setting up the CodeQL CLI [https://docs.github.com/en/code-security/codeql-cli/getting-started-with-the-codeql-cli/setting-up-the-codeql-cli]" |  Write-Host
    "2. Add 'codeql' directory to your PATH." | Write-Host
    exit 1
}

& $codeql --version

$csharp_dir = ((& $codeql resolve languages --format=json) | ConvertFrom-Json).csharp

if (-not (Test-Path -Path "$csharp_dir")) { 
    "'csharp' CodeQL language pack (dir: $csharp_dir) not found" | Write-Host -ForegroundColor Red
    exit 1
}

"Creating database for a source tree..." | Write-Host -ForegroundColor Cyan
if (Test-Path -Path "$db") {
    Remove-Item -Path "$db" -Recurse -Force
}

& $codeql database create "$db" --overwrite --language=csharp --source-root "$project"

if(-not ($?))
{
   "The database was not created properly." | Write-Host -ForegroundColor Red
   exit 1
}

"Running the project analysis..." | Write-Host -ForegroundColor Cyan
New-Item -Path "$output_dir" -ItemType Directory -Force

& $codeql database analyze "$db" codeql/csharp-queries --download --format=sarif-latest --output "$output"

if($?)
{
   "Analysis completed. Output file: $output" | Write-Host -ForegroundColor Green
}
else
{
   "Analysis did not complete successfully." | Write-Host -ForegroundColor Red
   exit 1
}

