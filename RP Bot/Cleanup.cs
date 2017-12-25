using System.Timers;

namespace BattleBot
{
    internal class Cleanup
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
