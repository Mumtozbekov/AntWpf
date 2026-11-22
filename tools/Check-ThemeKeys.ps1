<#
.SYNOPSIS
    Verifies that every theme dictionary defines exactly the same resource keys.

.DESCRIPTION
    Compares Themes/Light.xaml with Themes/Dark.xaml (or the files passed in) and checks that
    both define the same x:Key set, that each key has the same element type (Color,
    SolidColorBrush, ...) and the same Options:Freeze setting. Exits with code 1 on mismatch.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools/Check-ThemeKeys.ps1
#>
param(
    [string]$Reference = (Join-Path $PSScriptRoot '..\AntWpf\Themes\Light.xaml'),
    [string[]]$Themes = @((Join-Path $PSScriptRoot '..\AntWpf\Themes\Dark.xaml'))
)

$xamlNs    = 'http://schemas.microsoft.com/winfx/2006/xaml'
$optionsNs = 'http://schemas.microsoft.com/winfx/2006/xaml/presentation/options'

function Get-ThemeKeys([string]$path) {
    [xml]$doc = Get-Content -LiteralPath $path -Raw -Encoding UTF8
    $keys = [ordered]@{}
    foreach ($node in $doc.DocumentElement.ChildNodes) {
        if ($node.NodeType -ne 'Element') { continue }
        $key = $node.GetAttribute('Key', $xamlNs)
        if (-not $key) { continue }
        if ($keys.Contains($key)) { throw "Duplicate key '$key' in $path" }
        $keys[$key] = [pscustomobject]@{
            Type   = $node.LocalName
            Freeze = $node.GetAttribute('Freeze', $optionsNs)
        }
    }
    return $keys
}

$refKeys = Get-ThemeKeys $Reference
$failed = $false
Write-Host ("{0}: {1} keys" -f (Split-Path $Reference -Leaf), $refKeys.Count)

foreach ($theme in $Themes) {
    $name = Split-Path $theme -Leaf
    $keys = Get-ThemeKeys $theme
    $problems = @()

    foreach ($k in $refKeys.Keys) {
        if (-not $keys.Contains($k)) { $problems += "missing key '$k'"; continue }
        if ($keys[$k].Type -ne $refKeys[$k].Type) {
            $problems += "key '$k' is $($keys[$k].Type), expected $($refKeys[$k].Type)"
        }
        if ($keys[$k].Freeze -ne $refKeys[$k].Freeze) {
            $problems += "key '$k' Options:Freeze='$($keys[$k].Freeze)', expected '$($refKeys[$k].Freeze)'"
        }
    }
    foreach ($k in $keys.Keys) {
        if (-not $refKeys.Contains($k)) { $problems += "extra key '$k'" }
    }

    if ($problems.Count -eq 0) {
        Write-Host ("{0}: {1} keys - OK" -f $name, $keys.Count)
    } else {
        $failed = $true
        Write-Host ("{0}: {1} problem(s)" -f $name, $problems.Count)
        $problems | ForEach-Object { Write-Host "  - $_" }
    }
}

if ($failed) { exit 1 }
exit 0
