using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhadgarBot.Models
{
    public class GDQScheduleData
    {
        private string _runDate;
        private string _game;
        private string _runners;

        public GDQScheduleData(string date, string game, string runners)
        {
            RunDate = date;
            Game = game;
            Runners = runners;
        }

        public string RunDate
        {
            get => _runDate;
            private set => _runDate = value;
        }

        public string Game
        {
            get => _game;
            private set => _game = value;
        }

        public string Runners
        {
            get => _runners;
            private set => _runners = value;
        }
    }
}
