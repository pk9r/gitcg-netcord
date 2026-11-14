<#
.SYNOPSIS
    Manages Antigravity agent plugin configuration for the gitcg-netcord repository.
.DESCRIPTION
    Optimizes agent context window by managing installed global plugins (Strategy B: Zero-Truncation).
    Disables 12 non-essential plugins while preserving 6 core plugins (dotnet11, dotnet-data,
    dotnet, dotnet-msbuild, dotnet-test, dotnet-diag). Provides on-demand switches for Blazor.
.PARAMETER Apply
    Disables 12 non-essential plugins (including Blazor) to achieve 61 active skills and zero truncation.
.PARAMETER Restore
    Re-enables all 12 plugins when switching to a different project workload.
.PARAMETER EnableBlazor
    Activates dotnet-blazor on-demand when starting Blazor web frontend/dashboard development.
.PARAMETER DisableBlazor
    Deactivates dotnet-blazor to reclaim context for pure bot backend development.
.PARAMETER Status
    Displays current status of all plugins and calculates skill budget.
.EXAMPLE
    pwsh scripts/optimize-agent-skills.ps1 -Status
.EXAMPLE
    pwsh scripts/optimize-agent-skills.ps1 -Apply
.EXAMPLE
    pwsh scripts/optimize-agent-skills.ps1 -EnableBlazor
.EXAMPLE
    pwsh scripts/optimize-agent-skills.ps1 -DisableBlazor
.EXAMPLE
    pwsh scripts/optimize-agent-skills.ps1 -Restore
#>

[CmdletBinding(DefaultParameterSetName = 'Status')]
param(
    [Parameter(ParameterSetName = 'Apply', Mandatory = $true)]
    [switch]$Apply,

    [Parameter(ParameterSetName = 'Restore', Mandatory = $true)]
    [switch]$Restore,

    [Parameter(ParameterSetName = 'EnableBlazor', Mandatory = $true)]
    [switch]$EnableBlazor,

    [Parameter(ParameterSetName = 'DisableBlazor', Mandatory = $true)]
    [switch]$DisableBlazor,

    [Parameter(ParameterSetName = 'Status')]
    [switch]$Status
)

$ErrorActionPreference = 'Stop'

# Define target non-essential plugins for Strategy B (12 plugins)
$TargetPlugins = @(
    'azure',
    'dotnet-maui',
    'dotnet-winforms',
    'dotnet-template-engine',
    'dotnet-nuget',
    'dotnet-test-migration',
    'dotnet-upgrade',
    'dotnet-advanced',
    'dotnet-aspnetcore',
    'dotnet-ai',
    'dotnet-experimental',
    'dotnet-blazor'
)

