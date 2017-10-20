using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Models.Client;

namespace KhadgarBot.Interfaces
{
    interface IChatCommand
    {
        bool CanProcess(ChatMessage chatMessage);
    }
}
