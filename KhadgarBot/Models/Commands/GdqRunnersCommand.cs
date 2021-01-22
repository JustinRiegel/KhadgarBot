using HtmlAgilityPack;
using KhadgarBot.Interfaces;
using KhadgarBot.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.V5.Models.Streams;
using TwitchLib.Client.Models;

namespace KhadgarBot.Models.Commands
{
    public class GdqRunnersCommand : IChatCommand
    {
        #region Members
        
        private KhadgarBotViewModel _khadgarBotViewModel;
        private StreamByUser _currentStream;
        private List<GDQScheduleData> _scehduleData = new List<GDQScheduleData>();
        private TwitchAPI _twitchApi = new TwitchAPI();

        #endregion

        #region Constructor

        public GdqRunnersCommand(KhadgarBotViewModel khadgarBotViewModel)
        {
            _khadgarBotViewModel = khadgarBotViewModel;
            SetStreamByUser("gamesdonequick");
            ParseGDQSchedule();
        }

        #endregion

        #region Methods

        public bool CanProcess(ChatMessage chatMessage)
        {
            return CanProcessAsync(chatMessage).Result;
        }

        //not totally happy with how i have this working now, i want to make it a bit cleaner to be able to accept multiple command strings with different permission levels
        public async Task<bool> CanProcessAsync(ChatMessage chatMessage)
        {
            if (chatMessage.Message.ToLower() == "!runner" || chatMessage.Message.ToLower() == "!runners")
            {
                var message = "";
                var runnerString = "";
                List<string> runnerList = new List<string>();
                await SetStreamByUser("gamesdonequick");

                if(_currentStream != null && _currentStream.Stream != null)
                {
                    var matchingGames = _scehduleData.Where(g => g.Game == _currentStream.Stream.Game);
                    var temp = _currentStream.Stream.Channel;
                    if(matchingGames.Count() > 1)//the game is played more than once, so check the date
                    {
                        var firstMatch = matchingGames.FirstOrDefault(m => DateTime.Parse(m.RunDate).Date == DateTime.Now.Date);
                        if (firstMatch != null)
                        {
                            runnerString = firstMatch.Runners;
                        }
                    }
                    else if(matchingGames.Count() == 1)
                    {
                        runnerString = matchingGames.First().Runners;
                    }
                    else
                    {
                        //there was no match, so just silently fail
                        return true;
                    }
                    
                    if(runnerString.Contains(","))//multiple runners
                    {
                        runnerList.AddRange(runnerString.Split(','));
                    }
                    else
                    {
                        runnerList.Add(runnerString);
                    }

                    message = "Follow the runner(s) of " + _currentStream.Stream.Game + " on Twitch: " + Environment.NewLine;
                    foreach(var runner in runnerList)
                    {
                        message += ("https://twitch.tv/" + runner.Trim() + Environment.NewLine);
                    }

                    if(chatMessage.IsBroadcaster || chatMessage.IsModerator)
                    {
                        message += "I see that you are the streamer or a mod. If you would like me to stop whispering people, the command !killbot will stop me. I was created by Ciarenni, feel free to message him with inquiries.";
                    }

                    _khadgarBotViewModel.SendWhisper(chatMessage.Username, message);
                }
                return true;
            }
            else if ((chatMessage.Username == "ciarenni" || chatMessage.IsModerator || chatMessage.IsBroadcaster) &&chatMessage.Message.ToLower() == "!killbot")
            {
                File.Create("Bot ended by " + chatMessage.Username + ".txt");
                Environment.Exit(0);
            }
            return false;
        }

        public async Task SetStreamByUser(string channelName)
        {
            _twitchApi = new TwitchAPI();
            _twitchApi.Settings.AccessToken = "5fitjq7k64nd2vjvzu02p1k41slak6";
            var userList = _twitchApi.V5.Users.GetUserByNameAsync(channelName).Result.Matches;
            var userId = userList[0].Id;
            _currentStream = await _twitchApi.V5.Streams.GetStreamByUserAsync(userId);
            //_currentStream =  await api.V5.Streams.GetStreamByUserAsync(channelName);
        }

        private void ParseGDQSchedule()
        {
            int count = 0;
            string game = "";
            string runner = "";
            string date = "";
            var html = @"https://gamesdonequick.com/schedule";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            var runTableNode = htmlDoc.DocumentNode.SelectSingleNode("//table[@id='runTable']");

            foreach (HtmlNode row in runTableNode.Descendants().Where(n => n.Name.Equals("tr")))
            {
                var temp = row;
                if (row.Descendants().FirstOrDefault(d => d.Name.Equals("td")) != null)
                {
                    if (row.Descendants().FirstOrDefault(d => d.Name.Equals("td")).Attributes.Any(a => a.Value == "start-time text-right"))
                    {
                        //first node is start time
                        //second node is game
                        //third node is runner
                        //fourth node is setup time
                        count = 0;
                        game = "";
                        runner = "";
                        foreach (HtmlNode cell in row.Descendants().Where(n => n.Name.Equals("td")))
                        {
                            switch (count)
                            {
                                case 0:
                                    date = cell.InnerHtml;
                                    break;
                                case 1:
                                    game = cell.InnerHtml;
                                    game = game.Replace("&#039;", "'");
                                    break;
                                case 2:
                                    runner = cell.InnerHtml;
                                    break;
                            }
                            count++;
                        }
                        _scehduleData.Add(new GDQScheduleData(date, game, runner));
                    }
                }
            }
        }

        #endregion
    }
}
