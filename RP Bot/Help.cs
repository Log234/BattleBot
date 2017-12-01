using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BattleBot
{
    [Group("help")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        // Help
        [Command]
        [Summary("General help")]
        public async Task Help()
        {
            IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync("Type **!help <command>** to learn more about the different commands:\nfeedback\nevent\nattack\nheal\nward");
        }

        // Event
        [Group("event")]
        public class EventModule : ModuleBase<SocketCommandContext>
        {
            [Command]
            [Summary("Event help")]
            public async Task Help()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync("**!event <command>** allows you to create, join or alter events with these commands:\nevent create\nevent start\nevent finish\nevent join\nevent add <command>\nevent remove <command>\nevent set <command>");
            }

            [Command("create")]
            [Summary("Event creation help")]
            public async Task Create()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync("**!event create <name>** creates a new event and sets it active for you, so that following commands will be directed at this event.");
            }

            [Command("start")]
            [Summary("Event start help")]
            public async Task Start()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync("**!event start <event ID>** starts an event and sets it active for you.\nAfter you have added all the characters and prepared the event, use this command to start the combat.");
            }

            [Command("status")]
            [Summary("Event status help")]
            public async Task Status()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync("**!event status [event ID]** I'll read out some information about the event, such as the name, whether it has started and who have joined it. :books:");
            }


            [Command("finish")]
            [Summary("Event finish help")]
            public async Task Finish()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync("**!event finish <event ID>** ends an event and posts the resulting event log to the chat. Use this command when the event is finished and you no longer need it.");
            }

            [Command("join")]
            [Summary("Event join help")]
            public async Task Join()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync("**!event join <event ID>** allows you to join an event and sets it active for you, so that following commands wil be directed at this event.");
            }

            [Group("add")]
            public class EventAddModule : ModuleBase<SocketCommandContext>
            {
                [Command]
                [Summary("Event addition help")]
                public async Task Add()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event add <command>** allows you to add characters, NPCs, administrators and teams to an event with these commands:\nevent add admin\nevent add character\nevent add new <command>");
                }

                [Command("admin")]
                [Summary("Event add admin help")]
                public async Task AddAdmin()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event add admin <User tag>** allows you to add an admin to your event, who will have access to administrative commands such as **!event start <event ID>** and **!event set ruleset <ruleset ID>**.");
                }

                [Command("character")]
                [Summary("Event add character help")]
                public async Task AddCharacter()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event add character <character alias>** allows you to add one of your existing characters to this event.");
                }

                [Group("new")]
                public class EventAddNewModule : ModuleBase<SocketCommandContext>
                {
                    [Command]
                    [Summary("Event add new help")]
                    public async Task New()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync("**!event add new <command>** allows you create new characters, NPCs and teams for this event and they will be added automatically:\nevent add new character\nevent add new npc\nevent add new team");
                    }

                    [Command("character")]
                    [Summary("Event add new character help")]
                    public async Task NewCharacter()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync("**!event add new character <event ID> <name>** allows you to create a new character and add it to this event.\nThe character alias will by default be the first name of the character.");
                    }

                    [Command("npc")]
                    [Summary("Event add new npc help")]
                    public async Task NewNpc()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync("**!event add new npc <event ID> <name>** allows you to create a new NPC and add it to this event.\nThe event rounds does not wait for NPCs to spend their action points, when all normal characters have spent their points the round will end.");
                    }

                    [Command("team")]
                    [Summary("Event add new team help")]
                    public async Task NewTeam()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync("**!event add new team <event ID> <team name>** allows you to create a new team and add it to this event.\nCharacters and NPCs can be added to teams with !event set team <character alias> <team alias>. When only one team is left standing they win.");
                    }
                }
            }


            [Group("remove")]
            public class EventRemoveModule : ModuleBase<SocketCommandContext>
            {
                [Command]
                [Summary("Event remove help")]
                public async Task Remove()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event remove <command>** allows you remove characters, NPCs, teams and admins from this event:\nevent remove admin\nevent remove character\nevent remove npc\nevent remove team");
                }

                [Command("admin")]
                [Summary("Event remove admin help")]
                public async Task RemoveAdmin()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event remove admin <user tag>** removes the given admin from the event.");
                }

                [Command("character")]
                [Summary("Event remove character help")]
                public async Task RemoveCharacter()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event remove character <character alias>** removes the given character from the event.");
                }

                [Command("npc")]
                [Summary("Event remove npc help")]
                public async Task RemoveNpc()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event remove npc <character alias>** removes the given npc from the event.");
                }

                [Command("team")]
                [Summary("Event remove team help")]
                public async Task RemoveTeam()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event remove team <team alias>** removes the given team from the event.\nCharacters from the removed team will remain in the event, but not in any team until they are reassigned.");
                }
            }


            [Group("set")]
            public class EventSetModule : ModuleBase<SocketCommandContext>
            {
                [Command]
                [Summary("Event set help")]
                public async Task Remove()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event set <command>** allows you change details for this event:\nevent set ruleset\nevent set team\nevent set maxhealth\nevent set health");
                }

                [Command("ruleset")]
                [Summary("Event set ruleset help")]
                public async Task SetRuleset()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event set ruleset <ruleset ID>** changes the event ruleset.\nAvailable rulesets:\n Amentia's ruleset - **amentia**");
                }

                [Command("team")]
                [Summary("Event set team help")]
                public async Task SetTeam()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event set team <character alias> <team alias>** assigns the character to the given team, a character can only be in one team at a time.");
                }

                [Command("maxhealth")]
                [Summary("Event set maxhealth help")]
                public async Task SetMaxhealth()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event set maxhealth <character alias> <health>** sets the character's max health to the given value.");
                }

                [Command("health")]
                [Summary("Event set health help")]
                public async Task SetHealth()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync("**!event set health <character alias> <health>** sets the character's health to the given value.");
                }
            }
        }

        [Command("feedback")]
        [Summary("Feedback help")]
        public async Task Feedback()
        {
            IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync("**!feedback <feedback text>** Let me know how I'm doing and what I can get better at!\nI'll do my very best to be the best BattleBot I can be. :smile:");
        }

        [Command("attack")]
        [Summary("Attack help")]
        public async Task Attack()
        {
            IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync("**!attack <character alias> <target-1 alias> ... <target-n alias>** the character will attempt to attack one or more targets according to the event ruleset. Yarrrrr! :crossed_swords:\nThis only works after the event has started.");
        }

        [Command("heal")]
        [Summary("Heal help")]
        public async Task Heal()
        {
            IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync("**!heal <character alias> <target-1 alias> ... <target-n alias>** the character will attempt to heal one or more targets according to the event ruleset. :hearts:\nThis only works after the event has started.");
        }

        [Command("ward")]
        [Summary("Ward help")]
        public async Task Ward()
        {
            IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync("**!ward <character alias> <target-1 alias> ... <target-n alias>** the character will attempt to ward one or more targets according to the event ruleset. :shield:\nThis only works after the event has started.");
        }
    }
}
