using System;
using System.Collections.Generic;
using System.Text;

namespace RP_Bot
{
    using Discord.Commands;
    using Discord.WebSocket;
    using System.Threading.Tasks;

    [Group("rphelp")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        // Help
        

        // Ping
        [Command("ping")]
        [Summary("Verifies that the bot functions correctly.")]
        public async Task Ping()
        {
            await ReplyAsync("Pong!");
        }
    }

    [Group("event")]
    public class EventModule : ModuleBase<SocketCommandContext>
    {
        // Create an event
        [Command("create")]
        [Summary("Creates a new RP event.")]
        public async Task CreateEvent()
        {
            SocketUser dm = Context.Message.Author;
            SocketChannel channel = (Context.Message.Channel as SocketChannel);
            Event curEvent = new Event(dm, channel);
            
            await ReplyAsync($"New event *{curEvent.Id}* created!\nType \"!event join {curEvent.Id}\" to join.");
        }


        // Start an event
        [Command("start")]
        [Summary("Starts an existing RP event.")]
        public async Task StartEvent(string eventId)
        {
            ulong channelId = Context.Message.Channel.Id;
            Event curEvent = Data.GetEvent(channelId, eventId);
            if (curEvent == null)
            {
                await ReplyAsync($"Could not find any event in this channel with that ID.");
                return;
            }
            
            await ReplyAsync(curEvent.Start(Context.Message.Author));
        }


        // Character commands
        [Group("character")]
        [Summary("All character related commands per event.")]
        public class EventCharacterModule : ModuleBase<SocketCommandContext>
        {

            // Setting character traits on per-event basis
            [Group("set")]
            public class EventCharacterSetModule : ModuleBase<SocketCommandContext>
            {
                // Set max health
                [Command("maxhealth")]
                [Summary("Sets the max health for the given character during this event.")]
                public async Task MaxHealth(string charId, int health)
                {

                }
            }
        }
    }

    [Group("character")]
    [Summary("All global character related commands.")]
    public class CharacterModule : ModuleBase<SocketCommandContext>
    {

    }
}
