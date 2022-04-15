
$ignore = (git describe --tags) -match 'v(?<version>(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+))'
$version = $Matches
$version.patch = [int]$version.patch + 1
$version.version = "$($version.major).$($version.minor).$($version.patch)"

$tag = "v$($version.version)"

$version.version | sc version.txt

"Release $tag"

git commit -m $tag -a
git tag $tag
git push --tags
