using System.Threading.Tasks;
using Discord;
using Discord.Commands;

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
            await channel.SendMessageAsync(
                "Type `!help <command>` to learn more about the different commands:\nfeedback\nroll\nflipcoin\nchannel\nevent\nattack\nheal\nward\nblock\npotion");
        }

        // Guild help
        [Group("guild")]
        public class GuildModule : ModuleBase<SocketCommandContext>
        {
            [Command]
            [Summary("Guild help")]
            public async Task GuildHelp()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(
                    "`!guild <command>` allows you to moderate a guild with these commands:\nguild status\nguild add <command>\nguild remove <command>");
            }

            [Command("status")]
            [Summary("Prints the status of a guild.")]
            public async Task StatusGuild()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(
                    "`!guild status` prints out information about the current guild, such as admins, admin roles, channels and events.");
            }

            [Group("add")]
            public class AddModule : ModuleBase<SocketCommandContext>
            {
                [Command]
                [Summary("Guild add help")]
                public async Task GuildHelp()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!guild add <command>` allows you to add things such as admins and admin roles to a guild with these commands:\nguild add adminrole");
                }

                [Command("adminrole")]
                [Summary("Adds an admin role.")]
                public async Task AddAdminrole()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!guild add adminrole <role>` allows you to add roles from your guild that will be treated as admins of BattleBot in this guild.");
                }
            }

            [Group("remove")]
            public class RemoveModule : ModuleBase<SocketCommandContext>
            {
                [Command]
                [Summary("Guild remove help")]
                public async Task GuildHelp()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!guild remove <command>` allows you to remove things such as admins and admin roles from a guild with these commands:\nguild remove adminrole");
                }

                [Command("adminrole")]
                [Summary("Removes an admin role.")]
                public async Task RemoveAdminrole()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!guild add adminrole <role>` allows you to remove roles that was treated as admins of BattleBot in this guild.");
                }
            }
        }

        // Channel
        [Group("channel")]
        public class ChannelModule : ModuleBase<SocketCommandContext>
        {
            [Command]
            [Summary("Channel help")]
            public async Task ChannelHelp()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(
                    "`!channel <command>` allows you to moderate channels with these commands:\nchannel status\nchannel remove");
            }

            [Command("status")]
            [Summary("Channel status help")]
            public async Task Status()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(
                    "`!channel status [event ID]` I'll read out some information about the channel, such as the name, if there is an ongoing event, and what events are in it. :books:");
            }

            [Group("remove")]
            public class RemoveModule : ModuleBase<SocketCommandContext>
            {
                [Command]
                [Summary("Channel remove help")]
                public async Task ChannelRemoveHelp()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!channel remove <command>` allows you to remove things from a channel:\nchannel remove activeevent");
                }

                [Command("activeevent")]
                [Summary("Removes the active event.")]
                public async Task RemoveActiveEvent()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!channel remove activeevent` Removes the currently active event from a channel.");
                }
            }
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
                await channel.SendMessageAsync(
                    "`!event <command>` allows you to create, join or alter events with these commands:\nevent create\nevent start\nevent finish\nevent join\nevent add <command>\nevent remove <command>\nevent set <command>\nevent disable <command>\nevent enable <command>\nevent hide <command>\nevent reveal <command>");
            }

            [Command("create")]
            [Summary("Event creation help")]
            public async Task Create()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(
                    "`!event create <name>` creates a new event and sets it active for you, so that following commands will be directed at this event.");
            }

            [Command("start")]
            [Summary("Event start help")]
            public async Task Start()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(
                    "`!event start <event ID>` starts an event and sets it active for you.\nAfter you have added all the characters and prepared the event, use this command to start the combat.");
            }

            [Command("status")]
            [Summary("Event status help")]
            public async Task Status()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(
                    "`!event status [event ID]` I'll list all characters and how they are doing. :books:");
            }

            [Command("fullstatus")]
            [Summary("Event status help")]
            public async Task FullStatus()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(
                    "`!event fullstatus [event ID]` I'll read out some information about the event, such as the name, whether it has started and who have joined it. :books:");
            }


            [Command("finish")]
            [Summary("Event finish help")]
            public async Task Finish()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(
                    "`!event finish <event ID>` ends an event and posts the resulting event log to the chat. Use this command when the event is finished and you no longer need it.");
            }

            [Command("join")]
            [Summary("Event join help")]
            public async Task Join()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(
                    "`!event join <event ID>` allows you to join an event and sets it active for you, so that following commands wil be directed at this event.");
            }

            [Group("add")]
            public class EventAddModule : ModuleBase<SocketCommandContext>
            {
                [Command]
                [Summary("Event addition help")]
                public async Task Add()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event add <command>` allows you to add characters, NPCs, administrators and teams to an event with these commands:\nevent add admin\nevent add character\nevent add alias\nevent add new <command>");
                }

                [Command("admin")]
                [Summary("Event add admin help")]
                public async Task AddAdmin()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event add admin <User tag>` allows you to add an admin to your event, who will have access to administrative commands such as `!event start <event ID>` and `!event set ruleset <ruleset ID>`.");
                }

                [Command("character")]
                [Summary("Event add character help")]
                public async Task AddCharacter()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event add character <character alias>` allows you to add one of your existing characters to this event.");
                }

                [Command("alias")]
                [Summary("Add an alias to the character.")]
                public async Task AddAlias()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event add character <character alias> <new alias>` allows you to add an alias to a character in this event.");
                }

                [Group("new")]
                public class EventAddNewModule : ModuleBase<SocketCommandContext>
                {
                    [Command]
                    [Summary("Event add new help")]
                    public async Task New()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync(
                            "`!event add new <command>` allows you create new characters, NPCs and teams for this event and they will be added automatically:\nevent add new character\nevent add new npc\nevent add new team");
                    }

                    [Command("character")]
                    [Summary("Event add new character help")]
                    public async Task NewCharacter()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync(
                            "`!event add new character <event ID> <name>` allows you to create a new character and add it to this event.\nThe character alias will by default be the first name of the character.");
                    }

                    [Command("npc")]
                    [Summary("Event add new npc help")]
                    public async Task NewNpc()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync(
                            "`!event add new npc <event ID> <name>` allows you to create a new NPC and add it to this event.\nThe event rounds does not wait for NPCs to spend their action points, when all normal characters have spent their points the round will end.");
                    }

                    [Command("team")]
                    [Summary("Event add new team help")]
                    public async Task NewTeam()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync(
                            "`!event add new team <event ID> <team name>` allows you to create a new team and add it to this event.\nCharacters and NPCs can be added to teams with !event set team <character alias> <team alias>. When only one team is left standing they win.");
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
                    await channel.SendMessageAsync(
                        "`!event remove <command>` allows you remove characters, NPCs, teams and admins from this event:\nevent remove admin\nevent remove character\nevent remove npc\nevent remove team");
                }

                [Command("admin")]
                [Summary("Event remove admin help")]
                public async Task RemoveAdmin()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event remove admin <user tag>` removes the given admin from the event.");
                }

                [Command("character")]
                [Summary("Event remove character help")]
                public async Task RemoveCharacter()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event remove character <character alias>` removes the given character from the event.");
                }

                [Command("npc")]
                [Summary("Event remove npc help")]
                public async Task RemoveNpc()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event remove npc <character alias>` removes the given npc from the event.");
                }

                [Command("team")]
                [Summary("Event remove team help")]
                public async Task RemoveTeam()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event remove team <team alias>` removes the given team from the event.\nCharacters from the removed team will remain in the event, but not in any team until they are reassigned.");
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
                    await channel.SendMessageAsync(
                        "`!event set <command>` allows you change details for this event:\nevent set ruleset\nevent set npc\nevent set melee\nevent set ranged\nevent set team\nevent set turn\nevent set fixedorder\nevent set maxhealth\nevent set health\nevent set ward\nevent set ap\nevent set basedamage\nevent set baseheal\nevent set potion <command>\nevent set order <command>");
                }

                [Command("ruleset")]
                [Summary("Event set ruleset help")]
                public async Task SetRuleset()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set ruleset <ruleset ID>` changes the event ruleset.\nAvailable rulesets:\nAmentia's ruleset - **amentia**\nAmentia's second ruleset - **amentia2**");
                }

                [Command("npc")]
                [Summary("Changes whether a character has ranged attacks or not.")]
                public async Task SetNpc()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set npc <character alias> <true/false> [event ID]` set a character to be a normal character or an npc.");
                }

                [Command("melee")]
                [Summary("Event set ranged help")]
                public async Task Setmelee()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set melee <character alias> <true/false> [event ID]` enables or disables melee attacks for a character.");
                }

                [Command("ranged")]
                [Summary("Event set ranged help")]
                public async Task SetRanged()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set ranged <character alias> <true/false> [event ID]` enables or disables ranged attacks for a character.");
                }

                [Command("team")]
                [Summary("Event set team help")]
                public async Task SetTeam()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set team <team alias> <character alias 1> ... <character alias n>` assigns the characters to the given team, a character can only be in one team at a time.");
                }

                [Command("turn")]
                [Summary("Event set turn help")]
                public async Task SetTurn()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set turn <character alias>` gives the turn to the given character.");
                }

                [Command("fixedorder")]
                [Summary("Event set turn help")]
                public async Task SetFixedOrder()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set fixedorder <true/false>` Enables or disables fixed order, fixed order forces characters to play on their turn only, otherwise any character can play at any time.");
                }

                [Command("maxhealth")]
                [Summary("Event set maxhealth help")]
                public async Task SetMaxhealth()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set maxhealth <character alias> <health>` sets the character's max health to the given value.");
                }

                [Command("health")]
                [Summary("Event set health help")]
                public async Task SetHealth()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set health <character alias> <health>` sets the character's health to the given value.");
                }

                [Command("ward")]
                [Summary("Event set ward help")]
                public async Task SetWard()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set ward <character alias> <health> <duration>` adds a ward to the character with the given health and duration.");
                }

                [Command("ap")]
                [Summary("Event set ap help")]
                public async Task SetAp()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set ap <character alias> <action points>` sets the character's action points to the given value.");
                }

                [Command("basedamage")]
                [Summary("Event set base damage help")]
                public async Task SetBasedamage()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set basedamage <damage>` sets the events's base damage to the given value.");
                }

                [Command("baseheal")]
                [Summary("Event set base heal help")]
                public async Task SetBaseheal()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event set baseheal <health>` sets the events's base heal to the given value.");
                }

                [Group("potion")]
                [Summary("All commands for setting specific properties of the event.")]
                public class EventSetPotionModule : ModuleBase<SocketCommandContext>
                {
                    [Command]
                    [Summary("Event set potion help")]
                    public async Task SetPotion()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync(
                            "`!event set potion <command>` allows you to change the number of potions a character has for an event with these commands:\nevent set potion health");
                    }

                    // Health potions
                    [Command("health")]
                    [Summary("Health potions")]
                    public async Task SetHealthPotion()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync(
                            "`!event set potion health <character alias> <amount>` sets the amount of health potions a character has.");
                    }
                }

                [Group("order")]
                [Summary("All commands for setting specific properties of the event.")]
                public class EventSetOrderModule : ModuleBase<SocketCommandContext>
                {
                    [Command]
                    [Summary("Event set order help")]
                    public async Task SetPotion()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync(
                            "`!event set order <command>` allows you to change the teams and characters for an event with these commands:\nevent set order teams");
                    }

                    // Teams
                    [Command("teams")]
                    [Summary("Team order")]
                    public async Task SetOrderTeams()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync(
                            "`!event set order teams [<team alias> ... <team alias>]` sets the order of teams regarding when they get their turns, if you enter this without any team aliases you will get a list of team aliases.");
                    }
                }
            }

            // Set event details
            [Group("disable")]
            [Summary("All commands for disabling specific properties of the event.")]
            public class EventDisableModule : ModuleBase<SocketCommandContext>
            {
                [Command]
                [Summary("Event disable help")]
                public async Task Disable()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event disable <command>` allows you to disable certain things for an event with these commands:\nevent disable potion <command>");
                }

                // Set event details
                [Group("potion")]
                [Summary("All commands for setting specific properties of the event.")]
                public class EventDisablePotionModule : ModuleBase<SocketCommandContext>
                {
                    [Command]
                    [Summary("Event disable potion help")]
                    public async Task DisablePotion()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync(
                            "`!event disable potion <command>` allows you to disable different potions for an event with these commands:\nevent disable potion health");
                    }

                    // melee attacks
                    [Command("health")]
                    [Summary("Health potions")]
                    public async Task DisableHealthPotion()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync(
                            "`!event disable potion health` disables health potions and removes any existing health potions.");
                    }
                }
            }

            // Set event details
            [Group("enable")]
            [Summary("All commands for enabling specific properties of the event.")]
            public class EventEnableModule : ModuleBase<SocketCommandContext>
            {
                [Command]
                [Summary("Event enable help")]
                public async Task Enable()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event enable <command>` allows you to enable certain things for an event with these commands:\nevent enable potion <command>");
                }

                // Set event details
                [Group("potion")]
                [Summary("All commands for setting specific properties of the event.")]
                public class EventEnablePotionModule : ModuleBase<SocketCommandContext>
                {
                    [Command]
                    [Summary("Event enable potion help")]
                    public async Task EnablePotion()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync(
                            "`!event enable potion <command>` allows you to enable different potions for an event with these commands:\nevent enable potion health");
                    }

                    // melee attacks
                    [Command("health")]
                    [Summary("Health potions")]
                    public async Task EnableHealthPotion()
                    {
                        IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync(
                            "`!event enable potion health <min potions> <max potions>` enables health potions for the event and grants a randomly selected amount of health potions in the given range to existing characters.");
                    }
                }

            }

            // Event Hide
            [Group("hide")]
            [Summary("Commands for hiding characters and teams.")]
            public class EventHideModule : ModuleBase<SocketCommandContext>
            {
                [Command]
                [Summary("Event hide help")]
                public async Task Hide()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event hide <command>` allows you to hide a character or a team for an event with these commands:\nevent hide team\nevent hide character");
                }

                // Hide team
                [Command("team")]
                [Summary("Hide team")]
                public async Task HideTeam()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event hide team <team alias>` hides a team and all its characters.");
                }

                // Hide character
                [Command("character")]
                [Summary("Hide character")]
                public async Task HideCharacter()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event hide character <character alias>` hides a character until they are revealed through some action or the reveal command.");
                }
            }

            // Event reveal
            [Group("reveal")]
            [Summary("Commands for revealing characters and teams.")]
            public class EventRevealModule : ModuleBase<SocketCommandContext>
            {
                [Command]
                [Summary("Event reveal help")]
                public async Task Reveal()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event reveal <command>` allows you to reveal a character or a team for an event with these commands:\nevent reveal team\nevent reveal character");
                }

                // Hide team
                [Command("team")]
                [Summary("Reveal team")]
                public async Task RevealTeam()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event reveal team <team alias>` reveals a team and all its characters.");
                }

                // Hide character
                [Command("character")]
                [Summary("Reveal character")]
                public async Task RevealCharacter()
                {
                    IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(
                        "`!event reveal character <character alias>` reveals a character.");
                }
            }
        }

        [Command("feedback")]
        [Summary("Feedback help")]
        public async Task Feedback()
        {
            IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync(
                "`!feedback <feedback text>` Let me know how I'm doing and what I can get better at!\nI'll do my very best to be the best BattleBot I can be. " +
                Emotes.Heart);
        }

        [Command("roll")]
        [Summary("Roll a dice help")]
        public async Task Roll()
        {
            IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync(
                "`!roll <dice(s)>` Here you can simulate rolling dices!\nJust type !roll d<dice size>, for example !roll d10 for a d10 dice. You may also roll multiple dices at a time by typing !roll d10+d10.\nAdd, subtract, multiply or divide by changing the + for a -, * or /.");
        }

        [Command("flipcoin")]
        [Summary("Flip a coin help")]
        public async Task Flip()
        {
            IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync("`!flipcoin` Here you can flip a coin to get either heads or tail.");
        }

        [Command("attack")]
        [Summary("Attack help")]
        public async Task Attack()
        {
            IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync(
                "`!attack <character alias> <target-1 alias> ... <target-n alias>` the character will attempt to attack one or more targets according to the event ruleset. Yarrrrr! :crossed_swords:\nThis only works after the event has started.");
        }

        [Command("heal")]
        [Summary("Heal help")]
        public async Task Heal()
        {
            IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync(
                "`!heal <character alias> <target-1 alias> ... <target-n alias>` the character will attempt to heal one or more targets according to the event ruleset. :hearts:\nThis only works after the event has started.");
        }

        [Command("ward")]
        [Summary("Ward help")]
        public async Task Ward()
        {
            IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync(
                "`!ward <character alias> <target-1 alias> ... <target-n alias>` the character will attempt to ward one or more targets according to the event ruleset. :shield:\nThis only works after the event has started.");
        }

        [Command("block")]
        [Summary("Block help")]
        public async Task Block()
        {
            IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync(
                "`!block <character alias>` the character will block one attack this round. :shield:\nThis only works after the event has started.");
        }

        [Group("potion")]
        public class PotionModule : ModuleBase<SocketCommandContext>
        {
            [Command]
            [Summary("Potion help")]
            public async Task Remove()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(
                    "`!potion <command>` allows you use different potions during an event:\nevent health");
            }

            [Command("health")]
            [Summary("Health potion help")]
            public async Task RemoveAdmin()
            {
                IDMChannel channel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync("`!event health <character alias>` the character attempts to use a health potion.");
            }
        }
    }
}

