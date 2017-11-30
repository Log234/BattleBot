using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RP_Bot
{
    static class Data
    {
        public static Dictionary<ulong, User> users = new Dictionary<ulong, User>();
        public static Dictionary<ulong, Guild> guilds = new Dictionary<ulong, Guild>();
        public static Dictionary<ulong, Channel> channels = new Dictionary<ulong, Channel>();

        public static Event GetEvent(ulong channelId, string eventId)
        {
            if (!channels.TryGetValue(channelId, out Channel channel))
            {
                return null;
            }

            if (!channel.events.TryGetValue(eventId, out Event curEvent))
            {
                return null;
            }

            return curEvent;
        }

        // Gets or creates a guild based on a
        public static Guild GetGuild(SocketGuild guild)
        {
            if (!Data.guilds.TryGetValue(guild.Id, out Guild curGuild))
            {
                curGuild = new Guild(guild);
                guilds[guild.Id] = curGuild;
            }

            return curGuild;
        }

        // Gets or creates a channel based on a SocketChannel
        public static Channel GetChannel(SocketChannel channel)
        {
            if (!Data.channels.TryGetValue(channel.Id, out Channel curChannel))
            {
                if (channel is SocketGuildChannel)
                {
                    curChannel = new GuildChannel((SocketGuildChannel)channel);
                }
                else
                {
                    curChannel = new Channel(channel);
                }
                Data.channels[channel.Id] = curChannel;
            }

            return curChannel;
        }

        // Gets or creates a user based on a SocketUser
        public static User GetUser(SocketUser user)
        {
            if (!Data.users.TryGetValue(user.Id, out User curUser))
            {
                curUser = new User(user);
                Data.users.Add(user.Id, curUser);
            }

            return curUser;
        }

        // Get event based on context
        public static Event GetEvent(SocketCommandContext Context, string eventId = "Active")
        {
            Event curEvent;
            if (eventId.Equals("Active"))
            {
                curEvent = Data.users[Context.Message.Author.Id]?.activeEvents[Context.Channel.Id];
            }
            else
            {
                curEvent = Data.channels[Context.Channel.Id]?.events[eventId];
            }

            return curEvent;
        }
    }

    class User
    {
        public ulong Id { get; }
        public string Tag { get; }
        public string Username { get; }
        public string Discriminator { get; }
        public SocketUser SocketUser { get; }
        public DateTimeOffset Idle { get; set; }
        public HashSet<Character> characters = new HashSet<Character>();
        public Dictionary<ulong, Event> activeEvents = new Dictionary<ulong, Event>();
        private int eventID = 0;

        public User(SocketUser user)
        {
            Id = user.Id;
            Username = user.Username;
            Discriminator = user.Discriminator;
            Tag = "@" + Username + "#" + Discriminator;
            SocketUser = user;
            Console.WriteLine("User created: " + user.Username);
        }

        public int getEventID()
        {
            Idle = DateTimeOffset.Now;
            return eventID++;
        }

        public Character GetCharacter(string alias)
        {
            alias = alias.ToLower();
            foreach (Character character in characters)
            {
                foreach (string charAlias in character.aliases)
                {
                    if (alias.Equals(charAlias))
                        return character;
                }
            }

            return null;
        }

        public void SetActiveEvent(ulong channelId, Event curEvent)
        {
            if (!activeEvents.TryAdd(channelId, curEvent))
            {
                activeEvents[channelId] = curEvent;
            }
        }
    }

    class Guild
    {
        public ulong Id { get; }
        public SocketGuild SocketGuild { get; }
        public DateTimeOffset Idle { get; set; }
        private HashSet<User> admins = new HashSet<User>();
        private Dictionary<ulong, Channel> channels = new Dictionary<ulong, Channel>();

        public Guild(SocketGuild guild)
        {
            Id = guild.Id;
            SocketGuild = guild;
            Console.WriteLine("Guild created: " + SocketGuild.Name);
        }

        // Admins
        public void AddAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            admins.Add(user);
        }

        public void RemoveAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            admins.Remove(user);
        }

        public bool IsAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            return admins.Contains(user);
        }

    }

    class Channel
    {
        public ulong Id { get; }
        public SocketChannel SocketChannel { get; }
        public DateTimeOffset Idle { get; set; }
        protected HashSet<User> admins = new HashSet<User>();
        public Dictionary<String, Event> events = new Dictionary<string, Event>();
        public Event ActiveEvent { get; set; }

        public Channel(SocketChannel channel)
        {
            Id = channel.Id;
            SocketChannel = channel;
            Console.WriteLine("Channel created: " + Id);
        }

        public string GetEventName(User dm)
        {
            return dm.Username + "#" + dm.getEventID();
        }

        // Admins
        public void AddAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            admins.Add(user);
        }

        public void RemoveAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            admins.Remove(user);
        }

        public virtual bool IsAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            return admins.Contains(user);
        }
    }

    class GuildChannel : Channel
    {
        public Guild Guild { get; }

        public GuildChannel(SocketGuildChannel channel) : base(channel)
        {
            Guild = Data.GetGuild(channel.Guild);
            Console.WriteLine("Channel created: " + channel.Name);
        }

        public override bool IsAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            return admins.Contains(user) || Guild.IsAdmin(user);
        }
    }

    class Event
    {
        public String Id { get; }
        public Channel Channel { get; }
        public DateTimeOffset Created { get; }
        public DateTimeOffset Idle { get; set; }
        public Ruleset Ruleset { get; set; }
        public User Dm { get; }
        private HashSet<ulong> admins = new HashSet<ulong>();
        private List<Team> teams = new List<Team>();
        public HashSet<Character> characters = new HashSet<Character>();
        public HashSet<ulong> users = new HashSet<ulong>();

        public int Round { get; internal set; }
        private string log;

        public Event(User dm, Channel channel)
        {
            Id = channel.GetEventName(dm);
            Channel = channel;
            Created = DateTimeOffset.Now;
            Idle = DateTimeOffset.Now;
            Ruleset = new AmentiaRuleset();
            Dm = dm;
            Round = 0;
            log = "";

            channel.events[Id] = this;
            dm.activeEvents.Add(channel.Id, this);
            users.Add(dm.Id);

            Console.WriteLine("Created event: " + Id);
        }

        // Admins
        public string AddAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            admins.Add(user.Id);

            return Log($"{user.Tag} has been added as administrator of {Id}.");
        }

        public string RemoveAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            admins.Remove(user.Id);

            return Log($"{user.Tag} is nolonger administrator of {Id}.");
        }

        public bool IsAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            return admins.Contains(user.Id) || Dm.Equals(user) || Channel.IsAdmin(user);
        }

        // Commands

        public string Start(User user)
        {
            Idle = DateTimeOffset.Now;

            if (IsAdmin(user))
            {
                if (Channel.ActiveEvent == null)
                {
                    Round = 1;
                    Channel.ActiveEvent = this;
                    string msg = $"The event **{Id}** started!\n";
                    foreach (ulong curUser in users)
                    {
                        msg += Data.users[curUser].SocketUser.Mention + " ";
                    }

                    return Log(msg);
                }
                else
                {
                    return Log($"There is already an event running in this channel: {Channel.ActiveEvent.Id}.");
                }
            }
            else
            {
                return Log("You do not have permission to start that event.");
            }
        }

        public string Join(User user)
        {
            Idle = DateTimeOffset.Now;

            if (!users.Contains(user.Id))
            {
                users.Add(user.Id);
                user.SetActiveEvent(Channel.Id, this);

                return Log($"{user.Username} has joined {Id}.");
            }
            else
            {
                return Log("You have already joined this event.");
            }
        }

        public string Join(Character character, User user)
        {
            Idle = DateTimeOffset.Now;
            if (HasStarted() && !Ruleset.JoinAfterStart && !IsAdmin(user))
            {
                return Log("The event has already started, so you cannot join.");
            }

            AddCharacter(character, user);
            Join(user);

            return Log($"{user.Username} has joined {Id}, with {character.Name}!");
        }

        public string AddCharacter(Character character, User user)
        {
            Idle = DateTimeOffset.Now;
            if (HasStarted() && !Ruleset.JoinAfterStart && !IsAdmin(user))
            {
                return Log("The event has already started, so you cannot add a new character.");
            }

            Character joiner = character.CopyOf();

            if (joiner.Npc)
            {
                joiner.Maxhealth = Ruleset.NpcMaxhealth;
                joiner.Health = Ruleset.NpcMaxhealth;
            }
            else
            {
                joiner.Maxhealth = Ruleset.CharMaxhealth;
                joiner.Health = Ruleset.CharMaxhealth;
            }

            if (HasStarted() && Ruleset.WaitOnJoin)
                joiner.Actionpoints = 0;
            else
                joiner.Actionpoints = Ruleset.Actionpoints;
            characters.Add(joiner);

            return Log($"{character.Name} was added to {Id}.");
        }

        public string AddTeam(string name)
        {
            Team team = new Team(name);
            teams.Add(team);

            return Log($"The team {name} was added to {Id}.");
        }

        public string SetTeam(Character character, Team team)
        {
            string result = "";

            foreach (Team curTeam in teams)
            {
                if (curTeam.members.Contains(character))
                {
                    result += $"{character.Name} was removed from {curTeam.Name}.\n";
                    curTeam.members.Remove(character);
                }
            }
            result += $"{character.Name} was added to {team.Name}.\n";
            team.members.Add(character);

            return Log(result);
        }

        public string SetMaxhealth(Character character, int health)
        {
            Idle = DateTimeOffset.Now;
            character.Maxhealth = health;

            return Log($"{character.Name}'s max health is now {health}.");
        }

        public string SetHealth(Character character, int health)
        {
            Idle = DateTimeOffset.Now;
            character.Health = health;

            return Log($"{character.Name}'s health is now {health}.");
        }

        public string Attack(Character character, Character[] targets)
        {
            Idle = DateTimeOffset.Now;
            string result = Ruleset.Attack(character, targets);

            return Log(CheckRound(result));
        }

        public string Heal(Character character, Character[] targets)
        {
            Idle = DateTimeOffset.Now;
            string result = Ruleset.Heal(character, targets);

            return Log(CheckRound(result));
        }

        public string Ward(Character character, Character[] targets)
        {
            Idle = DateTimeOffset.Now;
            string result = Ruleset.Ward(character, targets);

            return Log(CheckRound(result));
        }

        // Utilities

        public bool HasStarted()
        {
            return Round > 0;
        }

        public string Log(string msg)
        {
            log += msg + "\n";
            Console.WriteLine(msg);
            return msg;
        }

        public string CheckRound(string result)
        {
            if (RemainingAP() != 0)
                return result;

            if (CheckVictoryCondition(result, out string sum))
            {
                Channel.ActiveEvent = null;
                return sum;
            }

            foreach (Character character in characters)
            {
                character.UpdateWards();
                if (character.Health > 0) character.Actionpoints = Ruleset.Actionpoints;
            }

            string status = result + RoundStatus();

            return status;
        }

        public int RemainingAP()
        {
            int ap = 0;
            foreach (Character character in characters)
            {
                if (!character.Npc) ap += character.Actionpoints;
            }

            return ap;
        }

        public bool CheckVictoryCondition(string result, out string sum)
        {
            if (teams.Count > 0)
            {
                List<Team> remaining = new List<Team>();

                foreach (Team team in teams)
                {
                    if (!team.IsDead())
                    {
                        remaining.Add(team);
                    }
                }

                if (remaining.Count == 1)
                {
                    sum = result + $"\n\n\n-------------- {remaining[0].Name} has won! --------------";
                    return true;
                }
            }
            sum = result;
            return false;
        }

        public string RoundStatus()
        {
            string status = $"\n\n\n-------------- Round {Round++} is done --------------";

            if (teams.Count > 0)
            {
                foreach (Team team in teams)
                {
                    status += "\n" + team.GetStatus() + "\n";
                }
            }
            else
            {
                status += GetNpcStatus();
            }

            return status;
        }

        public string GetNpcStatus()
        {
            string status = "\nCharacters:";

            foreach (Character character in characters)
            {
                if (!character.Npc) status += "\n" + character.GetStatus();
            }

            status += "\n\nNPCs:";

            foreach (Character character in characters)
            {
                if (character.Npc) status += "\n" + character.GetStatus();
            }

            return status;
        }

        public string Finish()
        {
            string path = Id + ".txt";
            File.WriteAllText(path, log);
            Channel.events.Remove(Id);

            foreach (ulong curUser in users)
            {
                User user = Data.users[curUser];
                if (user?.activeEvents[Channel.Id]?.Id == Id)
                {
                    user.activeEvents.Remove(Channel.Id);
                }
            }

            return path;
        }

        public Character GetCharacter(string alias)
        {
            alias = alias.ToLower();
            foreach (Character character in characters)
            {
                foreach (string charAlias in character.aliases)
                {
                    if (alias.Equals(charAlias))
                        return character;
                }
            }

            return null;
        }

        public Team GetTeam(string alias)
        {
            alias = alias.ToLower();
            foreach (Team team in teams)
            {
                foreach (string teamAlias in team.aliases)
                {
                    if (alias.Equals(teamAlias))
                        return team;
                }
            }

            return null;
        }

    }

    // Teams
    class Team
    {
        public string Name { get; }
        public HashSet<string> aliases = new HashSet<string>();
        public List<Character> members = new List<Character>();

        public Team(string name)
        {
            this.Name = name;
            this.aliases.Add(name.ToLower());
            if (name.Contains(" "))
            {
                this.aliases.Add(name.Substring(0, name.IndexOf(" ")).ToLower());
            }
        }

        public bool IsDead()
        {
            foreach (Character member in members)
            {
                if (member.Health > 0) return false;
            }

            return true;
        }

        public string GetStatus()
        {
            string status = Name + ":";

            foreach (Character member in members)
            {
                status += "\n" + member.GetStatus();
            }

            return status;
        }
    }

    // Characters
    class Character
    {
        public string Name { get; }
        public HashSet<string> aliases = new HashSet<string>();
        public bool Npc { get; set; }
        public List<User> admins = new List<User>();
        public List<Ward> wards = new List<Ward>();
        public int Health { get; set; }
        public int Maxhealth { get; set; }
        public int Actionpoints { get; set; }

        public Character(string name, int health, int maxhealth, User admin)
        {
            this.Name = name;
            this.aliases.Add(name.ToLower());
            if (name.Contains(" "))
            {
                this.aliases.Add(name.Substring(0, name.IndexOf(" ")).ToLower());
            }
            this.Health = health;
            this.Maxhealth = maxhealth;
            this.admins.Add(admin);
        }

        public Character(string name, HashSet<string> aliases, bool npc, int health, int maxhealth, List<User> admin)
        {
            this.Name = name;
            this.aliases = new HashSet<string>(aliases);
            this.Npc = npc;
            this.Health = health;
            this.Maxhealth = maxhealth;
            this.admins = new List<User>(admin);
        }

        public Character CopyOf()
        {
            return new Character(Name, aliases, Npc, Health, Maxhealth, admins);
        }

        public void TakeDamage(int amount)
        {
            for (int i = 0; i < wards.Count; i++)
            {
                amount = wards[i].TakeDamage(amount);
                if (wards[i].Shield == 0) wards.RemoveAt(i);
                if (amount == 0) return;
            }
            Health -= amount;
            if (Health < 0) Health = 0;
        }

        public void Heal(int amount)
        {
            Health += amount;
            if (Health > Maxhealth) Health = Maxhealth;
        }

        public void Ward(Ward ward)
        {
            wards.Add(ward);
        }

        public void Action(int points)
        {
            Actionpoints -= points;
        }

        public string GetStatus()
        {
            string status = Name + ": " + Health;

            if (GetWards() > 0) status += " + " + GetWards();

            status += " / " + Maxhealth;

            return status;
        }

        public int GetWards()
        {
            int shields = 0;

            foreach (Ward ward in wards)
            {
                shields += ward.Shield;
            }

            return shields;
        }

        public void UpdateWards()
        {
            foreach (Ward ward in wards)
            {
                if (ward.Duration <= 0) wards.Remove(ward);
                ward.NextRound();
            }
        }
    }

    // Wards
    class Ward
    {
        public int Shield { get; internal set; }
        public int Duration { get; internal set; }

        public Ward(int shield, int duration)
        {
            this.Shield = shield;
            this.Duration = duration;
        }

        public int TakeDamage(int amount)
        {
            int remainder = amount - Shield;
            if (remainder > 0) return remainder;
            return 0;
        }

        public int NextRound()
        {
            return Duration--;
        }
    }

    // DoTs
    class Dot
    {
        public int Damage { get; }
        public int Frequency { get; }
        public int Duration { get; internal set; }

        public Dot(int dmg, int freq, int dur)
        {
            Damage = dmg;
            Frequency = freq;
            Duration = dur;
        }

    }
}
