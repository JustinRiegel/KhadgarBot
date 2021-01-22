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

        

        #endregion
    }
}
