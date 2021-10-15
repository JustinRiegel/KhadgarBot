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
using KhadgarBot.Interfaces;
using System.IO;
using Newtonsoft.Json;
using TwitchLib.Client.Events;
//using HtmlAgilityPack;
using HeroesInfoLibrary;
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;

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

        //need a lock for a command to grab when its going to be sending messages. separate lock for messages and whispers?
        //need a lock per message send attempt, maybe?
        //add lock statement to sending message/whisper

        //do i want the command threads to execute in their entirety before releasing the lock?
        //--if i go this route, do i want a "priority" message to be able to temporarily grab the lock and let the user know its working?
        //or do i want it to only grab a lock per message/whisper send attempt and let the threads have a free-for-all for who can send the next message?
        //--add a "complete" message to the results list to let the user know all the data has been returned, if i do this one
        
        private List<IChatCommand> _chatCommandList = new List<IChatCommand>();
        //private List<GDQScheduleData> _sechduleData = new List<GDQScheduleData>();
        private HeroesLibrarian _heroesLibrarian;

        private bool _arePredsRunning = false;
        private string _connectedChannelName = string.Empty;

        private object _messageLock = new object();
        private List<string> _messageSendQueue = new List<string>();
        private Timer _messageSendTimer;
        private int _messageSendTimerInterval = 750;//able to send a message every 3/4ths of a second

        private object _whisperLock = new object();
        private List<KeyValuePair<string, string>> _whisperSendQueue = new List<KeyValuePair<string, string>>();
        private Timer _whisperSendTimer;
        private int _whisperSendTimerInterval = 750;//able to send a whisper every 3/4ths of a second

        #endregion

        #region Constructor

        [ImportingConstructor]
        public KhadgarBotViewModel()
        {
            //grab the bot's login info from an xml file so sensitive info isn't publically posted
            var xmlLoginInfo = XDocument.Load(@"..\..\..\Resources\loginInfo.xml");
            var root = xmlLoginInfo.Descendants("root");
            var botNickname = root.Descendants("nick").First().Value;
            var botOAuth = root.Descendants("pass").First().Value;
                        
            ConnectedToTwitch = false;
            
            
            Model = new KhadgarBotModel(botNickname, botOAuth);
            BotAdminViewModel = new BotAdminViewModel(this);
            CommandLogViewModel = new CommandLogViewModel(this);

            _heroesLibrarian = new HeroesLibrarian("../../../../../Heroes-talents/heroes-talents-master/hero", "HeroesOfTheStormHeroData.db");

            _messageSendTimer = new Timer(_messageSendTimerInterval);
            _messageSendTimer.Elapsed += MessageSendTimer_Elapsed;
            _whisperSendTimer = new Timer(_whisperSendTimerInterval);
            _whisperSendTimer.Elapsed += WhisperSendTimer_Elapsed;
            //var result = _heroesLibrarian.GetAbilityAndTalentDataByString("sand blast");
            //var temp = 0;/

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
            Model.Client.OnLeftChannel += onLeftChannel;
            Model.Client.Connect();
            ConnectedToTwitch = true;
            SetupChatCommands();
        }

        public Action<string> JoinChannel { get; set; }

        public void ExecuteJoinChannel(string channelName)
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

        private void SendChatMessage(string message)
        {
            Dispatcher.Invoke(new Action(() => { Model.Client.SendMessage(_connectedChannelName, message); }), System.Windows.Threading.DispatcherPriority.Normal);
        }

        private void SendWhisper(string receivingUsername, string message)
        {
            Dispatcher.Invoke(new Action(() => { Model.Client.SendWhisper(receivingUsername, message); }), System.Windows.Threading.DispatcherPriority.Normal);
        }

        private void MessageSendTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_messageLock)
            {
                if (_messageSendQueue.Count > 0)
                {
                    SendChatMessage(_messageSendQueue.First());
                    _messageSendQueue.RemoveAt(0);
                }
            }
        }

        private void WhisperSendTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_whisperLock)
            {
                if (_whisperSendQueue.Count > 0)
                {
                    SendWhisper(_whisperSendQueue.First().Key, _whisperSendQueue.First().Value);
                    _whisperSendQueue.RemoveAt(0);
                }
            }
        }

        #endregion

        #region Events

        private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Model.Client.OnJoinedChannel -= onJoinedChannel;
                _messageSendTimer.Start();
                _whisperSendTimer.Start();
                _messageSendQueue = new List<string>();
                _whisperSendQueue = new List<KeyValuePair<string, string>>();
                //SendChatMessage("Knowledge is power.");
            }));
        }

        private void onLeftChannel(object sender, OnLeftChannelArgs e)
        {
            Dispatcher.Invoke(new Action(() => {
                Model.Client.OnLeftChannel -= onLeftChannel;
                _messageSendTimer.Stop();
                _whisperSendTimer.Stop();
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
            Task.Run(() =>
            {
                //loop over the commands passing in the message and let the first one that can process it, do so, then break the loop
                foreach (var chatCommand in _chatCommandList)
                {
                    chatCommand.ProcessMessage(chatMessage);
                }
            });
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
            _chatCommandList.Add(new HeroesInfoCommand(this));
        }

        public void AddMessageToSendQueue(string message)
        {
            var messageAdded = false;
            while (!messageAdded)
            {
                messageAdded = AddToMessageList(message);
            } 
        }

        public void AddPriorityMessageSendToQueue(string message)
        {
            var messageAdded = false;
            while (!messageAdded)
            {
                messageAdded = AddToMessageList(message, true);
            }
        }

        private bool AddToMessageList(string message, bool priority = false)
        {
            lock (_messageLock)
            {
                if (!priority)
                {
                    _messageSendQueue.Add(message);
                    Debug.WriteLine("Message successfully added to send queue.");
                    return true;
                }
                else
                {
                    _messageSendQueue.Insert(0, message);
                    Debug.WriteLine("Priority message successfully added to send queue.");
                    return true;
                }
            }

            //this if block is only needed for debugging. will need the return statement after it though
            if (!priority)
            {
                Debug.WriteLine("Message failed to be added to send queue.");
            }
            else
            {
                Debug.WriteLine("Priority message failed to be added to send queue.");
            }
            return false;
        }

        public void AddWhisperToSendQueue(string user, string message)
        {
            var messageAdded = false;
            while(!messageAdded)
            {
                messageAdded = AddToWhisperList(user, message);
            }
        }

        public void AddPriorityWhisperToSendQueue(string user, string message)
        {
            var messageAdded = false;
            while (!messageAdded)
            {
                messageAdded = AddToWhisperList(user, message, true);
            }
        }

        private bool AddToWhisperList(string user, string message, bool priority = false)
        {
            lock (_whisperLock)
            {
                if (!priority)
                {
                    _whisperSendQueue.Add(new KeyValuePair<string, string>(user, message));
                    Debug.WriteLine("Whisper successfully added to send queue.");
                    return true;
                }
                else
                {
                    _whisperSendQueue.Insert(0, new KeyValuePair<string, string>(user, message));
                    Debug.WriteLine("Priority whisper successfully added to send queue.");
                    return true;
                }
            }

            //this if block is only needed for debugging. will need the return statement after it though
            if (!priority)
            {
                Debug.WriteLine("Whisper failed to be added to send queue.");
            }
            else
            {
                Debug.WriteLine("Priority whisper failed to be added to send queue.");
            }
            return false;
        }

        public List<string> GetInfoFromHeroesLibrarian(string input)
        {
            return _heroesLibrarian.GetAbilityAndTalentDataByString(input);
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
