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
            User dm = Data.GetUser(Context.Message.Author);
            Channel channel = Data.GetChannel(Context.Message.Channel as SocketChannel);
            Event curEvent = new Event(dm, channel);

            await ReplyAsync($"New event **{curEvent.Id}** created!\nType ```!event join {curEvent.Id}``` to join.");
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

            await ReplyAsync(curEvent.Start(Data.GetUser(Context.Message.Author)));
        }

        // Join an event
        [Command("join")]
        [Summary("Join an event without a character.")]
        public async Task JoinEvent(string eventId)
        {
            Event curEvent = Data.GetEvent(Context.Message.Channel.Id, eventId);
            if (curEvent == null)
            {
                await ReplyAsync("Could not find that event.");
                return;
            }

            await ReplyAsync(curEvent.Join(Data.GetUser(Context.Message.Author)));
        }

        // Join an event
        [Command("join")]
        [Summary("Join an event with a character.")]
        public async Task JoinEvent(string eventId, string alias)
        {
            User user = Data.GetUser(Context.Message.Author);

            Event curEvent = Data.GetEvent(Context.Message.Channel.Id, eventId);
            if (curEvent == null)
            {
                await ReplyAsync("Could not find that event.");
                return;
            }

            Character character = user.GetCharacter(alias);
            if (character == null)
            {
                await ReplyAsync("Could not find that character.");
                return;
            }

            await ReplyAsync(curEvent.Join(character, user));
        }

        // Add x to the event
        [Group("add")]
        [Summary("Add characters or users to the event.")]
        public class EventAddModule : ModuleBase<SocketCommandContext>
        {
            // Add a character
            [Command("character")]
            [Summary("Add a character to the event.")]
            public async Task AddCharacter(string alias, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);

                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                Character character = Data.users[Context.Message.Author.Id]?.GetCharacter(alias);

                if (character == null)
                {
                    await ReplyAsync("Could not find a character with that alias.");
                    return;
                }
                
                await ReplyAsync(curEvent.AddCharacter(character));
            }

            // Create a new character or NPC for this event
            [Group("new")]
            [Summary("Create a new character or NPC for this event.")]
            public class EventNewModule : ModuleBase<SocketCommandContext>
            {

                // Create a new character for this event
                [Command("character")]
                [Summary("Create a new character for this event.")]
                public async Task NewCharacter(string eventId, [Remainder] string name)
                {
                    Event curEvent = Data.GetEvent(Context, eventId);

                    if (!Data.users.TryGetValue(Context.Message.Author.Id, out User user))
                    {
                        user = new User(Context.Message.Author);
                    }

                    Character character = new Character(name, 1, 1, user);
                    await ReplyAsync(curEvent.AddCharacter(character));
                }

                // Create a new NPC for this event
                [Command("npc")]
                [Summary("Create a new NPC for this event.")]
                public async Task NewNpc(string eventId, [Remainder] string name)
                {
                    Event curEvent = Data.GetEvent(Context, eventId);

                    if (!Data.users.TryGetValue(Context.Message.Author.Id, out User user))
                    {
                        user = new User(Context.Message.Author);
                    }

                    Character character = new Character(name, 1, 1, user)
                    {
                        Npc = true
                    };
                    await ReplyAsync(curEvent.AddCharacter(character));
                }
            }
        }

        // Set event details
        [Group("set")]
        [Summary("All commands for setting specific properties of the event.")]
        public class EventSetModule : ModuleBase<SocketCommandContext>
        {

            // Changing the ruleset of the event
            [Command("ruleset")]
            [Summary("Sets the ruleset for the event, can only be done before the event starts.")]
            public async Task SetRuleset(string rulesetID)
            {
                if (!Data.users.TryGetValue(Context.Message.Author.Id, out User user) || !user.activeEvents.TryGetValue(Context.Channel.Id, out Event curEvent))
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to set the ruleset for this event.");
                    return;
                }


                switch (rulesetID.ToLowerInvariant())
                {
                    case "amentia":
                    case "amentia's":
                    case "amentia's ruleset":
                        curEvent.Ruleset = new AmentiaRuleset();
                        await ReplyAsync($"Ruleset for the event {curEvent.Id} has changed to {curEvent.Ruleset.Name}.");
                        break;

                    default:
                        await ReplyAsync("Unknown ruleset.");
                        break;
                }
            }
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
