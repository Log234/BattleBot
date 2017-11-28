using System;
using System.Collections.Generic;
using System.Text;

namespace RP_Bot
{
    using Discord.Commands;
    using Discord.WebSocket;
    using System.Threading.Tasks;

    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        
    }

    [Group("event")]
    public class EventModule : ModuleBase<SocketCommandContext>
    {

        // Start an event
        [Command("start")]
        [Summary("Starts a new RP event.")]
        public async Task CreateEvent()
        {
            SocketUser dm = Context.Message.Author;
            // TO-DO Implement logic

            string eventName = "Test";
            await ReplyAsync($"New event \"{eventName}\" started!\nType \"!event join {eventName}\" to join.");
        }
    }
}
