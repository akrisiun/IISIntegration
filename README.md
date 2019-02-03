# BUILD 

```powershell
./run default-build


dotnet.exe msbuild -restore -t:noop -v:m $HOME\.dotnet\buildtools\korebuild\2.2.0-preview2-20181019.5\scripts/../modules/BundledPackages/BundledPackageRestorer.csproj
$HOME\.dotnet\buildtools\korebuild\2.2.0-preview2-20181019.5\scripts\common.psm1
# :11 char:9

Microsoft.AspNetCore.AspNetCoreModuleV2 2.2.0-preview3-35497
specified framework 'Microsoft.NETCore.App', version '2.2.0-rtm-27023-02' was not found.

sdk 2.2.100-preview3-009430
"Internal.AspNetCore.Sdk": "2.2.0-preview2-20181019.5"
~\.dotnet\x64

https://dotnetcli.blob.core.windows.net/dotnet/Sdk/2.2.100-preview3-009430/dotnet-sdk-2.2.100-preview3-009430-win-x64.zip
```

ASP.NET Core IISIntegration
========
This repo hosts the ASP.NET Core middleware for IIS integration and the ASP.NET Core Module.

This project is part of ASP.NET Core. You can find samples, documentation and getting started instructions for ASP.NET Core at the [Home](https://github.com/aspnet/home) repo.

## Building from source
1. Install prerequisites
   1. Visual Studio 2017
      1. Workload: `Desktop development with C++`
         1. Run `run.ps1 install vs` or install the following components
            1. Additional Component: `Windows 8.1 SDK and UCRT SDK`
            2. Additional Component: `Windows 10 SDK (10.0.15063.0) for Desktop C++ [x86 and x64]`
      2. Workload: `ASP.NET and web development`
2. Clone with submodules
   1. `git clone --recurse-submodules IISIntegration`
   2. OR run `git submodule update --init --recursive` after initial clone
3. `build.cmd`
