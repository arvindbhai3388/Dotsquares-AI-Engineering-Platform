<#
.SYNOPSIS
  Protected-file-guard hook template (PreToolUse) for a client project on the Dotsquares
  AI Engineering Platform.

.DESCRIPTION
  Blocks Read/Edit/Write/Grep/Glob calls that target a file matching this project's own
  restricted-file patterns, as a technical backstop alongside the CLAUDE.md-documented
  restriction - so the restriction is enforced by a script, not only by an instruction
  Claude has to remember and follow correctly every time.

  FAILS OPEN: any unexpected input, parse error, or unmatched pattern always ALLOWS the
  tool call through unmodified. This hook only ever blocks or warns - it never edits,
  deletes, commits, or pushes anything itself, and a bug in this script must never become
  a reason legitimate work gets stuck.

.NOTES
  TEMPLATE USAGE:
  1. Copy this file to <client-project>/.claude/hooks/protected-file-guard.ps1.
  2. Fill in $RestrictedPatterns below to mirror this project's own CLAUDE.md restricted-
     file list exactly - this script and that documentation must describe the same set of
     files, or one of them is lying.
  3. Register it in <client-project>/.claude/settings.json (use "powershell" for the Windows
     PowerShell 5.1 that ships with Windows, or "pwsh" if the target machine has PowerShell 7+
     installed - "powershell" is the safer default since it's not guaranteed every machine has
     PowerShell 7):

     {
       "hooks": {
         "PreToolUse": [
           {
             "matcher": "Read|Edit|Write|Grep|Glob",
             "hooks": [
               { "type": "command", "command": "powershell -File .claude/hooks/protected-file-guard.ps1" }
             ]
           }
         ]
       }
     }

  4. Test it: ask Claude Code to read one of the restricted files and confirm it's actually
     blocked with the message below, not silently allowed through.
#>

# ---------------------------------------------------------------------------
# CONFIGURE THIS PER PROJECT - mirror the restricted-file list already stated
# in this project's own CLAUDE.md. Glob-style patterns (PowerShell -like
# wildcards), matched against both the filename and the full relative path
# Claude Code passes in the tool call.
# ---------------------------------------------------------------------------
$RestrictedPatterns = @(
    "appsettings.json",
    "appsettings.*.json",
    "web.config",
    "secrets.json",
    "launchSettings.json",
    "*.env",
    ".env*",
    "*.key",
    "*.pem",
    "*.pfx",
    "*.p12",
    "*.snk"
    # "<PROJECT_SPECIFIC_RESTRICTED_FILE_HERE - add this project's own custom-named config>"
)

function Test-RestrictedPath {
    param([string]$Path)
    if ([string]::IsNullOrWhiteSpace($Path)) { return $false }
    $normalized = $Path -replace '\\', '/'
    $leaf = Split-Path -Leaf $normalized -ErrorAction SilentlyContinue
    foreach ($pattern in $RestrictedPatterns) {
        if ($leaf -and $leaf -like $pattern) { return $true }
        if ($normalized -like "*$pattern") { return $true }
    }
    return $false
}

try {
    $stdinRaw = [Console]::In.ReadToEnd()
    if ([string]::IsNullOrWhiteSpace($stdinRaw)) {
        exit 0  # nothing to inspect - fail open
    }
    $payload = $stdinRaw | ConvertFrom-Json -ErrorAction Stop

    # tool_input's shape varies by tool - check every field that could carry a path,
    # rather than assuming one fixed field name across Read/Edit/Write/Grep/Glob.
    $candidatePaths = @()
    foreach ($field in @('file_path', 'path', 'pattern')) {
        $val = $payload.tool_input.$field
        if ($val) { $candidatePaths += [string]$val }
    }

    foreach ($p in $candidatePaths) {
        if (Test-RestrictedPath -Path $p) {
            $toolName = $payload.tool_name
            $message = "BLOCKED by protected-file-guard: '" + $p + "' matches this " +
                "project's restricted-file list (see CLAUDE.md's restricted-files " +
                "section). Do not read/edit this file directly - use the existing " +
                "configuration/options pattern, or ask the developer for a placeholder " +
                "value instead. (Tool: " + $toolName + ")"
            [Console]::Error.WriteLine($message)
            exit 2  # block - this reason is surfaced back to Claude, not just the user
        }
    }

    exit 0  # allow
}
catch {
    # Fail open on ANY unexpected error - a bug in this hook must never block legitimate work.
    exit 0
}
