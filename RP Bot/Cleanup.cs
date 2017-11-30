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
        public static int eventTimeout;

        public Cleanup()
        {
            eventTimeout = 99;

            timer = new Timer();
            timer.Interval = 86400000;
            timer.AutoReset = true;
            timer.Elapsed += Data.OnCleanup;
            timer.Start();
        }
    }
}
