param (
  [string]$CommitMsg
)

$versionFile = "VERSION"
if (-Not (Test-Path $versionFile)) {
  "0.0.0" | Out-File -Encoding ASCII $versionFile
}

$version = Get-Content $versionFile -Raw
$parts = $version -split '\.'

$major = [int]$parts[0]
$minor = [int]$parts[1]
$patch = [int]$parts[2]

if ($CommitMsg -match 'BREAKING CHANGE' -or $CommitMsg -match '^feat!?:') {
  $major++
  $minor = 0
  $patch = 0
} elseif ($CommitMsg -match '^feat:') {
  $minor++
  $patch = 0
} elseif ($CommitMsg -match '^fix:') {
  $patch++
}

$bumpedVersion = "$major.$minor.$patch"

# Set output
"version=v$bumpedVersion" >> $env:GITHUB_OUTPUT
