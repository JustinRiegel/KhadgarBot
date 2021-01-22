using System;
using System.Windows;
using KhadgarBot.Enums;
using Prism.Commands;
using System.ComponentModel.Composition;
using KhadgarBot.Models;
using KhadgarBot.Models.Commands;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
//using System.Data.SQLite;
using KhadgarBot.Interfaces;
using System.IO;
using Newtonsoft.Json;
using TwitchLib.Client.Events;
using HtmlAgilityPack;

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
        //private SQLiteConnection _sqLiteConnection = new SQLiteConnection($"Data Source={KHADGARBOT_SQLITE_DBNAME};Version=3;");
        private List<IChatCommand> _chatCommandList = new List<IChatCommand>();
        //private List<GDQScheduleData> _sechduleData = new List<GDQScheduleData>();

        private bool _arePredsRunning = false;
        private string _connectedChannelName = string.Empty;
        
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
            

            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
            //SQLitePCL.Batteries.Init();

            SetUpHeroDataList();
            CheckForHeroesTalentDatabase();

            Model = new KhadgarBotModel(botNickname, botOAuth);
            BotAdminViewModel = new BotAdminViewModel(this);
            CommandLogViewModel = new CommandLogViewModel(this);

            ChangeTabCallback = new DelegateCommand<object>(ExecuteChangeTab);
            ConnectToTwitch = new Action(ExecuteConnectToTwitch);
            JoinChannel = new Action<string>(ExecuteJoinChannel);
            LeaveChannel = new Action<string>(ExecuteLeaveChannel);

            //TestParse();
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

        //public StreamByUser CurrentStream
        //{
        //    get => (StreamByUser)GetValue(CurrentStreamProperty);
        //    set => SetValue(CurrentStreamProperty, value);
        //}

        private static readonly DependencyProperty SelectedTabIndexProperty =
            DependencyProperty.Register(nameof(SelectedTabIndex), typeof(TabNameEnum), typeof(KhadgarBotViewModel), new PropertyMetadata(TabNameEnum.BotAdmin));

        private static readonly DependencyProperty ConnectedToTwitchProperty =
            DependencyProperty.Register(nameof(ConnectedToTwitch), typeof(bool), typeof(KhadgarBotViewModel), new PropertyMetadata(false));

        //private static readonly DependencyProperty CurrentStreamProperty = DependencyProperty.Register("CurrentStream", typeof(StreamByUser), typeof(KhadgarBotViewModel), new UIPropertyMetadata());

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
            SetupChatCommands();
        }

        public Action<string> JoinChannel { get; set; }

        public async void ExecuteJoinChannel(string channelName)
        {
            Model.Client.JoinChannel(channelName);
            //CurrentStream = await TwitchAPI.Streams.v5.GetStreamByUserAsync(channelName);
            _connectedChannelName = channelName;
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
            Dispatcher.Invoke(new Action(() => { Model.Client.SendMessage(_connectedChannelName ?? "ciarenni", message); }));
        }

        public void SendWhisper(string receivingUsername, string message)
        {
            Dispatcher.Invoke(new Action(() => { Model.Client.SendWhisper(receivingUsername, message); }));
        }

        #endregion

        #region Events

        private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Dispatcher.Invoke(new Action(() => {
                Model.Client.OnJoinedChannel -= onJoinedChannel;
                //SendChatMessage("Knowledge is power.");
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

            //if (chatMessage.Message.Substring(0, 1) == "!")
            //{
                //loop over the commands passing in the message and let the first one that can process it, do so, then break the loop
                foreach (var chatCommand in _chatCommandList)
                {
                    if (chatCommand.CanProcessAsync(chatMessage).Result)
                        break;
                }
            //}
            //else if (_arePredsRunning)
            //{

            //}
        }

        #endregion

        #region Methods

        private void SetupChatCommands()
        {
            //_chatCommandList.Add(new ChatPollCommand(this));
            //_chatCommandList.Add(new GdqRunnersCommand(this));
            _chatCommandList.Add(new PredsCommand(this));
        }

        //public void StartPreds()
        //{
        //    _arePredsRunning = true;
        //}

        //public void StopPreds()
        //{
        //    _arePredsRunning = false;
        //}

        //private void TestParse()
        //{
        //    int count = 0;
        //    string game = "";
        //    string runner = "";
        //    string date = "";
        //    var html = @"https://gamesdonequick.com/schedule";
        //    HtmlWeb web = new HtmlWeb();
        //    var htmlDoc = web.Load(html);

        //    var runTableNode = htmlDoc.DocumentNode.SelectSingleNode("//table[@id='runTable']");

        //    foreach (HtmlNode row in runTableNode.Descendants().Where(n => n.Name.Equals("tr")))
        //    {
        //        var temp = row;
        //        if (row.Descendants().FirstOrDefault(d => d.Name.Equals("td")) != null)
        //        {
        //            if (row.Descendants().FirstOrDefault(d => d.Name.Equals("td")).Attributes.Any(a => a.Value == "start-time text-right"))
        //            {
        //                //first node is start time
        //                //second node is game
        //                //third node is runner
        //                //fourth node is setup time
        //                count = 0;
        //                game = "";
        //                runner = "";
        //                foreach(HtmlNode cell in row.Descendants().Where(n => n.Name.Equals("td")))
        //                {
        //                    switch(count)
        //                    {
        //                        case 0: date = cell.InnerHtml;
        //                            break;
        //                        case 1: game = cell.InnerHtml;
        //                            break;
        //                        case 2: runner = cell.InnerHtml;
        //                            break;
        //                    }
        //                    count++;
        //                }
        //                _sechduleData.Add(new GDQScheduleData(date, game, runner));
        //            }
        //        }
        //    }
        //}

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
            
            //if(!File.Exists(KHADGARBOT_SQLITE_DBNAME))
            //{
            //    SQLiteConnection.CreateFile(KHADGARBOT_SQLITE_DBNAME);
            //}

            //using (SQLiteConnection sqLiteConnection = new SQLiteConnection($"Data Source={KHADGARBOT_SQLITE_DBNAME};Version=3;"))
            //{
            //    sqLiteConnection.Open();

            //    var sqLiteCheckHeroTableExists = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'Hero';";
            //    var sqLiteCheckAbilityTableExists = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'Ability';";
            //    var sqLiteCheckTalentTableExists = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'Talent';";

            //    using (SQLiteDataReader reader = new SQLiteCommand(sqLiteCheckHeroTableExists, sqLiteConnection).ExecuteReader())
            //    {
            //        if(!reader.Read())
            //        {
            //            var sqLiteCreateHeroTableCommandText = @"CREATE TABLE Hero (
            //                Id          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
            //                HeroId      INTEGER             NOT NULL,
            //                ShortName   VARCHAR(50)         NOT NULL,
            //                AttributeId VARCHAR(50)         NOT NULL,
            //                Name        VARCHAR(50)         NOT NULL,
            //                Role        VARCHAR(50)         NOT NULL,
            //                Type        VARCHAR(50)         NOT NULL,
            //                ReleaseDate DATE                NOT NULL
            //                );";
            //            var sqLiteCreateHeroTableCommand = new SQLiteCommand(sqLiteCreateHeroTableCommandText, sqLiteConnection);
            //            sqLiteCreateHeroTableCommand.ExecuteNonQuery();
            //        }
            //    }

            //    using (SQLiteDataReader reader = new SQLiteCommand(sqLiteCheckAbilityTableExists, sqLiteConnection).ExecuteReader())
            //    {
            //        if (!reader.Read())
            //        {
            //            var sqLiteCreateAbilityTableCommandText = @"CREATE TABLE Ability (
            //                Id          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
            //                HeroId      UNIQUEIDENTIFIER    NOT NULL REFERENCES Hero (Id),
            //                Name        VARCHAR (50)        NOT NULL,
            //                Description VARCHAR (2000)      NOT NULL,
            //                Hotkey      CHAR (1),
            //                AbilityId   VARCHAR (50)        NOT NULL,
            //                Cooldown    INTEGER,
            //                ManaCost    VARCHAR (10),
            //                IsTrait       BIT
            //                );";
            //            var sqLiteCreateAbilityTableCommand = new SQLiteCommand(sqLiteCreateAbilityTableCommandText, sqLiteConnection);
            //            sqLiteCreateAbilityTableCommand.ExecuteNonQuery();
            //        }
            //    }

            //    using (SQLiteDataReader reader = new SQLiteCommand(sqLiteCheckTalentTableExists, sqLiteConnection).ExecuteReader())
            //    {
            //        if (!reader.Read())
            //        {
            //            var sqLiteCreateTalentTableCommandText = @"CREATE TABLE Talent (
            //                Id              UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
            //                HeroId          UNIQUEIDENTIFIER    NOT NULL REFERENCES Hero (Id),
            //                TalentTier      INTEGER             NOT NULL,
            //                TooltipId       VARCHAR (100)       NOT NULL,
            //                TalentTreeId    VARCHAR (100)       NOT NULL,
            //                Name            VARCHAR (50)        NOT NULL,
            //                Description     VARCHAR (2000)       NOT NULL,
            //                Sort            INTEGER             NOT NULL,
            //                AbilityId       VARCHAR (50)        NOT NULL
            //                );";
            //            var sqLiteCreateTalentTableCommand = new SQLiteCommand(sqLiteCreateTalentTableCommandText, sqLiteConnection);
            //            sqLiteCreateTalentTableCommand.ExecuteNonQuery();
            //        }
            //    }

            //    using (SQLiteCommand delCmd = new SQLiteCommand(sqLiteConnection))
            //    {
            //        delCmd.CommandText = "delete from Hero; delete from Ability; delete from Talent;";
            //        delCmd.ExecuteNonQuery();
            //    }

            //    string heroGuid;
            //    SQLiteCommand sqLiteHeroDataCommand;
            //    var sqLiteHeroInsertCommandText = @"INSERT INTO Hero VALUES ($Id, $HeroId, $ShortName, $AttributeId, $Name, $Role, $Type, $ReleaseDate)";
            //    var sqLiteAbilityInsertCommandText = @"INSERT INTO Ability (Id, HeroId, Name, Description, Hotkey, AbilityId, Cooldown, ManaCost, IsTrait) VALUES ($Id, $HeroId, $Name, $Description, $Hotkey, $AbilityId, $Cooldown, $ManaCost, $IsTrait)";
            //    var sqLiteTalentInsertCommandText = @"INSERT INTO Talent VALUES ($Id, $HeroId, $TalentTier, $TooltipId, $TalentTreeId, $Name, $Description, $Sort, $AbilityId)";

            //    foreach (var hero in _heroDataList)
            //    {
            //        heroGuid = Guid.NewGuid().ToString();
            //        sqLiteHeroDataCommand = new SQLiteCommand(sqLiteHeroInsertCommandText, sqLiteConnection);
            //        sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Id", heroGuid));
            //        sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$HeroId", hero.Hero.HeroId));
            //        sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$ShortName", hero.Hero.ShortName));
            //        sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$AttributeId", hero.Hero.AttributeId));
            //        sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Name", hero.Hero.Name));
            //        sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Role", hero.Hero.Role));
            //        sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Type", hero.Hero.Type));
            //        sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$ReleaseDate", hero.Hero.ReleaseDate.Date));
            //        sqLiteHeroDataCommand.ExecuteNonQuery();

            //        foreach (var ability in hero.Abilities)
            //        {
            //            //sqLiteHeroDataCommand = new SQLiteCommand(sqLiteAbilityInsertCommandText, sqLiteConnection);
            //            //sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Id", Guid.NewGuid().ToString()));
            //            //sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$HeroId", heroGuid));
            //            //sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Name", ability.Name));
            //            //sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Description", ability.Description));
            //            //sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Hotkey", ability.Hotkey));
            //            //sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$AbilityId", ability.AbilityId));
            //            //sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Cooldown", ability.Cooldown));
            //            //sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$ManaCost", ability.ManaCost));
            //            //sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$IsTrait", Convert.ToInt32(Convert.ToBoolean(ability.Trait))));
            //            //sqLiteHeroDataCommand.ExecuteNonQuery();
            //        }

            //        foreach (var talent in hero.Talents)
            //        {
            //            sqLiteHeroDataCommand = new SQLiteCommand(sqLiteTalentInsertCommandText, sqLiteConnection);
            //            sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Id", Guid.NewGuid().ToString()));
            //            sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$HeroId", heroGuid));
            //            sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$TalentTier", talent.TalentTier));
            //            sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$TooltipId", talent.TooltipId));
            //            sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$TalentTreeId", talent.TalentTreeId));
            //            sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Name", talent.Name));
            //            sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Description", talent.Description));
            //            sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$Sort", talent.Sort));
            //            sqLiteHeroDataCommand.Parameters.Add(new SQLiteParameter("$AbilityId", talent.AbilityId));
            //            sqLiteHeroDataCommand.ExecuteNonQuery();
            //        }
            //    }
            //}
        }

        #endregion
    }
}
