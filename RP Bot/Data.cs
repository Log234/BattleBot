using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BattleBot
{
    internal class Data
    {
        public static DiscordSocketClient Client;
        private static Storage _storage = new Storage();

        [Serializable]
        private class Storage
        {
            public Dictionary<ulong, User> users = new Dictionary<ulong, User>();
            public Dictionary<ulong, Guild> guilds = new Dictionary<ulong, Guild>();
            public Dictionary<ulong, Channel> channels = new Dictionary<ulong, Channel>();
        }

        private static Storage GetStorage
        {
            get { return _storage; }
        }

        public static Event GetEvent(ulong channelId, string eventId)
        {
            if (!_storage.channels.TryGetValue(channelId, out Channel channel))
            {
                return null;
            }

            if (!channel.events.TryGetValue(eventId.ToLower(), out Event curEvent))
            {
                return null;
            }

            return curEvent;
        }

        // Get event based on context
        public static Event GetEvent(SocketCommandContext context, string eventId = "Active")
        {
            if (context.Channel is SocketDMChannel)
            {
                User user = GetUser(context.Message.Author);
                return user.CurrentEvent;
            }

            Event curEvent = null;
            if (eventId.Equals("Active"))
            {
                _storage.users.TryGetValue(context.Message.Author.Id, out User user);
                user?.ActiveEvents.TryGetValue(context.Channel.Id, out curEvent);
            }
            else
            {
                if (_storage.channels.TryGetValue(context.Channel.Id, out Channel channel) && channel.events.TryGetValue(eventId.ToLower(), out Event newEvent))
                {
                    return newEvent;
                }
                else
                    return null;
            }

            return curEvent;
        }

        // Gets or creates a guild based on a
        public static Guild GetGuild(SocketGuild guild)
        {
            if (!_storage.guilds.TryGetValue(guild.Id, out Guild curGuild))
            {
                curGuild = new Guild(guild);
                _storage.guilds[guild.Id] = curGuild;
            }

            return curGuild;
        }

        // Gets or creates a guild based on a
        public static Guild GetGuild(ulong guildId)
        {
            if (!_storage.guilds.TryGetValue(guildId, out Guild curGuild))
            {
                return null;
            }

            return curGuild;
        }

        // Gets or creates a channel based on a SocketChannel
        public static Channel GetChannel(SocketChannel channel)
        {
            if (!_storage.channels.TryGetValue(channel.Id, out Channel curChannel))
            {
                if (channel is SocketGuildChannel)
                {
                    curChannel = new GuildChannel((SocketGuildChannel)channel);
                }
                else
                {
                    curChannel = new Channel(channel);
                }
                _storage.channels[channel.Id] = curChannel;
            }

            return curChannel;
        }

        // Gets or creates a user based on a SocketUser
        public static User GetUser(SocketUser user)
        {
            if (!_storage.users.TryGetValue(user.Id, out User curUser))
            {
                curUser = new User(user);
                _storage.users.Add(user.Id, curUser);
            }

            return curUser;
        }

        // Gets or creates a user based on a SocketUser
        public static User GetUser(ulong userId)
        {
            if (!_storage.users.TryGetValue(userId, out User curUser))
            {
                return null;
            }

            return curUser;
        }

        // Message all active event DMs
        public static async Task MessageActiveDMs(string msg)
        {
            SocketUser log = Client.GetUser(174426714120781824);
            IDMChannel dm = await log.GetOrCreateDMChannelAsync();
            await dm.SendMessageAsync(msg);
            Console.WriteLine(msg);

            Storage storage = GetStorage;
            foreach (Channel channel in storage.channels.Values)
            {
                if (channel.ActiveEvent != null && (DateTimeOffset.Now - channel.ActiveEvent.Idle).TotalMinutes < 10)
                {
                    SocketUser user = Client.GetUser(dm.Id);
                    SocketChannel socketChannel = Client.GetChannel(channel.Id);
                    string mention = user.Mention;
                    (socketChannel as SocketTextChannel)?.SendMessageAsync(mention + "\n" + msg);
                }
            }
        }

        // Save and exit
        public static void Exit()
        {
            Console.WriteLine("Saving");
            SaveBackup();
            Program.exit = true;
            Console.WriteLine("Shutting down");
            Environment.Exit(0);

        }

        // Prepare for update
        public static void Update()
        {
#pragma warning disable 4014
            MessageActiveDMs("I will be taken offline in 15 minutes. " + Emotes.Wut + " The state of your active event will be saved, but it may take a few minutes before I come back. ");
            Thread.Sleep(600000);
            MessageActiveDMs("I will be taken offline in 5 minutes.");
            Thread.Sleep(240000);
            MessageActiveDMs("I will be taken offline in 1 minute.");
            Thread.Sleep(60000);
            MessageActiveDMs("Going offline, see you soon! " + Emotes.Heart);
#pragma warning restore 4014
            Exit();
        }

        public static void Save()
        {
            Console.WriteLine("Saving Data.dat");
            Storage storage = GetStorage;
            using (Stream stream = File.Open("Data.dat", FileMode.Create))
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, storage);
            }
        }

        public static void Save(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Saving Data.dat");
            Storage storage = GetStorage;
            using (Stream stream = File.Open("Data.dat", FileMode.Create))
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, storage);
            }
        }

        public static void Backup()
        {
            Console.WriteLine("Saving backup");
            Directory.CreateDirectory("Backup");
            using (Stream stream = File.Open($"Backup/Data-{Utilities.GetDateTime()}.dat", FileMode.Create))
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, _storage);
            }

        }

        public static void Backup(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Saving backup");
            Directory.CreateDirectory("Backup");
            using (Stream stream = File.Open($"Backup/Data-{Utilities.GetDateTime()}.dat", FileMode.Create))
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, _storage);
            }

        }

        private static void SaveBackup()
        {
            Save();
            Backup();
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
                var binaryFormatter = new BinaryFormatter();
                _storage = (Storage)binaryFormatter.Deserialize(stream);
            }
        }

        // Cleanup
        public static void OnCleanup(Object source, ElapsedEventArgs e)
        {
            List<ulong> remChannel = new List<ulong>();
            Dictionary<Channel, string> remEvent = new Dictionary<Channel, string>();

            foreach (KeyValuePair<ulong, Channel> channel in _storage.channels)
            {
                foreach (KeyValuePair<String, Event> eventPair in channel.Value.events)
                {
                    if ((DateTime.Now - eventPair.Value.Idle).TotalDays > Cleanup.eventTimeout)
                    {
                        if (channel.Value.events.Count == 1)
                        {
                            remChannel.Add(channel.Key);
                        }
                        else
                        {
                            remEvent.Add(channel.Value, eventPair.Key);
                        }
                    }
                }
            }

            foreach (ulong channel in remChannel)
            {
                _storage.channels.Remove(channel);
            }

            foreach (KeyValuePair<Channel, string> curEvent in remEvent)
            {
                curEvent.Key.events.Remove(curEvent.Value);
            }
        }

        // Reconnect
        public static async Task OnReconnect()
        {
            await MessageActiveDMs("Beep Boop!\nI'm back! " + Emotes.Happy);
        }
    }

    [Serializable]
    internal class User
    {
        public ulong Id { get; }
        public string Tag { get; }
        public string Username { get; }
        public string Discriminator { get; }
        private readonly HashSet<Character> _characters = new HashSet<Character>();
        public readonly Dictionary<ulong, Event> ActiveEvents = new Dictionary<ulong, Event>();
        public Event CurrentEvent;
        private int _eventId;

        public User(SocketUser user)
        {
            Id = user.Id;
            Username = user.Username;
            Discriminator = user.Discriminator;
            Tag = "@" + Username + "#" + Discriminator;
            CurrentEvent = null;
            Console.WriteLine("User created: " + user.Username);
        }

        public int GetEventId()
        {
            return _eventId++;
        }

        public Character GetCharacter(string alias)
        {
            alias = alias.ToLower();
            foreach (Character character in _characters)
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
            if (!ActiveEvents.TryAdd(channelId, curEvent))
            {
                ActiveEvents[channelId] = curEvent;
            }
        }
    }

    [Serializable]
    internal class Guild
    {
        private ulong Id { get; }
        private string Name { get; }
        private DateTimeOffset Idle { get; set; }
        private readonly HashSet<User> _admins = new HashSet<User>();
        private readonly HashSet<ulong> _adminRoles = new HashSet<ulong>();
        private Dictionary<ulong, Channel> _channels = new Dictionary<ulong, Channel>();

        public Guild(SocketGuild guild)
        {
            Id = guild.Id;
            Name = guild.Name;
            Console.WriteLine("Guild created: " + guild.Name);
        }

        // Admins
        public void AddAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            _admins.Add(user);
        }

        public void RemoveAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            _admins.Remove(user);
        }

        public string AddAdminrole(IRole role)
        {
            Idle = DateTimeOffset.Now;
            _adminRoles.Add(role.Id);
            return $"{role.Name} is now an admin role.";
        }

        public string RemoveAdminrole(IRole role)
        {
            Idle = DateTimeOffset.Now;
            _adminRoles.Remove(role.Id);
            return $"{role.Name} is no longer an admin role.";
        }

        public bool IsAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            SocketGuildUser sockUser = Data.Client.GetGuild(Id).GetUser(user.Id);

            return _admins.Contains(user) || sockUser.GuildPermissions.Administrator || ((IGuildUser)sockUser).RoleIds.Intersect(_adminRoles).Any(); ;
        }

        public string GetStatus()
        {
            string status = $"-------------- {Name} --------------";

            status += "\nAdministrators:";
            foreach (User admin in _admins)
            {
                status += "\n" + admin.Username;
            }

            if (_adminRoles.Any())
            {
                SocketGuild guild = Data.Client.GetGuild(Id);
                status += "\n\nAdmin roles:";
                foreach (ulong role in _adminRoles)
                {
                    status += "\n" + guild.GetRole(role).Name;
                }
            }

            if (_channels.Any())
            {
                status += "\n\nChannels:";
                foreach (Channel channel in _channels.Values)
                {
                    if (channel.events.Any())
                    {
                        status += "\n - " + channel.Name + ":";
                        foreach (Event curEvent in channel.events.Values)
                        {
                            status += "\n" + curEvent.Name;
                        }
                    }
                    else
                    {
                        status += "\n - " + channel.Name;
                    }
                }
            }

            return status;
        }

    }

    [Serializable]
    internal class Channel
    {
        public ulong Id { get; }
        public ulong Guild { get; }
        public string Name { get; }
        public DateTimeOffset Idle { get; set; }
        protected HashSet<User> admins = new HashSet<User>();
        public Dictionary<String, Event> events = new Dictionary<string, Event>();
        public Event ActiveEvent { get; set; }

        public Channel(SocketChannel channel)
        {
            Id = channel.Id;

            if (channel is SocketGuildChannel)
            {
                Name = (channel as SocketGuildChannel).Name;
                Guild = (channel as SocketGuildChannel).Guild.Id;
            }
            else if (channel is SocketDMChannel)
            {
                Name = (channel as SocketDMChannel).Recipient.Username;
            }
            else if (channel is SocketGroupChannel)
            {
                Name = (channel as SocketGroupChannel).Name;
            }

            Console.WriteLine("Channel created: " + Name);
        }

        public string GetEventName(User dm)
        {
            return dm.Username + "#" + dm.GetEventId();
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
            if (Guild != 0)
            {
                Guild guild = Data.GetGuild(Guild);
                return admins.Contains(user) || guild.IsAdmin(user);
            }
            return admins.Contains(user);
        }

        //

        public string GetStatus()
        {
            string status = $"-------------- {Name} --------------";
            if (ActiveEvent != null)
            {
                status += $"\nActive event: {ActiveEvent.Name} ({ActiveEvent.Id})\n";
            }
            status += "\nEvents:";
            foreach (KeyValuePair<string, Event> curEvent in events)
            {
                status += $"\n{curEvent.Value.Name} ({curEvent.Key})";
            }


            return status;
        }

        public string RemoveActive()
        {
            if (ActiveEvent == null)
            {
                return "There was no active event.";
            }

            string name = ActiveEvent.Name + " (" + ActiveEvent.Id + ")";
            ActiveEvent = null;
            return name + " is no longer an active event.";
        }
    }

    [Serializable]
    internal class GuildChannel : Channel
    {
        public Guild Guild { get; }

        public GuildChannel(SocketGuildChannel channel) : base(channel)
        {
            Guild = Data.GetGuild(channel.Guild);
        }

        public override bool IsAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            return admins.Contains(user) || Guild.IsAdmin(user) || user.Id == 174426714120781824;
        }
    }

    // Teams
    [Serializable]
    internal class Team
    {
        public string Name { get; }
        public HashSet<string> aliases = new HashSet<string>();
        public List<Character> members = new List<Character>();

        public Team(string name)
        {
            Name = name;
            aliases.Add(name.ToLower());
            if (name.Contains(" "))
            {
                aliases.Add(name.Substring(0, name.IndexOf(" ")).ToLower());
            }
        }

        public void Hide()
        {
            foreach (Character member in members)
            {
                member.Hidden = true;
            }
        }

        public void Reveal()
        {
            foreach (Character member in members)
            {
                member.Hidden = false;
            }
        }

        public bool IsHidden()
        {
            return members.All(member => member.Hidden);
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
                if (!member.Hidden) status += "\n" + member.GetStatus();
            }

            return status;
        }
    }

    // Characters
    [Serializable]
    internal class Character
    {
        public string Name { get; }
        public HashSet<string> aliases = new HashSet<string>();
        public bool Npc { get; set; }
        public bool Hidden { get; set; }
        public bool Melee { get; set; }
        public bool Ranged { get; set; }
        public readonly List<User> Admins = new List<User>();
        public readonly List<Ward> Wards = new List<Ward>();
        public int Blocking { get; set; }
        public int Health { get; set; }
        public int Maxhealth { get; set; }
        public int Actionpoints { get; set; }
        public int HealthPotions { get; set; }

        public Character(string name, int health, int maxhealth, User admin)
        {
            Name = name;
            aliases.Add(name.Contains(" ")
                ? name.Substring(0, name.IndexOf(" ", StringComparison.Ordinal)).ToLower()
                : name.ToLower());
            Health = health;
            Maxhealth = maxhealth;
            Admins.Add(admin);
            Melee = true;
        }

        private Character(string name, HashSet<string> aliases, bool npc, bool melee, bool ranged, List<User> admin, int health, int maxhealth)
        {
            Name = name;
            this.aliases = new HashSet<string>(aliases);
            Npc = npc;
            Melee = melee;
            Ranged = ranged;
            Health = health;
            Maxhealth = maxhealth;
            Admins = new List<User>(admin);
        }

        public Character CopyOf()
        {
            return new Character(Name, aliases, Npc, Melee, Ranged, Admins, Health, Maxhealth);
        }

        public void TakeDamage(int amount)
        {
            List<Ward> remove = new List<Ward>();
            foreach (Ward ward in Wards)
            {
                amount = ward.TakeDamage(amount);
                if (ward.Shield <= 0) remove.Add(ward);
            }

            foreach (Ward ward in remove)
            {
                Wards.Remove(ward);
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
            Wards.Add(ward);
        }

        public void Action(int points)
        {
            Actionpoints -= points;
        }

        public string GetStatus()
        {
            string status = Name + ": ";

            for (int i = 0; i < Health; i++)
            {
                status += "⛊";
            }

            for (int i = 0; i < Maxhealth - Health; i++)
            {
                status += "⛉";
            }

            for (int i = 0; i < GetWards(); i++)
            {
                status += "⛨";
            }

            if (HealthPotions > 0)
            {
                status += $" {HealthPotions}x{Emotes.HealthPotion}";
            }

            status += $" - {Actionpoints} AP";

            if (Melee)
            {
                status += " " + Emotes.melee;
            }

            if (Ranged)
                status += " " + Emotes.Ranged;

            if (Blocking > 0)
            {
                status += " **Blocking**";
            }

            return status;
        }

        public int GetWards()
        {
            int shields = 0;

            foreach (Ward ward in Wards)
            {
                shields += ward.Shield;
            }

            return shields;
        }

        public void UpdateWards()
        {
            List<Ward> remove = new List<Ward>();
            foreach (Ward ward in Wards)
            {
                if (ward.Duration <= 0) remove.Add(ward);
                ward.NextRound();
            }

            foreach (Ward ward in remove)
            {
                Wards.Remove(ward);
            }

            if (Blocking > 0)
            {
                Blocking--;
            }
        }

        public bool IsAdmin(User user)
        {
            return Admins.Contains(user);
        }
    }

    // Wards
    [Serializable]
    internal class Ward
    {
        public int Shield { get; internal set; }
        public int Duration { get; internal set; }

        public Ward(int shield, int duration)
        {
            Shield = shield;
            Duration = duration;
        }

        public int TakeDamage(int amount)
        {
            Shield -= amount;
            if (Shield < 0)
            {
                return Math.Abs(Shield);
            }
            return 0;
        }

        public int NextRound()
        {
            return Duration--;
        }
    }

    // DoTs
    [Serializable]
    internal class Dot
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
