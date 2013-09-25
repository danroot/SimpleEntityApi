copy ..\SimpleEntityApi.Web\Scripts\SimpleEntityApi\SimpleEntityApi-0.1.js .\content\Scripts
copy ..\SimpleEntityApi.Web\bin\SimpleEntityApi.Library.dll .\lib\net40

del *.nupkg 

./NuGet.exe pack ./SimpleEntityApi.nuspec
$packageFile = dir SimpleEntityApi.*.nupkg
./NuGet.exe push $($packageFile.FullName)


