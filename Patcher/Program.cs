using System;
using Avalonia;
using Avalonia.ReactiveUI;

namespace Patcher
{
    
    // To Publish: 
    // dotnet publish -r win-x64 -c Release -p:PublishReadyToRun=true --self-contained -o c:\tmp\publish -p:PublishSingleFile=true -p:DebugType=embedded -p:IncludeAllContentForSelfExtract=true
    // "C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe" sign /t http://timestamp.sectigo.com c:\tmp\publish\Patcher.exe
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();

    }
}
