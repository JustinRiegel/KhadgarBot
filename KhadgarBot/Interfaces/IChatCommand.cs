using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace KhadgarBot.Interfaces
{
    interface IChatCommand
    {
        //bool CanProcess(ChatMessage chatMessage);
        //Task<bool> CanProcessAsync(ChatMessage chatMessage);

        void ProcessMessage(ChatMessage chatMessage);
    }
}
