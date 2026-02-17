@echo off
setlocal
cls

echo ========================================
echo MeleePerspectives Compiler
echo ========================================
echo Which would you like to compile to?
echo.
echo 1) NAOT (Small)
echo 2) R2R (Portable + Reflection)
echo.

set /p choice="Enter selection (1 or 2): "

if "%choice%"=="1" goto :compile_naot
if "%choice%"=="2" goto :compile_r2r

echo Invalid selection. Exiting.
pause
exit /b

:compile_naot
echo.
echo Compiling with Native AOT...
dotnet publish MeleePerspectives.csproj -c Release -r win-x64 --self-contained true -p:PublishAot=true
goto :end

:compile_r2r
echo.
echo Compiling MeleeThirdPerson with Ready-to-Run...
dotnet publish MeleePerspectives.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:PublishTrimmed=true 
:: -p:IncludeNativeLibrariesForSelfExtract=true
goto :end

:end
echo.
echo Done! Check your bin\Release\net8.0\win-x64\publish folder.
pause