using System;
using System.Collections.Generic;
using System.Text;

namespace RP_Bot
{
    class Event
    {
        public String Id { get; }
        public Channel Channel { get; }
        public DateTimeOffset Created { get; }
        public DateTimeOffset Idle { get; set; }
        public Ruleset Ruleset { get; set; }
        public User Dm { get; }
        private HashSet<ulong> admins = new HashSet<ulong>();
        private List<Team> teams = new List<Team>();
        public HashSet<Character> characters = new HashSet<Character>();
        public HashSet<ulong> users = new HashSet<ulong>();

        public int Round { get; internal set; }
        private string log;

        public Event(User dm, Channel channel)
        {
            Id = channel.GetEventName(dm);
            Channel = channel;
            Created = DateTimeOffset.Now;
            Idle = DateTimeOffset.Now;
            Ruleset = new AmentiaRuleset();
            Dm = dm;
            Round = 0;
            log = "";

            channel.events[Id] = this;
            dm.activeEvents.Add(channel.Id, this);
            users.Add(dm.Id);

            Console.WriteLine("Created event: " + Id);
        }

        // Admins
        public string AddAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            admins.Add(user.Id);

            return Log($"{user.Tag} has been added as administrator of {Id}.");
        }

        public string RemoveAdmin(User user)
        {
            Idle = DateTimeOffset.Now;
            admins.Remove(user.Id);

            return Log($"{user.Tag} is nolonger administrator of {Id}.");
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
                if (Channel.ActiveEvent == null)
                {
                    Round = 1;
                    Channel.ActiveEvent = this;
                    string msg = $"The event **{Id}** started!\n";
                    foreach (ulong curUser in users)
                    {
                        msg += Data.GetUser(curUser).SocketUser.Mention + " ";
                    }

                    return Log(msg);
                }
                else
                {
                    return Log($"There is already an event running in this channel: {Channel.ActiveEvent.Id}.");
                }
            }
            else
            {
                return Log("You do not have permission to start that event.");
            }
        }

        public string Join(User user)
        {
            Idle = DateTimeOffset.Now;

            if (!users.Contains(user.Id))
            {
                users.Add(user.Id);
                user.SetActiveEvent(Channel.Id, this);

                return Log($"{user.Username} has joined {Id}.");
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

            return Log($"{user.Username} has joined {Id}, with {character.Name}!");
        }

        public string AddCharacter(Character character, User user)
        {
            Idle = DateTimeOffset.Now;
            if (HasStarted() && !Ruleset.JoinAfterStart && !IsAdmin(user))
            {
                return Log("The event has already started, so you cannot add a new character.");
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
            characters.Add(joiner);

            return Log($"{character.Name} was added to {Id}.");
        }

        public string RemoveCharacter(Character character)
        {
            Idle = DateTimeOffset.Now;

            foreach (Character curChar in characters)
            {
                if (curChar.Name.Equals(character.Name))
                {
                    characters.Remove(curChar);
                    return Log($"{character.Name} was removed from {Id}.");
                }
            }

            return Log($"Could not find that character.");
        }

        public string AddTeam(string name)
        {
            Idle = DateTimeOffset.Now;

            Team team = new Team(name);
            teams.Add(team);

            return Log($"The team {name} was added to {Id}.");
        }

        public string RemoveTeam(Team team)
        {
            Idle = DateTimeOffset.Now;

            foreach (Team curTeam in teams)
            {
                if (curTeam.Name.Equals(team.Name))
                {
                    teams.Remove(curTeam);
                    return Log($"{team.Name} was removed from {Id}.");
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
                return sum;
            }

            if (RemainingAP() != 0)
                return result;

            foreach (Character character in characters)
            {
                character.UpdateWards();
                if (character.Health > 0) character.Actionpoints = Ruleset.Actionpoints;
            }

            string status = result + RoundStatus();

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
            string status = $"\n\n\n-------------- Round {Round++} is done --------------";

            if (teams.Count > 0)
            {
                foreach (Team team in teams)
                {
                    status += "\n" + team.GetStatus() + "\n";
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
            Channel.events.Remove(Id);

            foreach (ulong curUser in users)
            {
                User user = Data.GetUser(curUser);
                if (user?.activeEvents[Channel.Id]?.Id == Id)
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
