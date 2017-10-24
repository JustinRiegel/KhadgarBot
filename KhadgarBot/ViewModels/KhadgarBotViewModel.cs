using System;
using System.Windows;
using KhadgarBot.Enums;
using Prism.Commands;
using System.ComponentModel.Composition;
using KhadgarBot.Models;
using System.Linq;
using System.Xml.Linq;
using TwitchLib.Events.Client;
using System.Collections.Generic;
using System.Data.SQLite;
using KhadgarBot.Interfaces;
using System.IO;
using Newtonsoft.Json;

namespace KhadgarBot.ViewModels
{
    //possible functionality:
    //death counter, resets on stream death
    //timer, mod level
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class KhadgarBotViewModel : DependencyObject
    {
        #region Members

        private const string KHADGARBOT_SQLITE_DBNAME = "KhadgarBot.sqlite";

        private List<HeroData> _heroDataList = new List<HeroData>();
        private SQLiteConnection _sqLiteConnection = new SQLiteConnection($"Data Source={KHADGARBOT_SQLITE_DBNAME};Version=3;");
        private List<IChatCommand> _chatCommandList = new List<IChatCommand>();        
        
        #endregion

        #region Constructor

        [ImportingConstructor]
        public KhadgarBotViewModel()
        {
            //grab the bot's login info from an xml file so sensitive info isn't publically posted
            var xmlLoginInfo = XDocument.Load(@"..\..\Resources\loginInfo.xml");
            var root = xmlLoginInfo.Descendants("root");
            var botNickname = root.Descendants("nick").First().Value;
            var botOAuth = root.Descendants("pass").First().Value;
                        
            ConnectedToTwitch = false;
            _chatCommandList.Add(new ChatPollCommand(this));

            CheckForHeroesTalentDatabase();

            Model = new KhadgarBotModel(botNickname, botOAuth);
            BotAdminViewModel = new BotAdminViewModel(this);
            CommandLogViewModel = new CommandLogViewModel(this);

            ChangeTabCallback = new DelegateCommand<object>(ExecuteChangeTab);
            ConnectToTwitch = new Action(ExecuteConnectToTwitch);
            JoinChannel = new Action<string>(ExecuteJoinChannel);
            LeaveChannel = new Action<string>(ExecuteLeaveChannel);
        }

        #endregion

        #region Properties

        public KhadgarBotModel Model { get; set; }
        public BotAdminViewModel BotAdminViewModel { get; set; }
        public CommandLogViewModel CommandLogViewModel { get; set; }

        public TabNameEnum SelectedTabIndex
        {
            get { return (TabNameEnum)GetValue(SelectedTabIndexProperty); }
            set { SetValue(SelectedTabIndexProperty, value); }
        }

        public bool ConnectedToTwitch
        {
            get { return (bool)GetValue(ConnectedToTwitchProperty); }
            set { SetValue(ConnectedToTwitchProperty, value); }
        }

        private static readonly DependencyProperty SelectedTabIndexProperty =
            DependencyProperty.Register("SelectedTabIndex", typeof(TabNameEnum), typeof(KhadgarBotViewModel), new PropertyMetadata(TabNameEnum.BotAdmin));

        private static readonly DependencyProperty ConnectedToTwitchProperty =
            DependencyProperty.Register("ConnectedToTwitch", typeof(bool), typeof(KhadgarBotViewModel), new PropertyMetadata(false));

        #endregion

        #region Commands

        public DelegateCommand<object> ChangeTabCallback { get; set; }

        public void ExecuteChangeTab(object selectedIndex)
        {
            SelectedTabIndex = (TabNameEnum)selectedIndex;
        }

        public Action ConnectToTwitch { get; set; }

        public void ExecuteConnectToTwitch()
        {
            Model.Client.OnJoinedChannel += onJoinedChannel;
            Model.Client.Connect();
            ConnectedToTwitch = true;
        }

        public Action<string> JoinChannel { get; set; }

        public void ExecuteJoinChannel(string channelName)
        {
            Model.Client.JoinChannel(channelName);
            Model.Client.OnMessageReceived += onMessageReceived;
        }

        public Action<string> LeaveChannel { get; set; }

        public void ExecuteLeaveChannel(string channelName)
        {
            Model.Client.LeaveChannel(channelName);
            Model.Client.OnMessageReceived -= onMessageReceived;
        }

        public void SendChatMessage(string message)
        {
            Dispatcher.Invoke(new Action(() => { Model.Client.SendMessage(message); }));
        }

        #endregion

        #region Methods

        private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Dispatcher.Invoke(new Action(() => {
                Model.Client.OnJoinedChannel -= onJoinedChannel;
                Model.Client.SendMessage("Knowledge is power.");
            }));
        }

        private void onMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            //DisplayName
            //Username
            //Message
            //Channel
            //_messages.Add(e.ChatMessage);

