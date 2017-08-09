using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KhadgarBot.Enums;
using Prism.Commands;
using TwitchLib;
using TwitchLib.Models.Client;
using System.ComponentModel.Composition;

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

        #endregion

        #region Constructor

        [ImportingConstructor]
        public KhadgarBotViewModel()
        {
            BotInfoView = new BotInfoViewModel(this);
            BotAdminView = new BotAdminViewModel(this);
            CommandLogView = new CommandLogViewModel(this);
            ChangeTabCallback = new DelegateCommand<object>(ExecuteChangeTab);

            //TODO combine the bot info and bot admin objects, there's no reason to separate them
            Credentials = new ConnectionCredentials(BotName, OAuth);
            Client = new TwitchClient(Credentials, ChannelName);
        }

        #endregion

        #region Properties

        public BotInfoViewModel BotInfoView { get; set; }
        public BotAdminViewModel BotAdminView { get; set; }
        public CommandLogViewModel CommandLogView { get; set; }

        public TwitchClient Client
        {
            get { return (TwitchClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        public ConnectionCredentials Credentials
        {
            get { return (ConnectionCredentials)GetValue(CredentialsProperty); }
            set { SetValue(CredentialsProperty, value); }
        }

        public TabNameEnum SelectedTabIndex
        {
            get { return (TabNameEnum)GetValue(SelectedTabIndexProperty); }
            set { SetValue(SelectedTabIndexProperty, value); }
        }

        private static readonly DependencyProperty SelectedTabIndexProperty = DependencyProperty.Register("SelectedTabIndex", typeof(TabNameEnum), typeof(KhadgarBotViewModel), new PropertyMetadata(TabNameEnum.BotInfo));
        private static readonly DependencyProperty ClientProperty = DependencyProperty.Register("Client", typeof(TwitchClient), typeof(KhadgarBotViewModel));
        private static readonly DependencyProperty CredentialsProperty = DependencyProperty.Register("Credentials", typeof(ConnectionCredentials), typeof(KhadgarBotViewModel));

        #endregion

        #region Commands

        public DelegateCommand<object> ChangeTabCallback { get; set; }

        public void ExecuteChangeTab(object selectedIndex)
        {
            SelectedTabIndex = (TabNameEnum)selectedIndex;
        }

        #endregion
    }
}
