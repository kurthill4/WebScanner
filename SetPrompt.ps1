# Determine the directory where this script is located
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ScriptPath = [System.IO.Path]::GetFullPath($ScriptPath).TrimEnd('\')
$RootPath = Split-Path $ScriptPath -Parent

Write-Host "ScriptPath: $ScriptPath"
Write-Host "RootPath: $RootPath"

# Define the prompt function in the global scope
function global:prompt {
    $current = [System.IO.Path]::GetFullPath($PWD.Path).TrimEnd('\')
    $customprompt = ""

    if ($current.StartsWith($ScriptPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        $relative = $current.Substring($RootPath.Length).TrimStart('\')
        if ($relative) {
            $customprompt = "$relative> "
        } else {
            # If in the solution directory, show its name
            $customprompt = "$(Split-Path $ScriptPath -Leaf)> "
        }
    } else {
        $customprompt = "$current> "
    }
    return $customprompt

}
