using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace RP_Bot
{
    class Cleanup
    {
        private static Timer timer;
        private static int eventTimeout;

        public Cleanup()
        {
            eventTimeout = 99;

            timer = new Timer();
            timer.Interval = 86400000;
            timer.AutoReset = true;
            timer.Elapsed += OnCleanup;
            timer.Start();
        }

        private static void OnCleanup(Object source, ElapsedEventArgs e)
        {
            foreach (KeyValuePair<ulong, Channel> channel in Data.channels)
            {
                foreach (KeyValuePair<String, Event> eventPair in channel.Value.events)
                {
                    if ((DateTime.Now - eventPair.Value.Idle).TotalDays > eventTimeout)
                    {
                        if (channel.Value.events.Count == 1)
                        {
                            Data.channels.Remove(channel.Key);
                        }
                        else
                        {
                            channel.Value.events.Remove(eventPair.Key);
                        }
                    }
                }
            }
        }
    }
}