# Locate agy CLI executable
function Find-AgyCli {
    $cli = Get-Command 'agy' -ErrorAction SilentlyContinue
    if ($cli) {
        return $cli.Source
    }

    $candidates = @(
        (Join-Path $HOME 'AppData\Local\agy\bin\agy.exe'),
        (Join-Path $HOME '.gemini\bin\agy.exe'),
        (Join-Path $env:LOCALAPPDATA 'Programs\antigravity\bin\agy.exe'),
        (Join-Path $env:ProgramFiles 'Antigravity\bin\agy.exe')
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    throw "Antigravity CLI (agy.exe) could not be found. Please ensure it is in PATH or under $HOME\.gemini\bin\agy.exe."
}

$AgyPath = Find-AgyCli

if ($EnableBlazor) {
    Write-Host "==> Enabling dotnet-blazor on-demand..." -ForegroundColor Cyan
    try {
        $null = & $AgyPath plugin enable 'dotnet-blazor' 2>&1
        Write-Host "dotnet-blazor is now ENABLED." -ForegroundColor Green
    }
    catch {
        Write-Host "Failed to enable dotnet-blazor: $_" -ForegroundColor Red
    }
    Write-Host ""
}

if ($DisableBlazor) {
    Write-Host "==> Disabling dotnet-blazor to reclaim context..." -ForegroundColor Cyan
    try {
        $null = & $AgyPath plugin disable 'dotnet-blazor' 2>&1
        Write-Host "dotnet-blazor is now DISABLED." -ForegroundColor Yellow
    }
    catch {
        Write-Host "Failed to disable dotnet-blazor: $_" -ForegroundColor Red
    }
    Write-Host ""
}

if ($Apply) {
    Write-Host "==> Applying Agent Skill Optimization (Strategy B: Zero-Truncation)..." -ForegroundColor Cyan
    Write-Host "Active target: 6 core plugins (dotnet11, dotnet-data, dotnet, dotnet-msbuild, dotnet-test, dotnet-diag)." -ForegroundColor Green
    Write-Host "Disabling 12 non-essential/auxiliary plugins: $($TargetPlugins -join ', ')...`n" -ForegroundColor Yellow

    foreach ($plugin in $TargetPlugins) {
        Write-Host "Disabling plugin: $plugin..." -NoNewline
        try {
            $null = & $AgyPath plugin disable $plugin 2>&1
            Write-Host " [DONE]" -ForegroundColor Green
        }
        catch {
            Write-Host " [FAILED: $_]" -ForegroundColor Red
        }
    }

    Write-Host "`nOptimization complete! Running status check...`n" -ForegroundColor Cyan
}

if ($Restore) {
    Write-Host "==> Restoring all plugins for gitcg-netcord..." -ForegroundColor Cyan
    foreach ($plugin in $TargetPlugins) {
        Write-Host "Enabling plugin: $plugin..." -NoNewline
        try {
            $null = & $AgyPath plugin enable $plugin 2>&1
            Write-Host " [DONE]" -ForegroundColor Green
        }
        catch {
            Write-Host " [FAILED: $_]" -ForegroundColor Red
        }
    }

    Write-Host "`nRestore complete! Running status check...`n" -ForegroundColor Cyan
}

# Status Dashboard
Write-Host "==> Antigravity Plugin & Skill Status for gitcg-netcord <==" -ForegroundColor Cyan
Write-Host "CLI: $AgyPath`n" -ForegroundColor DarkGray

$rawJson = & $AgyPath plugin list --json
if (-not $rawJson) {
    Write-Warning "Could not retrieve JSON plugin list from agy CLI."
    exit 0
}

$plugins = $rawJson | ConvertFrom-Json
$enabledPlugins = $plugins | Where-Object { $_.enabled -ne $false }
$disabledPlugins = $plugins | Where-Object { $_.enabled -eq $false }

$totalActiveSkills = ($enabledPlugins | ForEach-Object { $_.skills.Count } | Measure-Object -Sum).Sum

Write-Host ("{0,-25} {1,-12} {2,-10}" -f "PLUGIN NAME", "STATUS", "SKILLS") -ForegroundColor White
Write-Host ("{0,-25} {1,-12} {2,-10}" -f "-----------", "------", "------") -ForegroundColor DarkGray

foreach ($p in $plugins | Sort-Object name) {
    $statusText = if ($p.enabled -ne $false) { "ENABLED" } else { "DISABLED" }
    $color = if ($p.enabled -ne $false) { "Green" } else { "DarkGray" }
    $skillCount = if ($p.skills) { $p.skills.Count } else { 0 }

    Write-Host ("{0,-25} " -f $p.name) -NoNewline
    Write-Host ("{0,-12} " -f $statusText) -ForegroundColor $color -NoNewline
    Write-Host ("{0,-10}" -f $skillCount)
}

Write-Host "`n--------------------------------------------------" -ForegroundColor DarkGray
Write-Host "Total Active Global Plugins: $($enabledPlugins.Count) / $($plugins.Count)" -ForegroundColor White
Write-Host "Total Active Plugin Skills : $totalActiveSkills skills" -ForegroundColor White

if ($totalActiveSkills -gt 65) {
    Write-Host "Status: CONTEXT OVERFLOW RISK! (Active skills exceed recommended budget limit ~65)" -ForegroundColor Red
    Write-Host "Recommendation: Run 'pwsh scripts/optimize-agent-skills.ps1 -Apply' to optimize." -ForegroundColor Yellow
} else {
    Write-Host "Status: HEALTHY! All required skills will load into agent context without dropping." -ForegroundColor Green
}
