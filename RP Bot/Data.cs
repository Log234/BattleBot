﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BattleBot
{
    class Data
    {
        public static DiscordSocketClient Client;
        private static Storage _storage = new Storage();

        [Serializable]
        class Storage
        {
            public Dictionary<ulong, User> users = new Dictionary<ulong, User>();
            public Dictionary<ulong, Guild> guilds = new Dictionary<ulong, Guild>();
            public Dictionary<ulong, Channel> channels = new Dictionary<ulong, Channel>();
        }

        static Storage GetStorage
        {
            get { return _storage; }
        }

        public static Event GetEvent(ulong channelId, string eventId)
        {
            if (!_storage.channels.TryGetValue(channelId, out Channel channel))
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
        public static Event GetEvent(SocketCommandContext context, string eventId = "Active")
        {
            Event curEvent;
            if (eventId.Equals("Active"))
            {
                curEvent = _storage.users[context.Message.Author.Id]?.ActiveEvents[context.Channel.Id];
            }
            else
            {
                if (_storage.channels.TryGetValue(context.Channel.Id, out Channel channel) && channel.events.TryGetValue(eventId, out Event newEvent))
                {
                    curEvent = newEvent;
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
                _storage =  (Storage)binaryFormatter.Deserialize(stream);
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
    class User
    {
        public ulong Id { get; }
        public string Tag { get; }
        public string Username { get; }
        public string Discriminator { get; }
        private readonly HashSet<Character> _characters = new HashSet<Character>();
        public Dictionary<ulong, Event> ActiveEvents = new Dictionary<ulong, Event>();
        private int _eventId;

        public User(SocketUser user)
        {
            Id = user.Id;
            Username = user.Username;
            Discriminator = user.Discriminator;
            Tag = "@" + Username + "#" + Discriminator;
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
    class Guild
    {
        public DateTimeOffset Idle { get; set; }
        private HashSet<User> admins = new HashSet<User>();
        private Dictionary<ulong, Channel> _channels = new Dictionary<ulong, Channel>();

        public Guild(SocketGuild guild)
        {
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
            } else if (channel is SocketDMChannel)
            {
                Name = (channel as SocketDMChannel).Recipient.Username;
            } else if (channel is SocketGroupChannel)
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
    class GuildChannel : Channel
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
    class Team
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
        public bool Meele { get; set; }
        public bool Ranged { get; set; }
        public List<User> admins = new List<User>();
        public List<Ward> wards = new List<Ward>();
        public int Health { get; set; }
        public int Maxhealth { get; set; }
        public int Actionpoints { get; set; }
        public int HealthPotions { get; set; }

        public Character(string name, int health, int maxhealth, User admin)
        {
            Name = name;
            if (name.Contains(" "))
            {
                aliases.Add(name.Substring(0, name.IndexOf(" ", StringComparison.Ordinal)).ToLower());
            } else
            {
                aliases.Add(name.ToLower());
            }
            Health = health;
            Maxhealth = maxhealth;
            admins.Add(admin);
        }

        public Character(string name, HashSet<string> aliases, bool npc, bool meele, bool ranged, List<User> admin, int health, int maxhealth)
        {
            Name = name;
            this.aliases = new HashSet<string>(aliases);
            Npc = npc;
            Health = health;
            Maxhealth = maxhealth;
            admins = new List<User>(admin);
        }

        public Character CopyOf()
        {
            return new Character(Name, aliases, Npc, Meele, Ranged, admins, Health, Maxhealth);
        }

        public void TakeDamage(int amount)
        {
            List<Ward> remove = new List<Ward>();
            for (int i = 0; i < wards.Count; i++)
            {
                amount = wards[i].TakeDamage(amount);
                if (wards[i].Shield <= 0) remove.Add(wards[i]);
            }

            foreach (Ward ward in remove)
            {
                wards.Remove(ward);
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

            if (Meele)
            {
                status += " " + Emotes.Meele;
            }

            if (Ranged)
                status += " " + Emotes.Ranged;

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
            List<Ward> remove = new List<Ward>();
            foreach (Ward ward in wards)
            {
                if (ward.Duration <= 0) remove.Add(ward);
                ward.NextRound();
            }

            foreach (Ward ward in remove)
            {
                wards.Remove(ward);
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
