using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleBot
{
    [Serializable]
    class Event
    {
        public String Id { get; }
        public String Name { get; internal set; }
        public Channel Channel { get; }
        public DateTimeOffset Created { get; }
        public DateTimeOffset Idle { get; set; }
        public Ruleset Ruleset { get; set; }
        public User Dm { get; }
        private HashSet<ulong> admins = new HashSet<ulong>();
        private List<Team> teams = new List<Team>();
        public HashSet<Character> characters = new HashSet<Character>();
        public HashSet<ulong> users = new HashSet<ulong>();

        private Dictionary<string, int> duplicates = new Dictionary<string, int>();
        public int Round { get; internal set; }
        public bool Done { get; internal set; }
        private string log;

        public Event(User dm, Channel channel, string name)
        {
            Id = channel.GetEventName(dm);
            Name = name;
            Channel = channel;
            Created = DateTimeOffset.Now;
            Idle = DateTimeOffset.Now;
            Ruleset = new AmentiaRuleset();
            Dm = dm;
            Round = 0;
            Done = false;
            log = "";

            channel.events[Id] = this;
            dm.activeEvents.Add(channel.Id, this);
            users.Add(dm.Id);

            Console.WriteLine("Created event: " + Name + " (" + Id + ")");
        }

        // Admins
        public string AddAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            if (admins.Contains(user.Id))
            {
                return Log($"{user.Tag} is already an administrator of {Name}.");
            }

            admins.Add(user.Id);

            if (!users.Contains(user.Id))
            {
                users.Add(user.Id);
            }

            return Log($"{user.Tag} has been added as administrator of {Name}.");
        }

        public string RemoveAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            if (!admins.Contains(user.Id))
            {
                return Log($"{user.Tag} is not an administrator of {Name}.");
            }

            admins.Remove(user.Id);
            return Log($"{user.Tag} is nolonger administrator of {Name}.");
        }

        public bool IsAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            return admins.Contains(user.Id) || Dm.Equals(user) || Channel.IsAdmin(user);
        }

        // Commands
        public string Start(User user)
        {
            Idle = DateTimeOffset.Now;

            if (IsAdmin(user))
            {
                if (Done)
                {
                    return Log("This event is already done.");
                }

                if (Channel.ActiveEvent == null)
                {
                    Round = 1;
                    Channel.ActiveEvent = this;
                    string msg = $"The event **{Name}** started!\n";
                    foreach (ulong curUser in users)
                    {
                        SocketUser socketUser = Data.client.GetUser(Data.GetUser(curUser).Id);
                        msg += socketUser.Mention + " ";
                    }

                    return Log(msg);
                }
                else
                {
                    return Log($"There is already an event running in this channel: {Channel.ActiveEvent.Name} ({Channel.ActiveEvent.Id}).\nDM: {Channel.ActiveEvent.Dm.Username}");
                }
            }
            else
            {
                return Log("You do not have permission to start that event.");
            }
        }

        public string Status()
        {
            string status = $"-------------- {Name} ({Id}) --------------";
            status += $"\nStarted: {Round > 0}";
            status += "\nDM: " + Dm.Username;
            status += "\nAdmins:";
            foreach (ulong userID in admins)
            {
                User user = Data.GetUser(userID);
                status += "\n" + user.Username;
            }
            status += "\n\nUsers:";
            foreach (ulong userID in users)
            {
                User user = Data.GetUser(userID);
                status += "\n" + user.Username;
            }

            status += "\n\nCharacters:" + RoundStatus();

            return status;
        }

        public string Join(User user)
        {
            Idle = DateTimeOffset.Now;

            if (!users.Contains(user.Id))
            {
                users.Add(user.Id);
                user.SetActiveEvent(Channel.Id, this);

                return Log($"{user.Username} has joined {Name}.");
            }
            else
            {
                return Log("You have already joined this event.");
            }
        }

        public string Join(Character character, User user)
        {
            Idle = DateTimeOffset.Now;
            if (HasStarted() && !Ruleset.JoinAfterStart && !IsAdmin(user))
            {
                return Log("The event has already started, so you cannot join.");
            }

            AddCharacter(character, user);
            Join(user);

            return Log($"{user.Username} has joined {Name}, with {character.Name}!");
        }

        public string AddCharacter(Character character, User user)
        {
            Idle = DateTimeOffset.Now;
            if (HasStarted() && !Ruleset.JoinAfterStart && !IsAdmin(user))
            {
                return Log("The event has already started, so you cannot add a new character. " + Emotes.Annoyed);
            }

            Character joiner = character.CopyOf();

            if (joiner.Npc)
            {
                joiner.Maxhealth = Ruleset.NpcMaxhealth;
                joiner.Health = Ruleset.NpcMaxhealth;
            }
            else
            {
                joiner.Maxhealth = Ruleset.CharMaxhealth;
                joiner.Health = Ruleset.CharMaxhealth;
            }

            if (HasStarted() && Ruleset.WaitOnJoin)
                joiner.Actionpoints = 0;
            else
                joiner.Actionpoints = Ruleset.Actionpoints;

            foreach (Character curCharacter in characters)
            {
                if (curCharacter.aliases.Overlaps(joiner.aliases))
                {
                    HashSet<string> overlap = new HashSet<string>(joiner.aliases);
                    overlap.IntersectWith(curCharacter.aliases);
                    foreach (string alias in overlap)
                    {
                        joiner.aliases.Remove(alias);

                        if (duplicates.ContainsKey(alias))
                        {
                            joiner.aliases.Add(alias + "#" + duplicates[alias]++);
                        } else
                        {
                            duplicates.Add(alias, 3);
                            joiner.aliases.Add(alias + "#2");
                        }
                    }
                }
            }

            if (teams.Count > 0)
            {
                bool found = false;
                foreach (Team curTeam in teams)
                {
                    if (curTeam.Name.Equals("Unassigned"))
                    {
                        curTeam.members.Add(joiner);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Team unassigned = new Team("Unassigned");
                    unassigned.members.Add(joiner);
                    teams.Add(unassigned);
                }

            }

            characters.Add(joiner);

            string newAlias = "";
            foreach (string curAlias in joiner.aliases)
            {
                newAlias = curAlias;
                break;
            }

            return Log($"{joiner.Name} ({newAlias}) was added to {Name}.");
        }

        public string RemoveCharacter(Character character)
        {
            Idle = DateTimeOffset.Now;

            bool done = false;
            foreach (Team team in teams)
            {
                foreach (Character curChar in team.members)
                {
                    if (curChar.Name.Equals(character.Name))
                    {
                        team.members.Remove(curChar);
                        if (team.members.Count == 0) teams.Remove(team);
                        done = true;
                        break;
                    }
                }
                if (done) break;
            }

            foreach (Character curChar in characters)
            {
                if (curChar.Name.Equals(character.Name))
                {
                    characters.Remove(curChar);
                    return Log($"{character.Name} was removed from {Name}.");
                }
            }

            return Log($"Could not find that character. {Emotes.Confused}");
        }

        public string AddTeam(string name)
        {
            Idle = DateTimeOffset.Now;

            if (teams.Count == 0 && characters.Count > 0)
            {
                Team unassigned = new Team("Unassigned");
                foreach (Character character in characters)
                {
                    unassigned.members.Add(character);
                }
                teams.Add(unassigned);
            }

            Team team = new Team(name);
            teams.Add(team);

            return Log($"The team {name} was added to {Name}.");
        }

        public string RemoveTeam(Team team)
        {
            Idle = DateTimeOffset.Now;

            Team unassigned = null;

            if (teams.Count > 1)
            {
                foreach (Team curTeam in teams)
                {
                    if (curTeam.Name.Equals("Unassigned"))
                    {
                        unassigned = curTeam;
                    }
                }

                if (unassigned == null)
                {
                    unassigned = new Team("Unassigned");
                    teams.Add(unassigned);

                } else if (teams.Count > 2)
                {
                    teams.Remove(unassigned);
                }
            }

            foreach (Team curTeam in teams)
            {
                if (curTeam.Name.Equals(team.Name))
                {
                    foreach (Character curChar in curTeam.members)
                    {
                        unassigned.members.Add(curChar);
                    }

                    teams.Remove(curTeam);
                    return Log($"{team.Name} was removed from {Name}.");
                }
            }

            return Log($"Could not find that team.");
        }

        public string SetTeam(Character character, Team team)
        {
            Idle = DateTimeOffset.Now;
            string result = "";

            foreach (Team curTeam in teams)
            {
                if (curTeam.members.Contains(character))
                {
                    result += $"{character.Name} was removed from {curTeam.Name}.\n";
                    curTeam.members.Remove(character);
                }
            }

            result += $"{character.Name} was added to {team.Name}.\n";
            team.members.Add(character);

            return Log(result);
        }

        public string SetMaxhealth(Character character, int health)
        {
            Idle = DateTimeOffset.Now;
            character.Maxhealth = health;

            return Log($"{character.Name}'s max health is now {health}.");
        }

        public string SetHealth(Character character, int health)
        {
            Idle = DateTimeOffset.Now;
            character.Health = health;

            return Log($"{character.Name}'s health is now {health}.");
        }

        public string Attack(Character character, Character[] targets)
        {
            Idle = DateTimeOffset.Now;
            string result = Ruleset.Attack(character, targets);

            return Log(CheckRound(result));
        }

        public string Heal(Character character, Character[] targets)
        {
            Idle = DateTimeOffset.Now;
            string result = Ruleset.Heal(character, targets);

            return Log(CheckRound(result));
        }

        public string Ward(Character character, Character[] targets)
        {
            Idle = DateTimeOffset.Now;
            string result = Ruleset.Ward(character, targets);

            return Log(CheckRound(result));
        }

        // Utilities

        public bool HasStarted()
        {
            return Round > 0;
        }

        public string Log(string msg)
        {
            log += msg + "\n";
            Console.WriteLine(msg);
            return msg;
        }

        public string CheckRound(string result)
        {

            if (CheckVictoryCondition(result, out string sum))
            {
                Channel.ActiveEvent = null;
                Done = true;
                return sum;
            }

            if (RemainingAP() != 0)
                return result;

            foreach (Character character in characters)
            {
                character.UpdateWards();
                if (character.Health > 0) character.Actionpoints = Ruleset.Actionpoints;
            }

            string status = result + $"\n\n\n-------------- Round {Round++} is done --------------" + RoundStatus();

            return status;
        }

        public int RemainingAP()
        {
            int ap = 0;
            foreach (Character character in characters)
            {
                if (!character.Npc) ap += character.Actionpoints;
            }

            return ap;
        }

        public bool CheckVictoryCondition(string result, out string sum)
        {
            if (teams.Count > 0)
            {
                List<Team> remaining = new List<Team>();

                foreach (Team team in teams)
                {
                    if (!team.IsDead())
                    {
                        remaining.Add(team);
                    }
                }

                if (remaining.Count == 1)
                {
                    sum = result + $"\n\n\n-------------- {remaining[0].Name} has won! --------------";
                    return true;
                }
            }
            sum = result;
            return false;
        }

        public string RoundStatus()
        {
            string status = "";

            if (teams.Count > 0)
            {
                foreach (Team team in teams)
                {
                    if (team.members.Count > 0)
                    {
                        status += "\n" + team.GetStatus() + "\n";
                    }
                }
            }
            else
            {
                status += GetNpcStatus();
            }

            return status;
        }

        public string GetNpcStatus()
        {
            string status = "\nCharacters:";

            foreach (Character character in characters)
            {
                if (!character.Npc) status += "\n" + character.GetStatus();
            }

            status += "\n\nNPCs:";

            foreach (Character character in characters)
            {
                if (character.Npc) status += "\n" + character.GetStatus();
            }

            return status;
        }

        public string Finish()
        {
            string path = Id + ".txt";
            System.IO.File.WriteAllText(path, log);
            if (!Channel.events.ContainsKey(Id))
            {
                Console.WriteLine("Missing: " + Id);

                foreach (string key in Channel.events.Keys)
                {
                    Console.WriteLine(key);
                }
            }
            Channel.events.Remove(Id);

            foreach (ulong curUser in users)
            {
                User user = Data.GetUser(curUser);
                if (user != null && user.activeEvents.TryGetValue(Channel.Id, out Event curEvent) && curEvent.Id == Id)
                {
                    user.activeEvents.Remove(Channel.Id);
                }
            }

            return path;
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

        public Team GetTeam(string alias)
        {
            alias = alias.ToLower();
            foreach (Team team in teams)
            {
                foreach (string teamAlias in team.aliases)
                {
                    if (alias.Equals(teamAlias))
                        return team;
                }
            }

            return null;
        }

    }
}
