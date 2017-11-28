using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace RP_Bot
{
    static class Data
    {
       static Dictionary<SocketChannel, Channel> channels = new Dictionary<SocketChannel, Channel>();
    }

    class Channel
    {
        DateTime joined;
        DateTime idle;
        Dictionary<string, Event> events = new Dictionary<string, Event>();
    }

    class Event
    {
        DateTime created;
        DateTime idle;
        SocketUser dm;
        HashSet<SocketUser> admins = new HashSet<SocketUser>();
        HashSet<Character> characters = new HashSet<Character>();
        
    }

    class Character
    {
        string name;
        int health;
    }
}
