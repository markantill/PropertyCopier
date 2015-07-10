@echo on
set config=%1
if "%config%" == "" (
   set config=Release
)

set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

if "%NuGet%" == "" (
	set NuGet=".nuget\nuget.exe"
)

REM Package restore
call %NuGet% restore PropertyCopier\packages.config -OutputDirectory %cd%\packages -NonInteractive

REM Build
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild PropertyCopier.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false

REM Unit tests
packages\NUnit.Runners.2.6.4\tools PropertyCopier.Tests\bin\%config%\PropertyCopier.Tests.dll
if not "%errorlevel%"=="0" goto failure

REM Package
mkdir Build
mkdir Build\net40
copy PropertyCopier\bin\%config%\PropertyCopier.dll Build\net40
copy PropertyCopier\bin\%config%\PropertyCopier.pdb Build\net40

call %NuGet% pack "PropertyCopier\PropertyCopier.csproj" -IncludeReferencedProjects -symbols -o Build\net40 -p Configuration=%config% %version%

:success
exit 0

:failure
exit -1





