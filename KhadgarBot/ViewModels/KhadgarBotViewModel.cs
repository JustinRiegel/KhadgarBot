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
using KhadgarBot.Models.Commands;

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

            SetUpHeroDataList();
            //CheckForHeroesTalentDatabase();

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
                //Model.Client.SendMessage("Knowledge is power.");
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

            //TODO: change this path to point to wherever in the solution i store the json files
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

                //for the heroes with multiple "profiles", such as abathur and abathur's hat, we need to loop over
                //all of them so we get the entire moveset
                foreach (var abilityProfile in jsonObj["abilities"])
                {
                    //this will only have one entry in it per profile. i think...
                    foreach (var profile in abilityProfile)
                    {
                        //loop over each profile and add it to the ability list
                        foreach (var ability in profile)
                        {
                            abilityList.Add(JsonConvert.DeserializeObject<Ability>(Convert.ToString(ability)));
                        }
                    }
                }

                //im using tiers (1,2,3,...) instead of level (1,4,7,...) because its more consistent when considering
                //heroes like chromie who get talents at different levels
                var talentTierNumber = 1;

                //talents are organized by tier, so loop over each tier to get the talents from each
                foreach (var talentTier in jsonObj["talents"])
                {
                    //this will only have one entry in it per tier. i think...
                    foreach (var talents in talentTier)
                    {
                        //loop over each talent in the tier and add it to the talent list
                        foreach (var talent in talents)
                        {
                            //add the talent tier to the json data
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

            Guid heroGuid;
            SQLiteCommand sqLiteHeroDataCommand;
            var sqLiteHeroInsertCommandText = @"INSERT INTO Hero VALUES ('{0}',{1},'{2}','{3}','{4}','{5}','{6}','{7}')";
            var sqLiteAbilityInsertCommandText = @"INSERT INTO Ability VALUES ('{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}',{8})";
            var sqLiteTalentInsertCommandText = @"INSERT INTO Talent VALUES ('{0}','{1}',{2},'{3}','{4}','{5}',{6},{7},{8})";
            //var temp = String.Format(sqLiteAbilityInsertCommandText,
            //            Guid.NewGuid(), Guid.NewGuid(), _heroDataList[0].Abilities[0].Name, _heroDataList[0].Abilities[0].Description, _heroDataList[0].Abilities[0].Hotkey, _heroDataList[0].Abilities[0].AbilityId,
            //            _heroDataList[0].Abilities[0].Cooldown, _heroDataList[0].Abilities[0].ManaCost, Convert.ToInt32(Convert.ToBoolean(_heroDataList[0].Abilities[0].Trait)));
            foreach (var hero in _heroDataList)
            {
                heroGuid = Guid.NewGuid();
                sqLiteHeroDataCommand = new SQLiteCommand(String.Format(sqLiteHeroInsertCommandText,
                Guid.NewGuid(), hero.Hero.HeroId, hero.Hero.ShortName,
                hero.Hero.AttributeId, hero.Hero.Name, hero.Hero.Role,
                hero.Hero.Type, DateTime.Now), _sqLiteConnection);
                sqLiteHeroDataCommand.ExecuteNonQuery();

                foreach(var ability in hero.Abilities)
                {
                    sqLiteHeroDataCommand = new SQLiteCommand(String.Format(sqLiteAbilityInsertCommandText,
                        Guid.NewGuid(), heroGuid, ability.Name, ability.Description, ability.Hotkey, ability.AbilityId,
                        ability.Cooldown, ability.ManaCost, Convert.ToInt32(Convert.ToBoolean(ability.Trait))), _sqLiteConnection);
                    sqLiteHeroDataCommand.ExecuteNonQuery();
                }

                foreach(var talent in hero.Talents)
                {
                    sqLiteHeroDataCommand = new SQLiteCommand(String.Format(sqLiteAbilityInsertCommandText,
                        Guid.NewGuid(), heroGuid, talent.TalentTier, talent.TooltipId, talent.TalentTreeId,
                        talent.Name, talent.Description, talent.Sort, talent.AbilityId), _sqLiteConnection);
                    sqLiteHeroDataCommand.ExecuteNonQuery();
                }
            }
            
            _sqLiteConnection.Close();
        }

        #endregion
    }
}
