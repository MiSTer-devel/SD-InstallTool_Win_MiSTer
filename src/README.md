# SD-InstallTool_Win_MiSTer source code

## Development tools
- Visual Studio 2017

## Pre-Requisites
- Windows SDK (7.0a or newer)

## NuGet packages
- SharpZipLib - Unpack file-based packages - https://github.com/icsharpcode/SharpZipLib
- Fody - Dependency for Fody Costura - https://github.com/Fody/Fody/
- Fody Costura - Embed dependency assemblies as resources into main one - https://github.com/Fody/Costura

## Source code structure

/misterhelper - C/C++ native Dll providing bridge to VDS Interfaces [read more](https://msdn.microsoft.com/en-us/library/windows/desktop/aa383370(v=vs.85).aspx)

/sdinstalltool - Winforms/C# main project

/tests - limited set of functional tests

## How to build

TBD
