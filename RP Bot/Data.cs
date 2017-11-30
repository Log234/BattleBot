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

        class Storage
        {
            public Dictionary<ulong, User> users = new Dictionary<ulong, User>();
            public Dictionary<ulong, Guild> guilds = new Dictionary<ulong, Guild>();
            public Dictionary<ulong, Channel> channels = new Dictionary<ulong, Channel>();
        }

        private static Storage storage = new Storage();

        static Storage GetStorage
        {
            get { return storage; }
        }

        public static Event GetEvent(ulong channelId, string eventId)
        {
            if (!storage.channels.TryGetValue(channelId, out Channel channel))
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
            if (!storage.guilds.TryGetValue(guild.Id, out Guild curGuild))
            {
                curGuild = new Guild(guild);
                storage.guilds[guild.Id] = curGuild;
            }

            return curGuild;
        }

        // Gets or creates a channel based on a SocketChannel
        public static Channel GetChannel(SocketChannel channel)
        {
            if (!storage.channels.TryGetValue(channel.Id, out Channel curChannel))
            {
                if (channel is SocketGuildChannel)
                {
                    curChannel = new GuildChannel((SocketGuildChannel)channel);
                }
                else
                {
                    curChannel = new Channel(channel);
                }
                storage.channels[channel.Id] = curChannel;
            }

            return curChannel;
        }

        // Gets or creates a user based on a SocketUser
        public static User GetUser(SocketUser user)
        {
            if (!storage.users.TryGetValue(user.Id, out User curUser))
            {
                curUser = new User(user);
                storage.users.Add(user.Id, curUser);
            }

            return curUser;
        }

        // Gets or creates a user based on a SocketUser
        public static User GetUser(ulong userId)
        {
            if (!storage.users.TryGetValue(userId, out User curUser))
            {
                return null;
            }

            return curUser;
        }

        // Get event based on context
        public static Event GetEvent(SocketCommandContext Context, string eventId = "Active")
        {
            Event curEvent;
            if (eventId.Equals("Active"))
            {
                curEvent = storage.users[Context.Message.Author.Id]?.activeEvents[Context.Channel.Id];
            }
            else
            {
                curEvent = storage.channels[Context.Channel.Id]?.events[eventId];
            }

            return curEvent;
        }

        // Save and exit
        public static void Exit()
        {
            Save();
            Environment.Exit(0);

        }

        // Prepare for update
        public static void Update()
        {
            Storage storage = GetStorage;
            foreach (KeyValuePair<ulong, Channel> channel in storage.channels)
            {
                if (channel.Value.ActiveEvent != null)
                {

                }
            }
        }

        private static void Save()
        {
            Storage storage = GetStorage;
            using (Stream stream = File.Open("Data.dat", FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, storage);
            }

        }

        public static void Load()
        {
            if (!File.Exists("Data.dat")) return;

            using (Stream stream = File.Open("Data.dat", FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                storage =  (Storage)binaryFormatter.Deserialize(stream);
            }
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
