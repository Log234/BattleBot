using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BattleBot
{
    [Group("channel")]
    public class ChannelModule : ModuleBase<SocketCommandContext>
    {
        // Get channel status
        [Command("status")]
        [Summary("Prints the status of a channel.")]
        public async Task StatusChannel()
        {
            await ReplyAsync(Data.GetChannel(Context.Message.Channel as SocketChannel).GetStatus());
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

            await ReplyAsync(curEvent.Status());
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
                    await ReplyAsync("Could not find a character with that alias.");
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
                    await ReplyAsync($"Could not find that character.");
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
                    User user = Data.GetUser(Context.Message.Author);

                    Character character = new Character(name, 1, 1, user)
                    {
                        Npc = true
                    };
                    await ReplyAsync(curEvent.AddCharacter(character, user));
                }

                // Create a new team for this event
                [Command("team")]
                [Summary("Create a new team for this event.")]
                public async Task NewTeam([Remainder] string name)
                {
                    Event curEvent = Data.GetEvent(Context);

                    await ReplyAsync(curEvent.AddTeam(name));
                }
            }
        }

        // Remove x from an event
        [Group("remove")]
        [Summary("Remove characters, teams and more from the event.")]
        public class EventRemoveModule : ModuleBase<SocketCommandContext>
        {
            // Add an admin
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
                    await ReplyAsync("You do not have permission to add an administrator to this event.");
                    return;
                }

                await ReplyAsync(curEvent.RemoveAdmin(Data.GetUser(user)));
            }

            // Create a new character for this event
            [Command("character")]
            [Alias("npc")]
            [Summary("Remove a character from the event.")]
            public async Task RemoveCharacter(string alias)
            {
                Event curEvent = Data.GetEvent(Context);
                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync($"Could not find that character.");
                    return;
                }

                await ReplyAsync(curEvent.RemoveCharacter(character));
            }

            // Create a new team for this event
            [Command("team")]
            [Summary("Create a new team for this event.")]
            public async Task NewTeam(string alias)
            {
                Event curEvent = Data.GetEvent(Context);
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
            public async Task SetNPC(string alias, bool value, string eventId = "Active")
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
                    await ReplyAsync("Could not find that character.");
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

            // Set Meele
            [Command("meele")]
            [Summary("Changes whether a character has meele attacks or not.")]
            public async Task Meele(string alias, bool value, string eventId = "Active")
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
                    await ReplyAsync("Could not find that character.");
                    return;
                }

                character.Meele = value;

                if (value)
                {
                    await ReplyAsync(character.Name + " now has meele attacks.");
                }
                else
                {
                    await ReplyAsync(character.Name + " no longer has meele attacks.");
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
                    await ReplyAsync("Could not find that character.");
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
            public async Task Team(string charAlias, string teamAlias, string eventId = "Active")
            {
                Event curEvent = Data.GetEvent(Context, eventId);
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find the event.");
                    return;
                }

                Character character = curEvent.GetCharacter(charAlias);
                if (character == null)
                {
                    await ReplyAsync("Could not find that character.");
                    return;
                }

                Team team = curEvent.GetTeam(teamAlias);

                await ReplyAsync(curEvent.SetTeam(character, team));
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

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync("Could not find that character.");
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

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync("Could not find that character.");
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

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync("Could not find that character.");
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

                Character character = curEvent.GetCharacter(alias);
                if (character == null)
                {
                    await ReplyAsync("Could not find that character.");
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
                // Meele attacks
                [Command("health")]
                [Summary("Health potions")]
                public async Task DisableHealthPotion()
                {
                    Event curEvent = Data.GetChannel(Context.Channel as SocketChannel).ActiveEvent;
                    if (curEvent == null)
                    {
                        await ReplyAsync("Could not find an active event.");
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
                // Meele attacks
                [Command("health")]
                [Summary("Health potions")]
                public async Task EnableHealthPotion(int min, int max)
                {
                    Event curEvent = Data.GetChannel(Context.Channel as SocketChannel).ActiveEvent;
                    if (curEvent == null)
                    {
                        await ReplyAsync("Could not find an active event.");
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
                        character.HealthPotions = rnd.Next(min, max);
                        result += $"\n{character.Name} got {character.HealthPotions} health potions.";
                    }

                    await ReplyAsync(result);
                }
            }

        }

        // Combat commands
        [Summary("All combat related commands.")]
        public class CombatModule : ModuleBase<SocketCommandContext>
        {
            // Meele attacks
            [Command("attack")]
            [Summary("Meele attacks")]
            public async Task Attack(string alias, [Remainder] string targetAlias)
            {
                Event curEvent = Data.GetChannel(Context.Channel as SocketChannel).ActiveEvent;
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find an active event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias.ToLower());

                string[] targetAliases = targetAlias.Split(' ');
                Character[] targets = new Character[targetAliases.Length];

                for (int i = 0; i < targetAliases.Length; i++)
                {
                    targets[i] = curEvent.GetCharacter(targetAliases[i]);
                    if (targets[i] == null)
                    {
                        await ReplyAsync($"Could not find the character: {targets[i]}.");
                        return;
                    }
                }

                await ReplyAsync(curEvent.Attack(character, targets));
            }

            // Meele attacks
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

                Character character = curEvent.GetCharacter(alias.ToLower());

                string[] targetAliases = targetAlias.Split(' ');
                Character[] targets = new Character[targetAliases.Length];

                for (int i = 0; i < targetAliases.Length; i++)
                {
                    targets[i] = curEvent.GetCharacter(targetAliases[i]);
                    if (targets[i] == null)
                    {
                        await ReplyAsync($"Could not find the character: {targets[i]}.");
                        return;
                    }
                }

                if (!character.Ranged)
                {
                    await ReplyAsync(character.Name + " does not have a ranged attack.");
                    return;
                }

                await ReplyAsync(curEvent.Ranged(character, targets));
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

                Character character = curEvent.GetCharacter(alias.ToLower());

                string[] targetAliases = targetAlias.Split(' ');
                Character[] targets = new Character[targetAliases.Length];

                for (int i = 0; i < targetAliases.Length; i++)
                {
                    targets[i] = curEvent.GetCharacter(targetAliases[i]);
                    if (targets[i] == null)
                    {
                        await ReplyAsync($"Could not find the character: {targets[i]}.");
                        return;
                    }
                }

                await ReplyAsync(curEvent.Heal(character, targets));
            }

            // Wards
            [Command("ward")]
            [Summary("Meele attacks")]
            public async Task Ward(string alias, [Remainder] string targetAlias)
            {
                Event curEvent = Data.GetChannel(Context.Channel as SocketChannel).ActiveEvent;
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find an active event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias.ToLower());
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                string[] targetAliases = targetAlias.Split(' ');
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

                await ReplyAsync(curEvent.Ward(character, targets));
            }

            // Blocks
            [Command("block")]
            [Summary("Meele attacks")]
            public async Task Block(string alias)
            {
                Event curEvent = Data.GetChannel(Context.Channel as SocketChannel).ActiveEvent;
                if (curEvent == null)
                {
                    await ReplyAsync("Could not find an active event.");
                    return;
                }

                Character character = curEvent.GetCharacter(alias.ToLower());
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                await ReplyAsync(curEvent.Block(character));
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

                    Character character = curEvent.GetCharacter(alias.ToLower());
                    if (character == null)
                    {
                        await ReplyAsync($"Could not find the character: " + alias);
                        return;
                    }

                    await ReplyAsync(curEvent.PotionHealth(character));
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

                Character character = curEvent.GetCharacter(alias.ToLower());
                if (character == null)
                {
                    await ReplyAsync($"Could not find the character: " + alias);
                    return;
                }

                await ReplyAsync(curEvent.Pass(character));
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