            var chatMessage = e.ChatMessage;

            foreach(var chatCommand in _chatCommandList)
            {
                if (chatCommand.CanProcess(chatMessage))
                    break;
            }
        }

        private void SetUpHeroDataList()
        {
            string jsonStringData;

            foreach (var file in Directory.EnumerateFiles(@"D:\GitHub Repos\heroes-talents\hero"))
            {
                using (var fs = new FileStream(file, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        jsonStringData = sr.ReadToEnd();
                    }
                }

                dynamic jsonObj = JsonConvert.DeserializeObject(jsonStringData);

                var hero = JsonConvert.DeserializeObject<Hero>(Convert.ToString(jsonObj));
                var abilityList = new List<Ability>();
                var talentList = new List<Talent>();

                foreach (var abilityProfile in jsonObj["abilities"])
                {
                    foreach (var profile in abilityProfile)
                    {
                        foreach (var ability in profile)
                        {
                            abilityList.Add(JsonConvert.DeserializeObject<Ability>(Convert.ToString(ability)));
                        }
                    }
                }

                var talentTierNumber = 1;
                foreach (var talentTier in jsonObj["talents"])
                {
                    foreach (var talents in talentTier)
                    {
                        foreach (var talent in talents)
                        {
                            talent["talentTier"] = $"{talentTierNumber}";
                            talentList.Add(JsonConvert.DeserializeObject<Talent>(Convert.ToString(talent)));
                        }
                    }
                    ++talentTierNumber;
                }

                _heroDataList.Add(new HeroData(hero, abilityList, talentList));
            }
        }

        private void CheckForHeroesTalentDatabase()
        {
            if(!File.Exists(KHADGARBOT_SQLITE_DBNAME))
            {
                SQLiteConnection.CreateFile(KHADGARBOT_SQLITE_DBNAME);
            }

            _sqLiteConnection.Open();

            var sqLiteCheckHeroTableExists = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'Hero';";
            var sqLiteCheckAbilityTableExists = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'Ability';";
            var sqLiteCheckTalentTableExists = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'Talent';";

            if (!(new SQLiteCommand(sqLiteCheckHeroTableExists, _sqLiteConnection)).ExecuteReader().Read())
            {
                var sqLiteCreateHeroTableCommandText = @"CREATE TABLE Hero (
                Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
                HeroId INTEGER NOT NULL,
                ShortName VARCHAR(50)     NOT NULL,
                AttributeId VARCHAR(50)     NOT NULL,
                Name        VARCHAR(50)     NOT NULL,
                Role        VARCHAR(50)     NOT NULL,
                Type        VARCHAR(50)     NOT NULL,
                ReleaseDate DATE NOT NULL
                );";
                var sqLiteCreateHeroTableCommand = new SQLiteCommand(sqLiteCreateHeroTableCommandText, _sqLiteConnection);
                sqLiteCreateHeroTableCommand.ExecuteNonQuery();
            }

            if (!(new SQLiteCommand(sqLiteCheckAbilityTableExists, _sqLiteConnection)).ExecuteReader().Read())
            {
                var sqLiteCreateAbilityTableCommandText = @"CREATE TABLE Ability (
                    Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
                    HeroId UNIQUEIDENTIFIER REFERENCES Hero (Id) NOT NULL,
                    Name VARCHAR (50) NOT NULL,
                    Description VARCHAR (2000) NOT NULL,
                    Hotkey CHAR (1),
                    AbilityId VARCHAR (50) NOT NULL,
                    Cooldown INTEGER,
                    ManaCost VARCHAR (10),
                    Trait BIT
                    );";
                var sqLiteCreateAbilityTableCommand = new SQLiteCommand(sqLiteCreateAbilityTableCommandText, _sqLiteConnection);
                sqLiteCreateAbilityTableCommand.ExecuteNonQuery();
            }

            if (!(new SQLiteCommand(sqLiteCheckTalentTableExists, _sqLiteConnection)).ExecuteReader().Read())
            {
                var sqLiteCreateTalentTableCommandText = @"CREATE TABLE Talent (
                    Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
                    HeroId UNIQUEIDENTIFIER REFERENCES Hero (Id) NOT NULL,
                    TalentTier INTEGER NOT NULL,
                    TooltipId VARCHAR (100) NOT NULL,
                    TalentTreeId VARCHAR (100) NOT NULL,
                    Name VARCHAR (50) NOT NULL,
                    Description VARCHAR (500) NOT NULL,
                    Sort INTEGER NOT NULL,
                    AbilityId VARCHAR (50) NOT NULL
                    );";
                var sqLiteCreateTalentTableCommand = new SQLiteCommand(sqLiteCreateTalentTableCommandText, _sqLiteConnection);
                sqLiteCreateTalentTableCommand.ExecuteNonQuery();
            }
            
            _sqLiteConnection.Close();
        }

        #endregion
    }
}
