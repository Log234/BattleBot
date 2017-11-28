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
    }

    class User
    {
        public ulong Id { get; }
        public SocketUser SocketUser { get; }
        public DateTimeOffset Idle { get; set; }
        private int eventID = 0;

        public User(SocketUser user)
        {
            Id = user.Id;
            SocketUser = user;
            Console.WriteLine("User created: " + user.Username);
        }

        public int getEventID()
        {
            Idle = DateTimeOffset.Now;
            return eventID++;
        }
    }

    class Guild
    {
        public ulong Id { get; }
        public SocketGuild SocketGuild { get; }
        public DateTimeOffset Idle { get; set; }
        private HashSet<SocketUser> admins = new HashSet<SocketUser>();
        private Dictionary<SocketChannel, Channel> channels = new Dictionary<SocketChannel, Channel>();

        public Guild(SocketGuild guild)
        {
            Id = guild.Id;
            SocketGuild = guild;
            Console.WriteLine("Guild created: " + SocketGuild.Name);
        }

        // Admins
        public void addAdmin(SocketUser user)
        {
            Idle = DateTimeOffset.Now;
            admins.Add(user);
        }

        public void removeAdmin(SocketUser user)
        {
            Idle = DateTimeOffset.Now;
            admins.Remove(user);
        }

        public bool isAdmin(SocketUser user)
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
        protected HashSet<SocketUser> admins = new HashSet<SocketUser>();
        public Dictionary<String, Event> events = new Dictionary<string, Event>();

        public Channel(SocketChannel channel)
        {
            Id = channel.Id;
            SocketChannel = channel;
            Console.WriteLine("Channel created: " + Id);
        }

        public string getEventName(SocketUser dm)
        {
            User user;
            if (!Data.users.TryGetValue(dm.Id, out user))
            {
                user = new User(dm);
                Data.users[dm.Id] = user;
            }

            return dm.Username + "#" + dm.Discriminator + "#" + user.getEventID();
        }

        // Admins
        public void AddAdmin(SocketUser user)
        {
            Idle = DateTimeOffset.Now;
            admins.Add(user);
        }

        public void RemoveAdmin(SocketUser user)
        {
            Idle = DateTimeOffset.Now;
            admins.Remove(user);
        }

        public virtual bool IsAdmin(SocketUser user)
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

        public override bool IsAdmin(SocketUser user)
        {
            Idle = DateTimeOffset.Now;
            return admins.Contains(user) || Guild.isAdmin(user);
        }
    }

    class Event
    {
        public String Id { get; }
        public Channel Channel { get; }
        public DateTimeOffset Created { get; }
        public DateTimeOffset Idle { get; set; }
        public SocketUser Dm { get; }
        private HashSet<SocketUser> admins = new HashSet<SocketUser>();
        HashSet<Character> characters = new HashSet<Character>();

        public Event(SocketUser dm, SocketChannel channel)
        {
            Channel curChannel;
            if (!Data.channels.TryGetValue(channel.Id, out curChannel))
            {
                if ((channel as SocketGuildChannel) == null)
                {
                    curChannel = new Channel(channel);
                } else
                {
                    curChannel = new GuildChannel((SocketGuildChannel) channel);
                }
                Data.channels[channel.Id] = curChannel;
            }

            Id = curChannel.getEventName(dm);
            Channel = curChannel;
            Created = DateTimeOffset.Now;
            Idle = DateTimeOffset.Now;
            Dm = dm;

            curChannel.events[Id] = this;

            Console.WriteLine("Created event: " + Id);
        }

        // Admins
        public void AddAdmin(SocketUser user)
        {
            Idle = DateTimeOffset.Now;
            admins.Add(user);
        }

        public void RemoveAdmin(SocketUser user)
        {
            Idle = DateTimeOffset.Now;
            admins.Remove(user);
        }

        public bool IsAdmin(SocketUser user)
        {
            Idle = DateTimeOffset.Now;
            return admins.Contains(user) || Dm.Equals(user) || Channel.IsAdmin(user);
        }

        // Commands

        public string Start(SocketUser user)
        {
            if (IsAdmin(user))
            {
                // TO-Do Do stuff
                return $"The event \"{Id}\" started!\nType \"!event join {Id}\" to join.";
            } else {
                return "You do not have permission to start that event.";
            }
        }

    }

    // Characters
    class Character
    {
        string name;
        int health;
    }
}
