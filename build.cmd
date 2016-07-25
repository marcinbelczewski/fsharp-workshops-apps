@echo off
cls
.paket\paket.bootstrapper.exe
.paket\paket.exe restore
packages\Build\FAKE\tools\FAKE.exe build.fsx %*