@echo on

set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

if "%NuGet%" == "" (
	set NuGet=".nuget\nuget.exe"
)

REM Package restore
call %NuGet% restore PropertyCopier.Tests\packages.config -OutputDirectory %cd%\packages -NonInteractive

REM Build
msbuild PropertyCopier.sln /p:Configuration="Release 4.0" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false /tv:14.0
msbuild PropertyCopier.sln /p:Configuration="Release 4.5" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false /tv:14.0

REM Unit tests
"packages\NUnit.ConsoleRunner.3.6.1\tools\nunit3-console.exe" "PropertyCopier.Tests\bin\Release 4.5\PropertyCopier.Tests.dll"
if not "%errorlevel%"=="0" goto failure

REM Package
mkdir output\lib
mkdir output\lib\net40
mkdir output\lib\net45

copy "PropertyCopier\bin\Release 4.0\PropertyCopier.dll" "output\lib\net40"
copy "PropertyCopier\bin\Release 4.0\PropertyCopier.pdb" "output\lib\net40"

copy "PropertyCopier\bin\Release 4.5\PropertyCopier.dll" "output\lib\net45"
copy "PropertyCopier\bin\Release 4.5\PropertyCopier.pdb" "output\lib\net45"

cd output
call ..\%NuGet% pack PropertyCopier.nuspec -IncludeReferencedProjects -symbols -o ..\Build
cd ..

:success
echo YAY
REM exit 0

:failure
echo BOO
REM exit -1





