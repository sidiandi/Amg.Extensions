<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	[ValidateSet('Debug', 'Release')]
	[string]$Configuration = 'Release'
)

$InformationPreference = 'Continue'

$runtime = "net6.0"
$Name = 'Amg.Extensions'
$Company = 'Amg'
$version = $null
$outDir = 'out'


# Synopsis: Build project.
task build get-version, {
	exec { dotnet build -c $Configuration /p:Version=${Version} }
}

# Synopsis: Run unit tests
task test build, {
	exec { dotnet test --configuration Release /p:Version=${Version} --no-build }
}

# Synopsis: pack nuget package
task pack test, {
	exec { dotnet pack --configuration Release /p:Version=${Version} --no-build --output $outDir }
}

# Synopsis: push nuget package
task push pack, {
	exec { nuget push Amg.Extensions.${VERSION}.nupkg -Verbosity detailed -ApiKey $NUGET_PUSH_AMG_EXTENSIONS -Source https://api.nuget.org/v3/index.json }
}

# Synopsis: Remove temporary files
task clean {
	remove bin, obj
}

# Synopsis: update all nuget packages to lastest versions. Run on developer machine
task nuget-update {
	exec { dotnet nukeeper update -m 10 -a 0 }
}

function GetVersion {
	$ignore = (git describe --tags) -match 'v(?<version>(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+))'
	$version = $Matches
	return $version
}

task get-version {
	$version = GetVersion
	$script:Version = "$($version.major).$($version.minor).$($version.patch)"
	Write-Information "Current version: ${script:Version}"
}

task release-minor {
	$version = GetVersion
	$version.minor = [int]$version.minor + 1
	$version.patch = 0
	$version.version = "$($version.major).$($version.minor).$($version.patch)"
	
	$tag = "v$($version.version)"
	
	$version.version | Set-Content version.txt
	
	"Release $tag"
	
	git commit -m $tag -a
	git tag $tag
	git push
	git push --tags
	
	# show release status page
	explorer https://github.com/sidiandi/Amg.Extensions/actions
}

task release-patch {
	$version = GetVersion
	$version.patch = [int]$version.patch + 1
	$version.version = "$($version.major).$($version.minor).$($version.patch)"
	
	$tag = "v$($version.version)"
	
	$version.version | sc version.txt
	
	"Release $tag"
	
	git commit -m $tag -a
	git tag $tag
	git push
	git push --tags
	
	# show release status page
	explorer https://github.com/sidiandi/Amg.Extensions/actions
}

# Synopsis: Default task.
task . build
