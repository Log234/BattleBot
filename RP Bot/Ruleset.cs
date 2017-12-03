using System;
using System.Collections.Generic;
using System.Text;

namespace BattleBot
{
    [Serializable]
    abstract class Ruleset
    {
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public bool JoinAfterStart { get; protected set; }   // Whether characters can join after the event has started
        public bool WaitOnJoin { get; protected set; }       // Whether characters that join mid-fight have to wait a round
        public bool AoeAttacks { get; protected set; }       // Whether AoE attacks are allowed
        public bool AoeRanged { get; protected set; }        // Whether AoE ranged attacks are allowed
        public bool AoeHeals { get; protected set; }         // Whether AoE heals are allowed
        public bool AoeWards { get; protected set; }         // Whether AoE wards are allowed
        public bool Stuns { get; protected set; }            // Whether a player can be stunned
        public bool Dots { get; protected set; }             // Whether DoT effects are allowed
        public bool WardCleansesDoTs { get; protected set; } // Whether wards cleanses DoTs

        public int CharMaxhealth { get; protected set; }     // The max health of characters
        public int NpcMaxhealth { get; protected set; }      // The max health of NPCs
        public int Actionpoints { get; protected set; }      // Number of action points for characters

        public int WardHealth { get; protected set; }        // The health of a ward
        public int WardDuration { get; protected set; }      // The duration of a ward

        abstract public string Attack(Character character, Character[] targets);
        abstract public string Ranged(Character character, Character[] targets);
        abstract public string Dot(Character character, Character[] targets);
        abstract public string Stun(Character character, Character[] targets);
        abstract public string Heal(Character character, Character[] targets);
        abstract public string Ward(Character character, Character[] targets);

    }

    [Serializable]
    class AmentiaRuleset : Ruleset
    {

        public AmentiaRuleset()
        {
            Name = "Amentia's Ruleset";
            Description = "";
            JoinAfterStart = true;
            WaitOnJoin = true;
            AoeAttacks = true;
            AoeRanged = false;
            AoeHeals = true;
            AoeWards = false;
            Stuns = false;
            Dots = false;
            WardCleansesDoTs = false;

            CharMaxhealth = 3;
            NpcMaxhealth = 4;
            Actionpoints = 2;

            WardHealth = 1;
            WardDuration = 1;

        }

        public override string Attack(Character character, Character[] targets)
        {
            Random rnd = new Random();

            if (character.Actionpoints < 2)
            {
                return "Insufficient action points.";
            }
            else
            {
                character.Action(2);
            }

            string result = character.Name + " attempts to attack " + Utilities.AppendNames(targets) + ":";
            int roll;

            for (int i = 0; i < targets.Length; i++)
            {
                roll = rnd.Next(1, 10);

                if (roll == 1)
                {
                    character.TakeDamage(2);
                    result += "\n" + character.Name + " **critically failed** " + targets[i].Name + " and took **2** damage. (rolled: " + roll + ")";
                }
                else if (roll <= 5)
                {
                    character.TakeDamage(1);
                    result += "\n" + character.Name + " **missed** " + targets[i].Name + " and took **1** damage. (rolled: " + roll + ")";
                }
                else if (roll < 10)
                {
                    targets[i].TakeDamage(1);
                    result += "\n" + character.Name + " **hit** " + targets[i].Name + " and did **1** damage. (rolled: " + roll + ")";
                }
                else
                {
                    targets[i].TakeDamage(2);
                    result += "\n" + character.Name + " **critically hit** " + targets[i].Name + " and did **2** damage. (rolled: " + roll + ")";
                }
            }

            if (character.Actionpoints > 0)
                result += "\n" + character.Actionpoints + " action points remaining.";

            return result;
        }

        public override string Ranged(Character character, Character[] targets)
        {
            return "Not implemented.";
        }

        public override string Dot(Character character, Character[] targets)
        {
            return "Not implemented.";
        }

        public override string Stun(Character character, Character[] targets)
        {
            return "Not implemented.";
        }

        public override string Heal(Character character, Character[] targets)
        {
            Random rnd = new Random();

            int requiredPoints;
            if (targets.Length > 1)
            {
                requiredPoints = 2;
            }
            else
            {
                requiredPoints = 1;
            }

            if (character.Actionpoints < requiredPoints)
                return "Insufficient action points.";
            else
                character.Action(requiredPoints);

            string result = character.Name + " attempts to heal " + Utilities.AppendNames(targets) + ":";
            int roll;

            for (int i = 0; i < targets.Length; i++)
            {
                roll = rnd.Next(1, 10);

                if (roll <= 5)
                {
                    result += "\n" + character.Name + " **missed** " + targets[i].Name + ". (rolled: " + roll + ")";
                }
                else if (roll < 10)
                {
                    targets[i].Heal(1);
                    result += "\n" + character.Name + " **healed** " + targets[i].Name + " for **1** health. (rolled: " + roll + ")";
                }
                else
                {
                    targets[i].Heal(2);
                    result += "\n" + character.Name + " **critically healed** " + targets[i].Name + " for **2** health. (rolled: " + roll + ")";
                }
            }

            if (character.Actionpoints > 0)
                result += "\n" + character.Actionpoints + " action points remaining.";

            return result;
        }

        public override string Ward(Character character, Character[] targets)
        {
            Random rnd = new Random();

            int requiredPoints;
            if (targets.Length > 1)
            {
                requiredPoints = 2;
            }
            else
            {
                requiredPoints = 1;
            }

            if (character.Actionpoints < requiredPoints)
                return "Insufficient action points.";
            else
                character.Action(requiredPoints);

            string result = character.Name + " attempts to ward " + Utilities.AppendNames(targets) + ":";
            int roll;

            for (int i = 0; i < targets.Length; i++)
            {
                roll = rnd.Next(1, 10);

                if (roll <= 5)
                {
                    result += "\n" + character.Name + " **missed** " + targets[i].Name + ". (rolled: " + roll + ")";
                }
                else
                {
                    targets[i].Ward(new Ward(WardHealth, WardDuration));
                    result += "\n" + character.Name + " **warded** " + targets[i].Name + ". (rolled: " + roll + ")";
                }
            }

            if (character.Actionpoints > 0)
                result += "\n" + character.Actionpoints + " action points remaining.";

            return result;
        }
    }
}
