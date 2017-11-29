using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
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
            Channel channel;
            if (!channels.TryGetValue(channelId, out channel))
            {
                return null;
            }
            
            Event curEvent;
            if(!channel.events.TryGetValue(eventId, out curEvent))
            {
                return null;
            }

            return curEvent;
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
            Guild guild;
            if (!Data.guilds.TryGetValue(channel.Guild.Id, out guild))
            {
                guild = new Guild(channel.Guild);
                Data.guilds[guild.Id] = guild;
                Guild = guild;
                Console.WriteLine("Created channel: " + channel.Name);
            }
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
        private HashSet<User> admins = new HashSet<User>();
        public HashSet<Character> characters = new HashSet<Character>();
        HashSet<User> users = new HashSet<User>();
        
        public int Round { get; internal set; }

        public Event(User dm, Channel channel)
        {
            Id = channel.GetEventName(dm);
            Channel = channel;
            Created = DateTimeOffset.Now;
            Idle = DateTimeOffset.Now;
            Ruleset = new AmentiaRuleset();
            Dm = dm;
            Round = 0;

            channel.events[Id] = this;

            Console.WriteLine("Created event: " + Id);
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
            return admins.Contains(user) || Dm.Equals(user) || Channel.IsAdmin(user);
        }

        // Commands

        public string Start(User user)
        {
            if (IsAdmin(user))
            {
                if (Channel.ActiveEvent == null)
                {
                    Round = 1;
                    Channel.ActiveEvent = this;
                    return $"The event **{Id}** started!\nType **!event join {Id}** to join.";
                } else
                {
                    return $"There is already an event running in this channel: {Channel.ActiveEvent.Id}.";
                }
            } else {
                return "You do not have permission to start that event.";
            }
        }

        public string Join(User user)
        {
            users.Add(user);
            user.activeEvents.Add(Channel.Id, this);

            return $"{user.Username} has joined {Id}.";
        }

        public string Join(Character character, User user)
        {
            if (HasStarted() && !Ruleset.JoinAfterStart && !IsAdmin(user))
            {
                return "The event has already started, so you cannot join.";
            }

            AddCharacter(character);
            Join(user);

            return $"{user.Username} has joined {Id}, with {character.Name}!";
        }

        public string AddCharacter(Character character)
        {
            Character joiner = character.CopyOf();
            joiner.Maxhealth = Ruleset.CharMaxhealth;
            joiner.Health = Ruleset.CharMaxhealth;

            if (Ruleset.WaitOnJoin)
                joiner.Actionpoints = 0;
            else
                joiner.Actionpoints = Ruleset.Actionpoints;
            characters.Add(joiner);

            return $"{character.Name} was added to {Id}.";
        }

        // Utilities

        public Boolean HasStarted()
        {
            return Round > 0;
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
            this.aliases.Add(name.Substring(0, name.IndexOf(" ")).ToLower());
            this.Health = health;
            this.Maxhealth = maxhealth;
            this.admins.Add(admin);
        }

        public Character(string name, HashSet<string> aliases, bool npc, int health, int maxhealth, List<User> admin)
        {
            this.Name = name;
            this.aliases = new HashSet<string>(aliases);
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
