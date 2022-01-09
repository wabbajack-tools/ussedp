using System.Reactive.Concurrency;
using System.Threading.Tasks;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Wabbajack.DTOs.Interventions;
using Wabbajack.Networking.Steam.UserInterventions;

namespace Patcher;

public class UserInterventionHandler : IUserInterventionHandler
{
    private readonly ILogger<UserInterventionHandler> _logger;

    public UserInterventionHandler(ILogger<UserInterventionHandler> logger)
    {
        _logger = logger;

    }

    public void Raise(IUserIntervention intervention)
    {
        RxApp.MainThreadScheduler.ScheduleAsync(intervention, async (_, i, _) =>
        {
            if (i is GetAuthCode ac)
            {
                string msg = "";
                if (ac.Type == GetAuthCode.AuthType.EmailCode)
                    msg = "Please input your SteamGuard Email Code";
                else
                {
                    msg = "Please input your Steam 2FA Code";
                }

                _logger.LogInformation("Got request for Auth Code");
                var msgbox = MessageBoxManager.GetMessageBoxInputWindow(new MessageBoxInputParams()
                {
                    ContentTitle = "SteamGuard is Activated",
                    ContentMessage = msg
                });
                var result = await msgbox.ShowDialog(App.MainWindow);
                ac.Finish(result.Message.Trim());
            }
            else
            {
                _logger.LogError("Unknown user intervention {Type}", i.GetType());
            }
        });
    }
}