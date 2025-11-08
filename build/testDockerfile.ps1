$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot './..'));

$dockerfile = Join-Path $root 'build/Dockerfile'

$containername = "localtest.me/jdc.build.dependencygraph:latest"

Write-Host $root;
write-host $dockerfile;

docker build -t $($containername) -f $dockerfile $root 

$projPath = $(Join-Path $root 'src');

docker run --rm -it -v "${projPath}:/src" $($containername)  --projectFile "/src/jdc.build.dependencyGraph/jdc.build.dependencyGraph.csproj"
