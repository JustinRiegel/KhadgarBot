using KhadgarBot.Interfaces;
using KhadgarBot.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace KhadgarBot.Models.Commands
{
    class HeroesInfoCommand : IChatCommand
    {
        #region Members

        private KhadgarBotViewModel _khadgarBotViewModel;

        #endregion

        #region Constructor

        public HeroesInfoCommand(KhadgarBotViewModel khadgarBotViewModel)
        {
            _khadgarBotViewModel = khadgarBotViewModel;
        }

        #endregion

        #region Commands

        public bool CanProcess(ChatMessage chatMessage)
        {
            return CanProcessAsync(chatMessage).Result;
        }

        public async Task<bool> CanProcessAsync(ChatMessage chatMessage)
        {
            if(chatMessage.Message.Substring(0, 2) == "[[" && chatMessage.Message.Substring(chatMessage.Message.Length - 2, 2) == "]]")
            {
                GetHeroesData(chatMessage.Message);
                return true;
            }
            return false;
        }

        #endregion

        #region Methods

        private void GetHeroesData(string message)
        {
            var userInput = message.Substring(2, message.Length - 4);
            var result = _khadgarBotViewModel.GetInfoFromHeroesLibrarian(userInput);

            foreach(var item in result)
            {
                _khadgarBotViewModel.SendChatMessage(item);
                Thread.Sleep(1000);//boy howdy, do i hate doing this. fortunately its just for testing, but damn, i still hate it
            }
            //var resultsTimer = new Timer(1000);
            //resultsTimer.Elapsed += (sender, e) => ResultsTimer_Elapsed(sender, e, result);
            //resultsTimer.Start();
        }

        //private void ResultsTimer_Elapsed(object sender, ElapsedEventArgs e, List<string> results)
        //{
        //    if (results.FirstOrDefault() != null)
        //    {
        //        _khadgarBotViewModel.SendChatMessage(results.First());
        //        results.RemoveAt(0);
        //    }

        //    if(results.Count > 0)
        //    {
                
        //    }
        //}

        #endregion
    }
}
