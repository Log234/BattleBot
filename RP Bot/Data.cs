using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace BattleBot
{
    class Data
    {
        public static DiscordSocketClient client;
        private static Storage storage = new Storage();

        [Serializable]
        class Storage
        {
            public Dictionary<ulong, User> users = new Dictionary<ulong, User>();
            public Dictionary<ulong, Guild> guilds = new Dictionary<ulong, Guild>();
            public Dictionary<ulong, Channel> channels = new Dictionary<ulong, Channel>();
        }

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

        // Message all active event DMs
        public static async Task MessageActiveDMs(string msg)
        {
            SocketUser log = client.GetUser(174426714120781824);
            Discord.IDMChannel dm = await log.GetOrCreateDMChannelAsync();
            await dm.SendMessageAsync(msg);
            Console.WriteLine(msg);

            Storage storage = GetStorage;
            foreach (Channel channel in storage.channels.Values)
            {
                if (channel.ActiveEvent != null && (DateTimeOffset.Now - channel.ActiveEvent.Idle).TotalMinutes < 10)
                {
                    SocketUser user = client.GetUser(dm.Id);
                    SocketChannel socketChannel = client.GetChannel(channel.Id);
                    string mention = user.Mention;
                    (socketChannel as SocketTextChannel)?.SendMessageAsync(mention + "\n" + msg);
                }
            }
        }

        // Save and exit
        public static void Exit()
        {
            Console.WriteLine("Saving");
            Save();
            Program.exit = true;
            Console.WriteLine("Shutting down");
            Environment.Exit(0);

        }

        // Prepare for update
        public static async Task Update()
        {
            await MessageActiveDMs("I will be taken offline in 15 minutes. The state of your active event will be saved, but it may take a few minutes before I come back.");
            await (Task.Delay(600000));
            await MessageActiveDMs("I will be taken offline in 5 minutes.");
            await (Task.Delay(240000));
            await MessageActiveDMs("I will be taken offline in 1 minute.");
            await (Task.Delay(60000));
            await MessageActiveDMs("Going offline, see you soon! :heart:");
            Exit();
        }

        private static void Save()
        {
            Console.WriteLine("Saving Data.dat");
            Storage storage = GetStorage;
            using (Stream stream = File.Open("Data.dat", FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, storage);
            }

            Console.WriteLine("Saving backup");
            Directory.CreateDirectory("Backup");
            using (Stream stream = File.Open($"Backup/Data-{Utilities.GetDateTime()}.dat", FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, storage);
            }

        }

        public static void Load()
        {
            if (!File.Exists("Data.dat"))
            {
                Console.WriteLine("Could not find Data.dat");
                return;
            }

            Console.WriteLine("Loading Data.dat");
            using (Stream stream = File.Open("Data.dat", FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                storage =  (Storage)binaryFormatter.Deserialize(stream);
            }
        }

        // Cleanup
        public static void OnCleanup(Object source, ElapsedEventArgs e)
        {
            foreach (KeyValuePair<ulong, Channel> channel in storage.channels)
            {
                foreach (KeyValuePair<String, Event> eventPair in channel.Value.events)
                {
                    if ((DateTime.Now - eventPair.Value.Idle).TotalDays > Cleanup.eventTimeout)
                    {
                        if (channel.Value.events.Count == 1)
                        {
                            storage.channels.Remove(channel.Key);
                        }
                        else
                        {
                            channel.Value.events.Remove(eventPair.Key);
                        }
                    }
                }
            }
        }

        // Reconnect
        public static async Task OnReconnect()
        {
            await Data.MessageActiveDMs("Beep Boop!\nI'm back! :smile:");
        }
    }

    [Serializable]
    class User
    {
        public ulong Id { get; }
        public string Tag { get; }
        public string Username { get; }
        public string Discriminator { get; }
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

    [Serializable]
    class Guild
    {
        public ulong Id { get; }
        public DateTimeOffset Idle { get; set; }
        private HashSet<User> admins = new HashSet<User>();
        private Dictionary<ulong, Channel> channels = new Dictionary<ulong, Channel>();

        public Guild(SocketGuild guild)
        {
            Id = guild.Id;
            Console.WriteLine("Guild created: " + guild.Name);
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

    [Serializable]
    class Channel
    {
        public ulong Id { get; }
        public DateTimeOffset Idle { get; set; }
        protected HashSet<User> admins = new HashSet<User>();
        public Dictionary<String, Event> events = new Dictionary<string, Event>();
        public Event ActiveEvent { get; set; }

        public Channel(SocketChannel channel)
        {
            Id = channel.Id;
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

    [Serializable]
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
    [Serializable]
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
    [Serializable]
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
    [Serializable]
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
    [Serializable]
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
