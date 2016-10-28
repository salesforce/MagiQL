@ECHO OFF

IF NOT EXIST ".\src\.paket\paket.exe" GOTO BootstrapPaket
GOTO RestoreBuildTools

:BootstrapPaket
pushd .\src\.paket
.\paket.bootstrapper.exe
popd
GOTO RestoreBuildTools

:RestoreBuildTools
pushd .\src
.\.paket\paket.exe restore group Build
popd
GOTO RunFake

:RunFake
.\tools\FAKE\Fake.exe %*