using System;

namespace BattleBot
{
    internal class Utilities
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
                result += " and " + characters[characters.Length - 1].Name;
            }
            else
            {
                result += characters[0].Name;
            }

            return result;
        }

        // Returns the current date-time formated for filenames
        public static string GetDateTime()
        {
            string datetime = "";
            datetime += DateTime.Now.Year + "-";
            datetime += DateTime.Now.Month + "-";
            datetime += DateTime.Now.Day + " ";
            datetime += DateTime.Now.Hour + "-";
            datetime += DateTime.Now.Minute;

            return datetime;
        }

        public static string[] SplitList(string list)
        {
            if (list.Contains(","))
            {
                string newList = list.Replace(" and ", ", ");
                return newList.Split(", ");
            }
            else
            {
                return list.Split(" ");
            }
        }

        public static string CreateList(string[] list)
        {
            string strList = "";
            for (int i = 0; i < list.Length; i++)
            {
                if (i != list.Length - 2)
                {
                    strList += list[i] + ", ";
                }
                else
                {
                    strList += list[i] + " and ";
                }
            }

            strList = strList.Substring(0, strList.Length - 2);
            return strList;
        }
    }
}
