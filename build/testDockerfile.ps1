$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot './..'));

$dockerfile = Join-Path $root 'build/Dockerfile'

$containername = "localtest.me/jdc.build.dependencygraph:latest"

Write-Host $root;
write-host $dockerfile;

docker build -t $($containername) -f $dockerfile $root 

$projFile = $(Join-Path $root 'src/jdc.build.dependencyGraph/jdc.build.dependencyGraph.csproj');

# mount the projfile under /src in the container
$projFullPath = (Get-Item $projFile).FullName
$projFileName = [IO.Path]::GetFileName($projFullPath)
$containerProjPath = "/src/$projFileName"


docker run  -it -v "${projFullPath}:${containerProjPath}" $($containername) 


