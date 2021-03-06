﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BattleBot
{
    [Group("guild")]
    public class GuildModule : ModuleBase<SocketCommandContext>
    {
        // Get guild status
        [Command("status")]
        [Summary("Prints the status of a guild.")]
        public async Task StatusGuild()
        {
            if (Context.Message.Channel is SocketGuildChannel channel)
            {
                string result = Data.GetGuild(channel.Guild).GetStatus();

                while (result.Length > 0)
                {
                    await ReplyAsync(Utilities.FixOverflow(result, out result));
                }
                return;
            }
            await ReplyAsync("Currently not in a guild channel.");
        }

        [Group("add")]
        public class AddModule : ModuleBase<SocketCommandContext>
        {
            [Command("adminrole")]
            [Summary("Adds an admin role.")]
            public async Task RemoveAdminrole(IRole role)
            {
                if (Context.Message.Channel is SocketGuildChannel channel)
                {
                    Guild guild = Data.GetGuild(channel.Guild);
                    User user = Data.GetUser(Context.Message.Author);
                    if (!guild.IsAdmin(user))
                    {
                        await ReplyAsync("You do not have admin access in this guild.");
                        return;
                    }

                    await ReplyAsync(Data.GetGuild(channel.Guild).AddAdminrole(role));
                    return;
                }
                await ReplyAsync("Currently not in a guild channel.");
            }
        }

        [Group("remove")]
        public class RemoveModule : ModuleBase<SocketCommandContext>
        {
            [Command("adminrole")]
            [Summary("Removes an admin role.")]
            public async Task RemoveAdminrole(IRole role)
            {
                if (Context.Message.Channel is SocketGuildChannel channel)
                {
                    Guild guild = Data.GetGuild(channel.Guild);
                    User user = Data.GetUser(Context.Message.Author);
                    if (!guild.IsAdmin(user))
                    {
                        await ReplyAsync("You do not have admin access in this guild.");
                        return;
                    }

                    await ReplyAsync(Data.GetGuild(channel.Guild).RemoveAdminrole(role));
                    return;
                }
                await ReplyAsync("Currently not in a guild channel.");
            }
        }
    }

    [Group("channel")]
    public class ChannelModule : ModuleBase<SocketCommandContext>
    {
        // Get channel status
        [Command("status")]
        [Summary("Prints the status of a channel.")]
        public async Task StatusChannel()
        {
            string result = Data.GetChannel(Context.Message.Channel as SocketChannel).GetStatus();

            while (result.Length > 0)
            {
                await ReplyAsync(Utilities.FixOverflow(result, out result));
            }
        }

        [Group("remove")]
        public class RemoveModule : ModuleBase<SocketCommandContext>
        {
            [Command("activeevent")]
            [Summary("Removes the active event.")]
            public async Task RemoveEvent()
            {
                Channel channel = Data.GetChannel(Context.Message.Channel as SocketChannel);
                User user = Data.GetUser(Context.Message.Author);
                if (!channel.IsAdmin(user))
                {
                    await ReplyAsync("You do not have admin access in this channel.");
                    return;
                }

                await ReplyAsync(channel.RemoveActive());
            }
        }
    }

    [Group("event")]
    public class EventModule : ModuleBase<SocketCommandContext>
    {
        // Create an event
        [Command("create")]
        [Summary("Creates a new RP event.")]
        public async Task CreateEvent([Remainder] string name)
        {
            User dm = Data.GetUser(Context.Message.Author);
            Channel channel = Data.GetChannel(Context.Message.Channel as SocketChannel);
            Event curEvent = new Event(dm, channel, name);

            await ReplyAsync(
                $"New event **{curEvent.Name} ({curEvent.Id})** created!\nType `!event join {curEvent.Id}` to join.");
        }


        // Start an event
        [Command("start")]
        [Summary("Starts an existing RP event.")]
        public async Task StartEvent(string eventId)
        {
            Event curEvent = Data.GetEvent(Context, eventId);
            if (curEvent == null)
            {
                await ReplyAsync($"Could not find any event in this channel with that ID.");
                return;
            }

            User curUser = Data.GetUser(Context.Message.Author);
            if (!curEvent.IsAdmin(curUser))
            {
                await ReplyAsync("You do not have permission to add an administrator to this event.");
                return;
            }

            await ReplyAsync(curEvent.Start(Data.GetUser(Context.Message.Author)));
        }

        // Event status
        [Command("status")]
        [Summary("Prints the status of an event.")]
        public async Task StatusEvent(string eventId = "Active")
        {
            Event curEvent = Data.GetEvent(Context, eventId);
            if (curEvent == null)
            {
                await ReplyAsync($"Could not find any event in this channel with that ID.");
                return;
            }

            string result = curEvent.Status();

            while (result.Length > 0)
            {
                await ReplyAsync(Utilities.FixOverflow(result, out result));
            }
        }

        // Event full status
        [Command("fullstatus")]
        [Summary("Prints the status of an event.")]
        public async Task FullStatusEvent(string eventId = "Active")
        {
            Event curEvent = Data.GetEvent(Context, eventId);
            if (curEvent == null)
            {
                await ReplyAsync($"Could not find any event in this channel with that ID.");
                return;
            }

            string result = curEvent.FullStatus();

            while (result.Length > 0)
            {
                await ReplyAsync(Utilities.FixOverflow(result, out result));
            }
        }

        // Start an event
        [Command("finish")]
        [Summary("Finishes an existing RP event.")]
        public async Task FinishEvent(string eventId)
        {
            Event curEvent = Data.GetEvent(Context, eventId);
            if (curEvent == null)
            {
                await ReplyAsync($"Could not find any event in this channel with that ID.");
                return;
            }

            User user = Data.GetUser(Context.Message.Author);
            if (!curEvent.IsAdmin(user))
            {
                await ReplyAsync("You do not have permission to end this event.");
                return;
            }

            string path = curEvent.Finish();
            await Context.Channel.SendFileAsync(curEvent.Finish(), "Event finished, here is the log.");
            File.Delete(path);
        }

        // Join an event
        [Command("join")]
        [Summary("Join an event without a character.")]
        public async Task JoinEvent(string eventId)
        {
            Event curEvent = Data.GetEvent(Context, eventId);
            if (curEvent == null)
            {
                await ReplyAsync("Could not find that event.");
                return;
            }

            await ReplyAsync(curEvent.Join(Data.GetUser(Context.Message.Author)));
        }

        // Join an event with a character
        [Command("join")]
        [Summary("Join an event with a character.")]
        public async Task JoinEvent(string eventId, string alias)
        {
            User user = Data.GetUser(Context.Message.Author);

            Event curEvent = Data.GetEvent(Context, eventId);
            if (curEvent == null)
            {
                await ReplyAsync("Could not find that event.");
                return;
            }

            Character character = user.GetCharacter(alias);
            if (character == null)
            {
                await ReplyAsync($"Could not find the character: " + alias);
                return;
            }

            await ReplyAsync(curEvent.Join(character, user));
        }

        // Add x to the event
        [Group("add")]
        [Summary("Add characters or users to the event.")]
        public class EventAddModule : ModuleBase<SocketCommandContext>
        {
            // Add an admin
            [Command("admin")]
            [Summary("Add an administrator to the event.")]
            public async Task AddAdmin(SocketUser user, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                User curUser = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(curUser))
                {
                    await ReplyAsync("You do not have permission to add an administrator to this event.");
                    return;
                }

                await ReplyAsync(curEvent.AddAdmin(Data.GetUser(user)));
            }

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

                User user = Data.GetUser(Context.Message.Author);
                Character character = user.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                if (!curEvent.Users.Contains(user.Id))
                {
                    await ReplyAsync(curEvent.Join(user));
                }

                await ReplyAsync(curEvent.AddCharacter(character, user));
            }

            // Add an alias
            [Command("alias")]
            [Summary("Add an alias to the character.")]
            public async Task AddAlias(string alias, string newAlias, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                User curUser = Data.GetUser(Context.Message.Author);
                if (!character.IsAdmin(curUser) && !curEvent.IsAdmin(curUser))
                {
                    await ReplyAsync("You do not have permission to add an administrator to this event.");
                    return;
                }

                foreach (Character eventCharacter in curEvent.Characters)
                {
                    if (eventCharacter.aliases.Contains(newAlias))
                    {
                        await ReplyAsync(eventCharacter.Name + " already has the alias " + newAlias + ".");
                        return;
                    }
                }

                character.aliases.Add(newAlias);

                await ReplyAsync(character.Name + " added the alias: " + newAlias + ".");
            }

            // Create a new character or NPC for this event
            [Group("new")]
            [Summary("Create a new character or NPC for this event.")]
            public class EventNewModule : ModuleBase<SocketCommandContext>
            {

                // Create a new character for this event
                [Command("character")]
                [Summary("Create a new character for this event.")]
                public async Task NewCharacter([Remainder] string name)
                {
                    Event curEvent = Data.GetEvent(Context);
                    if (curEvent == null)
                    {
                        await ReplyAsync("Could not find the event.");
                        return;
                    }

                    User user = Data.GetUser(Context.Message.Author);

                    Character character = new Character(name, 1, 1, user);

                    if (!curEvent.Users.Contains(user.Id))
                        await ReplyAsync(curEvent.Join(user));

                    await ReplyAsync(curEvent.AddCharacter(character, user));
                }

                // Create a new NPC for this event
                [Command("npc")]
                [Summary("Create a new NPC for this event.")]
                public async Task NewNpc([Remainder] string name)
                {
                    Event curEvent = Data.GetEvent(Context);
                    if (curEvent == null)
                    {
                        await ReplyAsync("Could not find the event.");
                        return;
                    }

                    User curUser = Data.GetUser(Context.Message.Author);
                    if (!curEvent.IsAdmin(curUser))
                    {
                        await ReplyAsync("You do not have permission to add an administrator to this event.");
                        return;
                    }

                    Character character = new Character(name, 1, 1, curUser)
                    {
                        Npc = true
                    };
                    await ReplyAsync(curEvent.AddCharacter(character, curUser));
                }

                // Create a new team for this event
                [Command("team")]
                [Summary("Create a new team for this event.")]
                public async Task NewTeam([Remainder] string name)
                {
                    Event curEvent = Data.GetEvent(Context);
                    if (curEvent == null)
                    {
                        await ReplyAsync("Could not find the event.");
                        return;
                    }

                    User curUser = Data.GetUser(Context.Message.Author);
                    if (!curEvent.IsAdmin(curUser))
                    {
                        await ReplyAsync("You do not have permission to add an administrator to this event.");
                        return;
                    }

                    await ReplyAsync(curEvent.AddTeam(name));
                }
            }
        }

        // Remove x from an event
        [Group("remove")]
        [Summary("Remove characters, teams and more from the event.")]
        public class EventRemoveModule : ModuleBase<SocketCommandContext>
        {
            // Remove an admin
            [Command("admin")]
            [Summary("Remove an administrator from the event.")]
            public async Task RemoveAdmin(SocketUser user, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                User curUser = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(curUser))
                {
                    await ReplyAsync("You do not have permission to remove an administrator from this event.");
                    return;
                }

                await ReplyAsync(curEvent.RemoveAdmin(Data.GetUser(user)));
            }

            // Remove a character from this event
            [Command("character")]
            [Alias("npc")]
            [Summary("Remove a character from the event.")]
            public async Task RemoveCharacter(string alias)
            {
                Event curEvent = Data.GetEvent(Context);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                User curUser = Data.GetUser(Context.Message.Author);
                if (!character.IsAdmin(curUser) && !curEvent.IsAdmin(curUser))
                {
                    await ReplyAsync("You do not have permission to remove a character from this event.");
                    return;
                }

                await ReplyAsync(curEvent.RemoveCharacter(character));
            }

            // Remove a team from this event
            [Command("team")]
            [Summary("Remove a team from this event.")]
            public async Task NewTeam(string alias)
            {
                Event curEvent = Data.GetEvent(Context);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                User curUser = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(curUser))
                {
                    await ReplyAsync("You do not have permission to add an administrator to this event.");
                    return;
                }

                Team team = curEvent.GetTeam(alias);
                if (team == null)
                {
                    await ReplyAsync("Could not find that character.");
                    return;
                }

                await ReplyAsync(curEvent.RemoveTeam(team));
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
            public async Task SetRuleset(string rulesetId, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);

                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to set the ruleset for this event.");
                    return;
                }


                switch (rulesetId.ToLower())
                {
                    case "amentia":
                    case "amentia's":
                        curEvent.Ruleset = new AmentiaRuleset();
                        await ReplyAsync(
                            $"Ruleset for the event {curEvent.Name} has changed to {curEvent.Ruleset.Name}.");
                        break;
                    case "amentia2":
                        curEvent.Ruleset = new Amentia2Ruleset();
                        await ReplyAsync(
                            $"Ruleset for the event {curEvent.Name} has changed to {curEvent.Ruleset.Name}");
                        break;
                    default:
                        await ReplyAsync("Unknown ruleset.");
                        break;
                }
            }

            // Set npc
            [Command("npc")]
            [Summary("Changes whether a character has ranged attacks or not.")]
            public async Task SetNpc(string alias, bool value, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to change this for this event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                character.Npc = value;

                if (value)
                {
                    await ReplyAsync(character.Name + " is now an NPC.");
                }
                else
                {
                    await ReplyAsync(character.Name + " is not a character.");
                }
            }

            // Set melee
            [Command("melee")]
            [Summary("Changes whether a character has melee attacks or not.")]
            public async Task Melee(string alias, bool value, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                User curUser = Data.GetUser(Context.Message.Author);
                if (!character.IsAdmin(curUser) && !curEvent.IsAdmin(curUser))
                {
                    await ReplyAsync("You do not have permission to change the meele ability of this character.");
                    return;
                }

                character.Melee = value;

                if (value)
                {
                    await ReplyAsync(character.Name + " now has melee attacks.");
                }
                else
                {
                    await ReplyAsync(character.Name + " no longer has melee attacks.");
                }
            }

            // Set ranged
            [Command("ranged")]
            [Summary("Changes whether a character has ranged attacks or not.")]
            public async Task Ranged(string alias, bool value, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                User curUser = Data.GetUser(Context.Message.Author);
                if (!character.IsAdmin(curUser) && !curEvent.IsAdmin(curUser))
                {
                    await ReplyAsync("You do not have permission to change the ranged ability of this character.");
                    return;
                }

                character.Ranged = value;

                if (value)
                {
                    await ReplyAsync(character.Name + " now has ranged attacks.");
                }
                else
                {
                    await ReplyAsync(character.Name + " no longer has ranged attacks.");
                }
            }

            // Set team
            [Command("team")]
            [Summary("Adds the character to a team.")]
            public async Task Team(string teamAlias, [Remainder] string charAlias)
            {
                Event curEvent = Data.GetEvent(Context);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                string[] charAliasList = Utilities.SplitList(charAlias);
                Character[] characters = new Character[charAliasList.Length];
                User curUser = Data.GetUser(Context.Message.Author);
                for (int i = 0; i < charAliasList.Length; i++)
                {
                    characters[i] = curEvent.GetCharacter(charAliasList[i]);
                    if (characters[i] == null)
                    {
                        await ReplyAsync("Could not find a character by the alias: " + charAliasList[i]);
                        return;
                    }

                    if (!characters[i].IsAdmin(curUser) && !curEvent.IsAdmin(curUser))
                    {
                        await ReplyAsync("You do not have permission to set the team of " + characters[i].Name + ".");
                        return;
                    }
                }

                Team team = curEvent.GetTeam(teamAlias);
                if (team == null)
                {
                    await ReplyAsync("Could not find a team by the alias: " + teamAlias);
                    return;
                }

                string result = "Changing teams:";

                foreach (Character character in characters)
                {
                    result += "\n" + curEvent.SetTeam(character, team);
                }

                await ReplyAsync(result);
            }

            // Set turn
            [Command("turn")]
            [Summary("Gives the turn to a specific character.")]
            public async Task SetTurn(string alias, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to set the ruleset for this event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                await ReplyAsync(curEvent.SetTurn(character));
            }

            // Set fixed order
            [Command("fixedorder")]
            [Summary("Enable fixed order, characters can only play on their turn.")]
            public async Task SetFixedOrder(bool value, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to set the ruleset for this event.");
                    return;
                }

                curEvent.FixedOrder = value;

                if (value)
                {
                    await ReplyAsync("Fixed order is now enabled.");
                    if (curEvent.Round > 0)
                    {
                        if (curEvent.Teams.Count > 1) curEvent.CurrentTeam = 0;
                        await ReplyAsync(curEvent.NextTurn());
                    }
                }
                else
                {
                    await ReplyAsync("Fixed order is now disabled.");
                }
            }

            // Set max health
            [Command("maxhealth")]
            [Summary("Sets the max health for the given character during this event.")]
            public async Task MaxHealth(string alias, int health, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to set the ruleset for this event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                await ReplyAsync(curEvent.SetMaxhealth(character, health));
            }

            // Set health
            [Command("health")]
            [Summary("Sets the health for the given character during this event.")]
            public async Task Health(string alias, int health, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to set the ruleset for this event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                await ReplyAsync(curEvent.SetHealth(character, health));
            }

            // Set health
            [Command("ward")]
            [Summary("Gives the character a ward.")]
            public async Task Health(string alias, int health, int duration, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to set the ruleset for this event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                await ReplyAsync(curEvent.SetWard(character, health, duration));
            }

            // Set action points
            [Command("ap")]
            [Summary("Sets the action points for the given character during this event.")]
            public async Task ActionPoints(string alias, int health, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to set the ruleset for this event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                await ReplyAsync(curEvent.SetAp(character, health));
            }

            // Set Damage
            [Command("basedamage")]
            [Summary("Sets the base damage during this event.")]
            public async Task BaseDamage(int damage, string eventId = "Active")
            {
                User user = Data.GetUser(Context.Message.Author);
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to set the ruleset for this event.");
                    return;
                }

                curEvent.Ruleset.BaseDamage = damage;

                await ReplyAsync("The base damage is now: " + damage);
            }

            // Set Heal
            [Command("baseheal")]
            [Summary("Sets the base heal during this event.")]
            public async Task BaseHeal(int health, string eventId = "Active")
            {
                User user = Data.GetUser(Context.Message.Author);
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to set the ruleset for this event.");
                    return;
                }

                curEvent.Ruleset.BaseHeal = health;

                await ReplyAsync("The base heal is now: " + health);
            }

            // Set event details
            [Group("potion")]
            [Summary("All commands for setting specific properties of the event.")]
            public class EventEnablePotionModule : ModuleBase<SocketCommandContext>
            {
                // melee attacks
                [Command("health")]
                [Summary("Health potions")]
                public async Task EnableHealthPotion(string alias, int amount, string eventId = "Active")
                {
                    Event curEvent = Data.GetEvent(Context, eventId);
                    if (curEvent == null)
                    {
                        await ReplyAsync("Could not find the event.");
                        return;
                    }

                    User user = Data.GetUser(Context.Message.Author);
                    if (!curEvent.IsAdmin(user))
                    {
                        await ReplyAsync("You do not have permission to set the ruleset for this event.");
                        return;
                    }

                    Character character = curEvent.GetCharacter(alias);
                    if (character == null)
                    {
                        await ReplyAsync($"Could not find the character: " + alias);
                        return;
                    }

                    character.HealthPotions = amount;

                    await ReplyAsync(character.Name + " now has " + amount + " health potions.");
                }
            }

            // Set event details
            [Group("order")]
            [Summary("All commands for setting the .")]
            public class EventSetOrderModule : ModuleBase<SocketCommandContext>
            {

                // Changing the order of the teams
                [Command("teams")]
                [Summary("Sets the order of the teams for the event, can only be done before the event starts.")]
                public async Task SetOrderTeams([Remainder] string teamAliases = "Empty")
                {
                    Event curEvent = Data.GetEvent(Context);

                    if (curEvent == null)
                    {
                        await ReplyAsync("Could not find the event.");
                        return;
                    }

                    User user = Data.GetUser(Context.Message.Author);
                    if (!curEvent.IsAdmin(user))
                    {
                        await ReplyAsync("You do not have permission to set the ruleset for this event.");
                        return;
                    }

                    if (teamAliases.Equals("Empty"))
                    {
                        await ReplyAsync("Teams:\n" + curEvent.ListTeamsAlias());
                        return;
                    }

                    string[] teamList = Utilities.SplitList(teamAliases);

                    if (teamList.Length != curEvent.Teams.Count)
                    {
                        await ReplyAsync("You need to set the order of all the teams:\n" + curEvent.ListTeamsAlias());
                        return;
                    }

                    foreach (string teamAlias in teamList)
                    {
                        if (curEvent.GetTeam(teamAlias) == null)
                        {
                            await ReplyAsync("Could not find the team: " + teamAlias);
                            return;
                        }
                    }

                    await ReplyAsync(curEvent.ReorderTeams(teamList));
                }
            }
        }

        // Set event details
        [Group("disable")]
        [Summary("All commands for disabling specific properties of the event.")]
        public class EventDisableModule : ModuleBase<SocketCommandContext>
        {
            // Set event details
            [Group("potion")]
            [Summary("All commands for setting specific properties of the event.")]
            public class EventDisablePotionModule : ModuleBase<SocketCommandContext>
            {
                // melee attacks
                [Command("health")]
                [Summary("Health potions")]
                public async Task DisableHealthPotion(string eventId = "Active")
                {
                    Event curEvent = Data.GetEvent(Context, eventId);
                    if (curEvent == null)
                    {
                        await ReplyAsync("Could not find an active event.");
                        return;
                    }

                    User user = Data.GetUser(Context.Message.Author);
                    if (!curEvent.IsAdmin(user))
                    {
                        await ReplyAsync("You do not have permission to set the ruleset for this event.");
                        return;
                    }

                    curEvent.Ruleset.PotionHealthRollMax = 0;

                    foreach (Character character in curEvent.Characters)
                    {
                        character.HealthPotions = 0;
                    }

                    await ReplyAsync("Health potions are now disabled.");
                }
            }
        }

        // Set event details
        [Group("enable")]
        [Summary("All commands for enabling specific properties of the event.")]
        public class EventEnableModule : ModuleBase<SocketCommandContext>
        {
            // Set event details
            [Group("potion")]
            [Summary("All commands for setting specific properties of the event.")]
            public class EventEnablePotionModule : ModuleBase<SocketCommandContext>
            {
                // melee attacks
                [Command("health")]
                [Summary("Health potions")]
                public async Task EnableHealthPotion(int min, int max, string eventId = "Active")
                {
                    Event curEvent = Data.GetEvent(Context, eventId);
                    if (curEvent == null)
                    {
                        await ReplyAsync("Could not find an active event.");
                        return;
                    }

                    User user = Data.GetUser(Context.Message.Author);
                    if (!curEvent.IsAdmin(user))
                    {
                        await ReplyAsync("You do not have permission to set the ruleset for this event.");
                        return;
                    }

                    if (max < min)
                    {
                        await ReplyAsync("The maximum cannot be less than the minimum.");
                        return;
                    }

                    curEvent.Ruleset.PotionHealthRollMin = min;
                    curEvent.Ruleset.PotionHealthRollMax = max;

                    Random rnd = new Random();
                    string result = "Health potions are now enabled.";

                    foreach (Character character in curEvent.Characters)
                    {
                        character.HealthPotions = rnd.Next(min, max + 1);
                        result += $"\n{character.Name} got {character.HealthPotions} health potions.";
                    }

                    await ReplyAsync(result);
                }
            }
        }

        // Hide teams or characters
        [Group("hide")]
        [Summary("Commands for hiding teams and characters.")]
        public class EventHideModule : ModuleBase<SocketCommandContext>
        {
            // hide team
            [Command("team")]
            [Summary("Hide team")]
            public async Task HideTeam(string alias, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find an active event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission hide a team for this event.");
                    return;
                }

                Team team = curEvent.GetTeam(alias);
                if (team == null)
                {
                    await ReplyAsync("Could not find a team by the alias: " + alias);
                    return;
                }

                team.Hide();

                foreach (Character teamMember in team.members)
                {
                    teamMember.Hidden = true;
                }

                await ReplyAsync(team.Name + " is now hidden.");
            }

            // hide character
            [Command("character")]
            [Summary("Hide character")]
            public async Task HideCharacter(string alias, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find an active event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to hide a character for this event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                character.Hidden = true;

                await ReplyAsync(character.Name + " is now hidden.");
            }
        }

        // Reveal teams or characters
        [Group("reveal")]
        [Summary("All commands for setting specific properties of the event.")]
        public class EventRevealModule : ModuleBase<SocketCommandContext>
        {
            // Reveal team
            [Command("team")]
            [Summary("Reveal team")]
            public async Task RevealTeam(string alias, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find an active event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to reveal a team for this event.");
                    return;
                }

                Team team = curEvent.GetTeam(alias);
                if (team == null)
                {
                    await ReplyAsync("Could not find a team by the alias: " + alias);
                    return;
                }

                team.Reveal();

                foreach (Character teamMember in team.members)
                {
                    teamMember.Hidden = false;
                }

                await ReplyAsync(team.Name + " was revealed!");
            }

            // Reveal character
            [Command("character")]
            [Summary("Reveal character")]
            public async Task RevealCharacter(string alias, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find an active event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                if (!curEvent.IsAdmin(user))
                {
                    await ReplyAsync("You do not have permission to reveal a character for this event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                character.Hidden = false;

                await ReplyAsync(character.Name + " was revealed!");
            }
        }
    }

    // Combat commands
    [Summary("All combat related commands.")]
    public class CombatModule : ModuleBase<SocketCommandContext>
    {
        // melee attacks
        [Command("attack")]
        [Summary("melee attacks")]
        public async Task Attack(string alias, [Remainder] string targetAlias)
        {
            Event curEvent = Data.GetChannel(Context.Channel as SocketChannel).ActiveEvent;
            if (curEvent == null)
            {
                await ReplyAsync("Could not find an active event.");
                return;
            }

            Character character = curEvent.GetCharacter(alias);
            if (character == null)
            {
                await ReplyAsync("Could not find the character " + alias);
                return;
            }

            User curUser = Data.GetUser(Context.Message.Author);
            if (!character.IsAdmin(curUser) && !curEvent.IsAdmin(curUser))
            {
                await ReplyAsync("You do not have permission to remove a character from this event.");
                return;
            }

            string[] targetAliases = Utilities.SplitList(targetAlias);
            Character[] targets = new Character[targetAliases.Length];

            for (int i = 0; i < targetAliases.Length; i++)
            {
                targets[i] = curEvent.GetCharacter(targetAliases[i]);
                if (targets[i] == null)
                {
                    await ReplyAsync($"Could not find the character: {targetAliases[i]}.");
                    return;
                }
            }

            if (!character.Melee)
            {
                await ReplyAsync(character.Name + " does not have a melee attack.");
                return;
            }

            string result = curEvent.Attack(character, targets);

            while (result.Length > 0)
            {
                await ReplyAsync(Utilities.FixOverflow(result, out result));
            }
        }

        // melee attacks
        [Command("ranged")]
        [Summary("Ranged attacks")]
        public async Task Ranged(string alias, [Remainder] string targetAlias)
        {
            Event curEvent = Data.GetChannel(Context.Channel as SocketChannel).ActiveEvent;
            if (curEvent == null)
            {
                await ReplyAsync("Could not find an active event.");
                return;
            }

            Character character = curEvent.GetCharacter(alias);
            if (character == null)
            {
                await ReplyAsync("Could not find the character " + alias);
                return;
            }

            User curUser = Data.GetUser(Context.Message.Author);
            if (!character.IsAdmin(curUser) && !curEvent.IsAdmin(curUser))
            {
                await ReplyAsync("You do not have permission to remove a character from this event.");
                return;
            }

            string[] targetAliases = Utilities.SplitList(targetAlias);
            Character[] targets = new Character[targetAliases.Length];

            for (int i = 0; i < targetAliases.Length; i++)
            {
                targets[i] = curEvent.GetCharacter(targetAliases[i]);
                if (targets[i] == null)
                {
                    await ReplyAsync($"Could not find the character: {targetAliases[i]}.");
                    return;
                }
            }

            if (!character.Ranged)
            {
                await ReplyAsync(character.Name + " does not have a ranged attack.");
                return;
            }

            string result = curEvent.Ranged(character, targets);

            while (result.Length > 0)
            {
                await ReplyAsync(Utilities.FixOverflow(result, out result));
            }
        }

        // Heals
        [Command("heal")]
        [Summary("Heals")]
        public async Task Heal(string alias, [Remainder] string targetAlias)
        {
            Event curEvent = Data.GetChannel(Context.Channel as SocketChannel).ActiveEvent;
            if (curEvent == null)
            {
                await ReplyAsync("Could not find an active event.");
                return;
            }

            Character character = curEvent.GetCharacter(alias);
            if (character == null)
            {
                await ReplyAsync("Could not find the character " + alias);
                return;
            }

            User curUser = Data.GetUser(Context.Message.Author);
            if (!character.IsAdmin(curUser) && !curEvent.IsAdmin(curUser))
            {
                await ReplyAsync("You do not have permission to remove a character from this event.");
                return;
            }

            string[] targetAliases = Utilities.SplitList(targetAlias);
            Character[] targets = new Character[targetAliases.Length];

            for (int i = 0; i < targetAliases.Length; i++)
            {
                targets[i] = curEvent.GetCharacter(targetAliases[i]);
                if (targets[i] == null)
                {
                    await ReplyAsync($"Could not find the character: {targetAliases[i]}.");
                    return;
                }
            }

            string result = curEvent.Heal(character, targets);

            while (result.Length > 0)
            {
                await ReplyAsync(Utilities.FixOverflow(result, out result));
            }
        }

        // Wards
        [Command("ward")]
        [Summary("Wards")]
        public async Task Ward(string alias, [Remainder] string targetAlias)
        {
            Event curEvent = Data.GetChannel(Context.Channel as SocketChannel).ActiveEvent;
            if (curEvent == null)
            {
                await ReplyAsync("Could not find an active event.");
                return;
            }

            Character character = curEvent.GetCharacter(alias);
            if (character == null)
            {
                await ReplyAsync($"Could not find the character: " + alias);
                return;
            }

            User curUser = Data.GetUser(Context.Message.Author);
            if (!character.IsAdmin(curUser) && !curEvent.IsAdmin(curUser))
            {
                await ReplyAsync("You do not have permission to remove a character from this event.");
                return;
            }

            string[] targetAliases = Utilities.SplitList(targetAlias);
            Character[] targets = new Character[targetAliases.Length];

            for (int i = 0; i < targetAliases.Length; i++)
            {
                targets[i] = curEvent.GetCharacter(targetAliases[i]);
                if (targets[i] == null)
                {
                    await ReplyAsync($"Could not find the character: {targetAliases[i]}.");
                    return;
                }
            }

            string result = curEvent.Ward(character, targets);

            while (result.Length > 0)
            {
                await ReplyAsync(Utilities.FixOverflow(result, out result));
            }
        }

        // Blocks
        [Command("block")]
        [Summary("Shield blocks")]
        public async Task Block(string alias)
        {
            Event curEvent = Data.GetChannel(Context.Channel as SocketChannel).ActiveEvent;
            if (curEvent == null)
            {
                await ReplyAsync("Could not find an active event.");
                return;
            }

            Character character = curEvent.GetCharacter(alias);
            if (character == null)
            {
                await ReplyAsync($"Could not find the character: " + alias);
                return;
            }

            User curUser = Data.GetUser(Context.Message.Author);
            if (!character.IsAdmin(curUser) && !curEvent.IsAdmin(curUser))
            {
                await ReplyAsync("You do not have permission to remove a character from this event.");
                return;
            }

            string result = curEvent.Block(character);

            while (result.Length > 0)
            {
                await ReplyAsync(Utilities.FixOverflow(result, out result));
            }
        }

        // Potions
        [Group("potion")]
        [Summary("All potion related commands.")]
        public class PotionModule : ModuleBase<SocketCommandContext>
        {
            // Potions
            [Command("health")]
            [Summary("Health potions")]
            public async Task Health(string alias)
            {
                Event curEvent = Data.GetChannel(Context.Channel as SocketChannel).ActiveEvent;
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find an active event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                User curUser = Data.GetUser(Context.Message.Author);
                if (!character.IsAdmin(curUser) && !curEvent.IsAdmin(curUser))
                {
                    await ReplyAsync("You do not have permission to remove a character from this event.");
                    return;
                }

                string result = curEvent.PotionHealth(character);

                while (result.Length > 0)
                {
                    await ReplyAsync(Utilities.FixOverflow(result, out result));
                }
            }
        }

        // Pass
        [Command("pass")]
        [Summary("Pass round")]
        public async Task Pass(string alias)
        {
            Event curEvent = Data.GetChannel(Context.Channel as SocketChannel).ActiveEvent;
            if (curEvent == null)
            {
                await ReplyAsync("Could not find an active event.");
                return;
            }

            Character character = curEvent.GetCharacter(alias);
            if (character == null)
            {
                await ReplyAsync($"Could not find the character: " + alias);
                return;
            }

            User curUser = Data.GetUser(Context.Message.Author);
            if (!character.IsAdmin(curUser) && !curEvent.IsAdmin(curUser))
            {
                await ReplyAsync("You do not have permission to remove a character from this event.");
                return;
            }

            string result = curEvent.Pass(character);

            while (result.Length > 0)
            {
                await ReplyAsync(Utilities.FixOverflow(result, out result));
            }
        }
    }

    [Group("character")]
    [Summary("All global character related commands.")]
    public class CharacterModule : ModuleBase<SocketCommandContext>
    {

    }

    [Group("user")]
    [Summary("All global user related commands.")]
    public class UserModule : ModuleBase<SocketCommandContext>
    {
        [Group("set")]
        [Summary("All global character related commands.")]
        public class SetModule : ModuleBase<SocketCommandContext>
        {
            // active event
            [Command("activeevent")]
            [Summary("Set Active event")]
            public async Task Pass(string eventId)
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find an active event.");
                    return;
                }

                User user = Data.GetUser(Context.Message.Author);
                user.SetActiveEvent(Context.Channel.Id, curEvent);

                if (curEvent.IsAdmin(user))
                {
                    user.CurrentEvent = curEvent;
                }

                await ReplyAsync(user.Username + "'s active event for this channel is now: " + curEvent.Name);
            }
        }
    }

    [Summary("General stuff.")]
    public class EtcModule : ModuleBase<SocketCommandContext>
    {
        // Hugs
        [Command("hug")]
        [Summary("Hugs.")]
        public async Task Hug()
        {
            await ReplyAsync($"-hugs {Context.Message.Author.Mention}- " + Emotes.Heart);
        }

        // Plays dead
        [Command("playdead")]
        [Summary("Plays dead.")]
        public async Task PlayDead()
        {
            await ReplyAsync(Emotes.Dead);
        }

        // Feedback
        [Command("feedback")]
        [Summary("Send feedback.")]
        public async Task Feedback([Remainder] string feedback)
        {
            string author = Context.Message.Author.Mention;
            await ReplyAsync($"Thank you very much for your feedback, {author}! " + Emotes.Heart);
            SocketUser log = Context.Client.GetUser(174426714120781824);
            await (log.SendMessageAsync($"From: {author}\n" + feedback));
        }

        // Roll dice
        [Command("roll")]
        [Summary("Roll a dice.")]
        public async Task Roll()
        {
            string author = Context.Message.Author.Mention;
            await ReplyAsync(Dice.Roll(author, "d6"));
        }

        // Roll dice
        [Command("roll")]
        [Summary("Roll a dice.")]
        public async Task Roll([Remainder] string cmd)
        {
            string author = Context.Message.Author.Mention;
            await ReplyAsync(Dice.Roll(author, cmd));
        }

        // Flip coin
        [Command("flipcoin")]
        [Alias("coinflip")]
        [Summary("Flip a coin.")]
        public async Task Flip()
        {
            string author = Context.Message.Author.Mention;
            await ReplyAsync(Dice.Flip(author));
        }

        // Immediate shutdown
        [Command("shutdown")]
        [Summary("Stops the bot immediately.")]
        public async Task Shutdown()
        {
            if (Context.Message.Author.Id != 174426714120781824)
            {
                await ReplyAsync("You are not permitted to use this command. " + Emotes.Angry);
                return;
            }
            await ReplyAsync("Shutting down. " + Emotes.Dead);
            Data.Exit();
        }

        // Delayed shutdown
        [Command("update")]
        [Summary("Stops the bot after 15 minutes.")]
        public async Task Update()
        {
            if (Context.Message.Author.Id != 174426714120781824)
            {
                await ReplyAsync("You are not permitted to use this command. " + Emotes.Angry);
                return;
            }
            await ReplyAsync("Starting shutdown sequence. " + Emotes.Sad);
            new Thread(Data.Update).Start();
        }
    }
}
