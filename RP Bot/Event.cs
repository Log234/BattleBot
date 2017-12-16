using System;
using System.Collections.Generic;
using System.IO;
using Discord.WebSocket;

namespace BattleBot
{
    [Serializable]
    class Event
    {
        public String Id { get; }
        public String Name { get; private set; }
        public Channel Channel { get; }
        public DateTimeOffset Idle { get; private set; }
        public Ruleset Ruleset { get; set; }
        public User Dm { get; }
        private readonly HashSet<ulong> _admins = new HashSet<ulong>();
        private readonly List<Team> _teams = new List<Team>();
        public readonly HashSet<Character> Characters = new HashSet<Character>();
        public readonly HashSet<ulong> Users = new HashSet<ulong>();

        private readonly Dictionary<string, int> _duplicates = new Dictionary<string, int>();
        private int Round { get; set; }
        private bool Done { get; set; }
        private string _log;

        public Event(User dm, Channel channel, string name)
        {
            Id = channel.GetEventName(dm);
            Name = name;
            Channel = channel;
            Idle = DateTimeOffset.Now;
            Ruleset = new AmentiaRuleset();
            Dm = dm;
            Round = 0;
            Done = false;
            _log = "";

            channel.events[Id] = this;
            if (dm == null)
            {
                Console.WriteLine("DM Null");
            }
            if (dm.ActiveEvents == null)
            {
                Console.WriteLine("ActiveEvents Null");
            }

            dm.SetActiveEvent(channel.Id, this);
            Users.Add(dm.Id);

            Console.WriteLine("Created event: " + Name + " (" + Id + ")");
        }

        // Admins
        public string AddAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            if (_admins.Contains(user.Id))
            {
                return Log($"{user.Tag} is already an administrator of {Name}.");
            }

            _admins.Add(user.Id);

            if (!Users.Contains(user.Id))
            {
                Users.Add(user.Id);
            }

            return Log($"{user.Tag} has been added as administrator of {Name}.");
        }

        public string RemoveAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            if (!_admins.Contains(user.Id))
            {
                return Log($"{user.Tag} is not an administrator of {Name}.");
            }

