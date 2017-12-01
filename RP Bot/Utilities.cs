using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleBot
{
    class Utilities
    {
        // Returns all the names concatenated to one string.
        public static string AppendNames(Character[] characters)
        {
            string result = "";

            if (characters.Length > 1)
            {
                for (int i = 0; i < characters.Length - 1; i++)
                {
                    result += characters[i].Name + ", ";
                }

                result = result.Substring(0, result.Length - 2);
                result += " and " + characters[characters.Length - 1];
            }
            else
            {
                result += characters[0].Name;
            }

            return result;
        }
    }
}
