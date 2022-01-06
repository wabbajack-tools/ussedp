dotnet publish Patcher -r win-x64 -c Release -p:PublishReadyToRun=true --self-contained -o c:\tmp\Patcher -p:PublishSingleFile=true -p:DebugType=embedded -p:IncludeAllContentForSelfExtract=true
dotnet run --project MakeSelfContained c:\tmp\Patcher\Patcher.exe c:\tmp\upload\patch c:\tmp\finalUpload\FullPatcher.exe
dotnet run --project MakeSelfContained c:\tmp\Patcher\Patcher.exe c:\tmp\uploadMini\patch c:\tmp\finalUpload\MiniPatcher.exe
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe" sign /t http://timestamp.sectigo.com c:\tmp\finalUpload\FullPatcher.exe
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe" sign /t http://timestamp.sectigo.com c:\tmp\finalUpload\MiniPatcher.exe