            _admins.Remove(user.Id);
            return Log($"{user.Tag} is nolonger administrator of {Name}.");
        }

        public bool IsAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            return _admins.Contains(user.Id) || Dm.Equals(user) || Channel.IsAdmin(user);
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
                    foreach (ulong curUser in Users)
                    {
                        SocketUser socketUser = Data.Client.GetUser(Data.GetUser(curUser).Id);
                        msg += socketUser.Mention + " ";
                    }

                    return Log(msg);
                }
                return Log($"There is already an event running in this channel: {Channel.ActiveEvent.Name} ({Channel.ActiveEvent.Id}).\nDM: {Channel.ActiveEvent.Dm.Username}");
            }
            return Log("You do not have permission to start that event.");
        }

        public string Status()
        {
            string status = $"-------------- {Name} ({Id}) --------------";
            status += $"\nStarted: {Round > 0}";
            status += "\nDM: " + Dm.Username;
            status += "\nAdmins:";
            foreach (ulong userId in _admins)
            {
                User user = Data.GetUser(userId);
                status += "\n" + user.Username;
            }
            status += "\n\nUsers:";
            foreach (ulong userId in Users)
            {
                User user = Data.GetUser(userId);
                status += "\n" + user.Username;
            }

            status += "\n\nCharacters:" + RoundStatus();

            return status;
        }

        public string Join(User user)
        {
            Idle = DateTimeOffset.Now;

            if (!Users.Contains(user.Id))
            {
                Users.Add(user.Id);
                user.SetActiveEvent(Channel.Id, this);

                return Log($"{user.Username} has joined {Name}.");
            }
            return Log("You have already joined this event.");
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

            foreach (Character curCharacter in Characters)
            {
                if (curCharacter.aliases.Overlaps(joiner.aliases))
                {
                    HashSet<string> overlap = new HashSet<string>(joiner.aliases);
                    overlap.IntersectWith(curCharacter.aliases);
                    foreach (string alias in overlap)
                    {
                        joiner.aliases.Remove(alias);

                        if (_duplicates.ContainsKey(alias))
                        {
                            joiner.aliases.Add(alias + "#" + _duplicates[alias]++);
                        } else
                        {
                            _duplicates.Add(alias, 3);
                            joiner.aliases.Add(alias + "#2");
                        }
                    }
                }
            }

            if (_teams.Count > 0)
            {
                bool found = false;
                foreach (Team curTeam in _teams)
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
                    _teams.Add(unassigned);
                }

            }

            string result = Ruleset.AddCharacter(joiner);

            Characters.Add(joiner);

            string newAlias = "";
            foreach (string curAlias in joiner.aliases)
            {
                newAlias = curAlias;
                break;
            }
            if (result == "")
            {
                result += $"{joiner.Name} ({newAlias}) was added to {Name}.";
            }
            else
            {
                result += $"\n{joiner.Name} ({newAlias}) was added to {Name}.";
            }

            return Log(result);
        }

        public string RemoveCharacter(Character character)
        {
            Idle = DateTimeOffset.Now;

            bool done = false;
            foreach (Team team in _teams)
            {
                foreach (Character curChar in team.members)
                {
                    if (curChar.Name.Equals(character.Name))
                    {
                        team.members.Remove(curChar);
                        if (team.members.Count == 0) _teams.Remove(team);
                        done = true;
                        break;
                    }
                }
                if (done) break;
            }

            foreach (Character curChar in Characters)
            {
                if (curChar.Name.Equals(character.Name))
                {
                    Characters.Remove(curChar);
                    return Log($"{character.Name} was removed from {Name}.");
                }
            }

            return Log($"Could not find that character. {Emotes.Confused}");
        }

        public string AddTeam(string name)
        {
            Idle = DateTimeOffset.Now;

            if (_teams.Count == 0 && Characters.Count > 0)
            {
                Team unassigned = new Team("Unassigned");
                foreach (Character character in Characters)
                {
                    unassigned.members.Add(character);
                }
                _teams.Add(unassigned);
            }

            Team team = new Team(name);
            _teams.Add(team);

            return Log($"The team {name} was added to {Name}.");
        }

        public string RemoveTeam(Team team)
        {
            Idle = DateTimeOffset.Now;

            Team unassigned = null;

            if (_teams.Count > 1)
            {
                foreach (Team curTeam in _teams)
                {
                    if (curTeam.Name.Equals("Unassigned"))
                    {
                        unassigned = curTeam;
                    }
                }

                if (unassigned == null)
                {
                    unassigned = new Team("Unassigned");
                    _teams.Add(unassigned);

                } else if (_teams.Count < 2)
                {
                    _teams.Remove(unassigned);
                }
            }

            foreach (Team curTeam in _teams)
            {
                if (curTeam.Name.Equals(team.Name))
                {
                    if (unassigned != null)
                    {
                        foreach (Character curChar in curTeam.members)
                        {
                            unassigned.members.Add(curChar);
                        }
                    }

                    _teams.Remove(curTeam);
                    return Log($"{team.Name} was removed from {Name}.");
                }
            }

            return Log($"Could not find that team.");
        }

        public string SetTeam(Character character, Team team)
        {
            Idle = DateTimeOffset.Now;
            string result = "";

            foreach (Team curTeam in _teams)
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

        public string SetAp(Character character, int ap)
        {
            Idle = DateTimeOffset.Now;
            character.Actionpoints = ap;

            return Log($"{character.Name} has now {ap} action points.");
        }

        public string SetWard(Character character, int health, int duration)
        {
            Idle = DateTimeOffset.Now;
            character.Ward(new Ward(health, duration));

            return Log($"{character.Name} has now a {health} health, {duration} rounds ward.");
        }

        public string Attack(Character character, Character[] targets)
        {
            Idle = DateTimeOffset.Now;
            string result = Ruleset.Attack(character, targets);

            return Log(CheckRound(result));
        }

        public string Ranged(Character character, Character[] targets)
        {
            Idle = DateTimeOffset.Now;
            string result = Ruleset.Ranged(character, targets);

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

        public string Block(Character character)
        {
            Idle = DateTimeOffset.Now;
            string result = Ruleset.Block(character);

            return Log(CheckRound(result));
        }

        public string PotionHealth(Character character)
        {
            Idle = DateTimeOffset.Now;
            string result = Ruleset.PotionHealth(character);

            return Log(CheckRound(result));
        }

        public string Pass(Character character)
        {
            Idle = DateTimeOffset.Now;
            character.Actionpoints = 0;

            return Log(CheckRound($"{character.Name} passed the turn."));
        }

        // Utilities

        public bool HasStarted()
        {
            return Round > 0;
        }

        public string Log(string msg)
        {
            _log += msg + "\n";
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

            if (RemainingAp() != 0)
                return result;

            foreach (Character character in Characters)
            {
                character.UpdateWards();
                if (character.Health > 0) character.Actionpoints = Ruleset.Actionpoints;
            }

            string status = result + $"\n\n\n-------------- Round {Round++} is done --------------" + RoundStatus();

            return status;
        }

        public int RemainingAp()
        {
            int ap = 0;
            foreach (Character character in Characters)
            {
                if (!character.Npc) ap += character.Actionpoints;
            }

            return ap;
        }

        public bool CheckVictoryCondition(string result, out string sum)
        {
            if (_teams.Count > 0)
            {
                List<Team> remaining = new List<Team>();

                foreach (Team team in _teams)
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

            if (_teams.Count > 0)
            {
                foreach (Team team in _teams)
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

            foreach (Character character in Characters)
            {
                if (!character.Npc) status += "\n" + character.GetStatus();
            }

            status += "\n\nNPCs:";

            foreach (Character character in Characters)
            {
                if (character.Npc) status += "\n" + character.GetStatus();
            }

            return status;
        }

        public string Finish()
        {
            string path = Id + ".txt";
            File.WriteAllText(path, _log);
            if (!Channel.events.ContainsKey(Id))
            {
                Console.WriteLine("Missing: " + Id);

                foreach (string key in Channel.events.Keys)
                {
                    Console.WriteLine(key);
                }
            }
            Channel.events.Remove(Id);
            Channel.ActiveEvent = null;

            foreach (ulong curUser in Users)
            {
                User user = Data.GetUser(curUser);
                if (user != null && user.ActiveEvents.TryGetValue(Channel.Id, out Event curEvent) && curEvent.Id == Id)
                {
                    user.ActiveEvents.Remove(Channel.Id);
                }
            }

            return path;
        }

        public Character GetCharacter(string alias)
        {
            alias = alias.ToLower();
            foreach (Character character in Characters)
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
            foreach (Team team in _teams)
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
