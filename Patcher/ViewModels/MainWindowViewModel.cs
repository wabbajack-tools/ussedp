using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameFinder.StoreHandlers.Steam;
using MessageBox.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Wabbajack.DTOs;
using Wabbajack.Paths;

namespace Patcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; }
        
        [Reactive]
        public AbsolutePath GamePath { get; set; }
        public MainWindowViewModel()
        {
            Activator = new ViewModelActivator();
            
            var tsk = LocateAndSetGame(Game.SkyrimSpecialEdition);

            

        }

        public async Task LocateAndSetGame(Game game)
        {
            try
            {
                var handler = new SteamHandler();
                handler.FindAllGames();

                var steamGame = handler.Games.First(g => game.MetaData().SteamIDs.Contains(g.ID));

                var path = steamGame.Path.ToAbsolutePath();
                GamePath = path;
            }
            catch (Exception ex)
            {
                var msg = MessageBoxManager.GetMessageBoxStandardWindow("Error",
                    $"Couldn't locate {game.MetaData().HumanFriendlyGameName} via Steam, exiting");
                await msg.Show();
                Environment.Exit(1);
                return;
            }
        }


    }
}
