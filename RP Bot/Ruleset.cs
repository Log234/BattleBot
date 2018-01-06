using System;

namespace BattleBot
{
    [Serializable]
    internal abstract class Ruleset
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
        public int BaseDamage { get; set; }      // The base damage for attacks
        public int BaseHeal { get; set; }      // The base heal for heals

        protected int WardHealth { get; set; }        // The health of a ward
        protected int WardDuration { get; set; }      // The duration of a ward
        public int PotionHealthRollMin { get; set; }      // The minimum amount of health potions
        public int PotionHealthRollMax { get; set; }      // The maximum amount of health potions

        public abstract string AddCharacter(Character character);

        public abstract string Attack(Character character, Character[] targets);
        public abstract string Ranged(Character character, Character[] targets);
        public abstract string Dot(Character character, Character[] targets);
        public abstract string Stun(Character character, Character[] targets);
        public abstract string Heal(Character character, Character[] targets);
        public abstract string Ward(Character character, Character[] targets);
        public abstract string Block(Character character);
        public abstract string PotionHealth(Character character);

    }

    [Serializable]
    internal class AmentiaRuleset : Ruleset
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
            NpcMaxhealth = 3;
            Actionpoints = 2;
            BaseDamage = 1;
            BaseHeal = 1;

            WardHealth = 1;
            WardDuration = 1;

            PotionHealthRollMin = 1;
            PotionHealthRollMax = 6;

        }

        public override string Attack(Character character, Character[] targets)
        {
            Random rnd = new Random();

            if (character.Actionpoints < 2)
            {
                return "Insufficient action points." + Emotes.Annoyed;
            }
            character.Action(2);

            if (character.Blocking > 0)
            {
                character.Blocking = 0;
            }

            string result = character.Name + " attempts to attack " + Utilities.AppendNames(targets) + ":";

            foreach (Character curCharacter in targets)
            {
                int roll = rnd.Next(1, 10+1);
                int secondary = -1;

                if (curCharacter.Blocking > 0)
                {
                    result += "\n" + curCharacter.Name + " is blocking.";
                    curCharacter.Blocking = 0;
                    int newRoll = rnd.Next(1, 10+1);

                    if (newRoll < roll)
                    {
                        secondary = roll;
                        roll = newRoll;
                    }
                    else
                    {
                        secondary = newRoll;
                    }
                }

                if (roll == 1)
                {
                    if (curCharacter.Melee)
                    {
                        character.TakeDamage(BaseDamage * 2);
                        result +=
                            $"\n{character.Name} **critically failed** the attack on {curCharacter.Name} and was counter attacked for **{BaseDamage * 2}** damage.";
                    }
                    else
                    {
                        result +=
                            $"\n{character.Name} **critically failed** the attack on {curCharacter.Name} but {curCharacter.Name} does not have a melee attack.";
                    }
                }
                else if (roll <= 5)
                {
                    if (curCharacter.Melee)
                    {
                        character.TakeDamage(BaseDamage);
                        result += $"\n{character.Name} **missed** {curCharacter.Name} and was counter attacked for **{BaseDamage}** damage.";
                    }
                    else
                    {
                        result +=
                            $"\n{character.Name} **missed** {curCharacter.Name} but {curCharacter.Name} does not have a melee attack.";
                    }
                }
                else if (roll < 10)
                {
                    curCharacter.TakeDamage(BaseDamage);
                    result += $"\n{character.Name} **hit** {curCharacter.Name} and did **{BaseDamage}** damage.";
                }
                else
                {
                    curCharacter.TakeDamage(BaseDamage * 2);
                    result += $"\n{character.Name} **critically hit** {curCharacter.Name} and did **{BaseDamage * 2}** damage.";
                }

                if (secondary == -1)
                {
                    result += $" (rolled: {roll})";
                }
                else
                {
                    result += $" (rolled: {roll} and {secondary}";
                }

                if (character.Health <= 0)
                {
                    character.Actionpoints = 0;
                    return result + "\n" + character.Name + " is out of the battle. " + Emotes.Dead;
                }
                if (curCharacter.Health <= 0)
                {
                    curCharacter.Actionpoints = 0;
                    result += "\n" + curCharacter.Name + " is out of the battle. " + Emotes.Dead;
                }
            }

            if (character.Actionpoints > 0)
                result += "\n" + character.Actionpoints + " action points remaining.";

            return result;
        }

        public override string Ranged(Character character, Character[] targets)
        {
            Random rnd = new Random();

            if (character.Actionpoints < 2)
            {
                return "Insufficient action points." + Emotes.Annoyed;
            }
            character.Action(2);

            if (character.Blocking > 0)
            {
                character.Blocking = 0;
            }

            string result = character.Name + " attempts to ranged attack " + Utilities.AppendNames(targets) + ":";

            foreach (Character curCharacter in targets)
            {
                int roll = rnd.Next(1, 10);
                int secondary = -1;

                if (curCharacter.Blocking > 0)
                {
                    result += "\n" + curCharacter.Name + " is blocking.";
                    curCharacter.Blocking = 0;
                    int newRoll = rnd.Next(1, 10+1);

                    if (newRoll < roll)
                    {
                        secondary = roll;
                        roll = newRoll;
                    }
                    else
                    {
                        secondary = newRoll;
                    }
                }

                if (roll == 1)
                {
                    if (curCharacter.Ranged)
                    {
                        character.TakeDamage(BaseDamage * 2);
                        result +=
                            $"\n{character.Name} **critically failed** {curCharacter.Name} and was counter attacked for **{BaseDamage * 2}** damage.";
                    }
                    else
                    {
                        result +=
                            $"\n{character.Name} **critically failed** {curCharacter.Name} but {curCharacter.Name} does not have a ranged attack.";
                    }
                }
                else if (roll <= 5)
                {
                    if (curCharacter.Ranged)
                    {
                        character.TakeDamage(BaseDamage);
                        result += $"\n{character.Name} **missed** {curCharacter.Name} and was counter attacked for **{BaseDamage}** damage.";
                    }
                    else
                    {
                        result +=
                            $"\n{character.Name} **missed** {curCharacter.Name} but {curCharacter.Name} does not have a ranged attack.";
                    }
                }
                else if (roll < 10)
                {
                    curCharacter.TakeDamage(BaseDamage);
                    result += $"\n{character.Name} **hit** {curCharacter.Name} and did **{BaseDamage}** damage.";
                }
                else
                {
                    curCharacter.TakeDamage(BaseDamage * 2);
                    result += $"\n{character.Name} **critically hit** {curCharacter.Name} and did **{BaseDamage * 2}** damage.";
                }

                if (secondary == -1)
                {
                    result += $" (rolled: {roll})";
                }
                else
                {
                    result += $" (rolled: {roll} and {secondary}";
                }

                if (character.Health <= 0)
                {
                    character.Actionpoints = 0;
                    return result + "\n" + character.Name + " is out of the battle. " + Emotes.Dead;
                }
                if (curCharacter.Health <= 0)
                {
                    curCharacter.Actionpoints = 0;
                    result += "\n" + curCharacter.Name + " is out of the battle. " + Emotes.Dead;
                }
            }

            if (character.Actionpoints > 0)
                result += "\n" + character.Actionpoints + " action points remaining.";

            return result;
        }

        public override string Dot(Character character, Character[] targets)
        {
            return "Not implemented. " + Emotes.Confused;
        }

        public override string Stun(Character character, Character[] targets)
        {
            return "Not implemented. " + Emotes.Confused;
        }

        public override string Heal(Character character, Character[] targets)
        {
            Random rnd = new Random();

            int requiredPoints;
            requiredPoints = targets.Length > 1 ? 2 : 1;

            if (character.Actionpoints < requiredPoints)
                return "Insufficient action points. " + Emotes.Annoyed;
            character.Action(requiredPoints);

            if (character.Blocking > 0)
            {
                character.Blocking = 0;
            }

            string result = character.Name + " attempts to heal " + Utilities.AppendNames(targets) + ":";

            for (int i = 0; i < targets.Length; i++)
            {
                int roll = rnd.Next(1, 10+1);

                if (roll < 4)
                {
                    result += $"\n{character.Name} **missed** {targets[i].Name}. (rolled: {roll})";
                }
                else if (roll < 10)
                {
                    targets[i].Heal(BaseHeal);
                    result += $"\n{character.Name} **healed** {targets[i].Name} for **{BaseHeal}** health. (rolled: {roll})";
                }
                else
                {
                    targets[i].Heal(BaseHeal * 2);
                    result += $"\n{character.Name} **critically healed** {targets[i].Name} for **{BaseHeal * 2}** health. (rolled: {roll})";
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
                return "Insufficient action points. " + Emotes.Annoyed;
            character.Action(requiredPoints);

            if (character.Blocking > 0)
            {
                character.Blocking = 0;
            }

            string result = character.Name + " attempts to ward " + Utilities.AppendNames(targets) + ":";

            for (int i = 0; i < targets.Length; i++)
            {
                var roll = rnd.Next(1, 10+1);

                if (roll < 4)
                {
                    result += "\n" + character.Name + " **missed** " + targets[i].Name + ". (rolled: " + roll + ")";
                }
                else if (roll < 10)
                {
                    targets[i].Ward(new Ward(WardHealth, WardDuration));
                    result += "\n" + character.Name + " **warded** " + targets[i].Name + ". (rolled: " + roll + ")";
                }
                else
                {
                    targets[i].Ward(new Ward(WardHealth * 2, WardDuration * 2));
                    result += "\n" + character.Name + " **critically warded** " + targets[i].Name + ". (rolled: " + roll + ")";
                }
            }

            if (character.Actionpoints > 0)
                result += "\n" + character.Actionpoints + " action points remaining.";

            return result;
        }

        public override string Block(Character character)
        {
            if (character.Actionpoints < 2)
                return "Insufficient action points. " + Emotes.Annoyed;
            character.Action(2);

            character.Blocking = WardDuration+1;
            string result = character.Name + " raised their block.";

            if (character.Actionpoints > 0)
                result += "\n" + character.Actionpoints + " action points remaining.";

            return result;
        }

        public override string PotionHealth(Character character)
        {
            if (character.HealthPotions <= 0)
            {
                return character.Name + " does not have any health potions left." + Emotes.Annoyed;
            }

            if (character.Actionpoints < 2)
                return "Insufficient action points. " + Emotes.Annoyed;
            character.Action(2);

            if (character.Blocking > 0)
            {
                character.Blocking = 0;
            }

            string result = character.Name + " used a health potion.";
            character.Health += 1;
            character.HealthPotions--;
            result += "\n" + character.HealthPotions + " health potions remaining.";

            if (character.Actionpoints > 0)
                result += "\n" + character.Actionpoints + " action points remaining.";

            return result;
        }

        public override string AddCharacter(Character character)
        {
            Random rnd = new Random();

            if (PotionHealthRollMax > 0)
            {
                int result = rnd.Next(PotionHealthRollMin, PotionHealthRollMax+1);
                character.HealthPotions = result;

                return character.Name + " starts with " + result + " health potion(s).";
            }

            return "";
        }
    }

    [Serializable]
    internal class Amentia2Ruleset : AmentiaRuleset
    {

        public Amentia2Ruleset()
        {
            Name = "Amentia's Second Ruleset";
        }

        public override string Attack(Character character, Character[] targets)
        {
            if (character.Actionpoints < 2)
            {
                if (character.Health <= 0)
                {
                    return character.Name + " is out of the battle." + Emotes.Annoyed;
                }

                return "Insufficient action points." + Emotes.Annoyed;
            }
            character.Action(2);

            if (character.Blocking > 0)
            {
                character.Blocking = 0;
            }

            string result = character.Name + " attempts to attack " + Utilities.AppendNames(targets) + ":";

            for (int i = 0; i < targets.Length; i++)
            {
                result += "\n" + AttemptAttack(character, targets[i]);
                if (character.Health == 0)
                {
                    return result;
                }
                if (targets[i].Health != 0)
                {
                    result += "\n" + AttemptAttack(targets[i], character);
                }
            }

            if (character.Actionpoints > 0)
                result += "\n" + character.Actionpoints + " action points remaining.";

            return result;
        }

        private string AttemptAttack(Character character, Character target)
        {
            string status = "";
            Random rnd = new Random();
            int roll = rnd.Next(1, 10+1);

            if (roll <= 2)
            {
                character.TakeDamage(2);
                status += character.Name + " **critically failed** " + target.Name + " and took **2** damage. (rolled: " + roll + ")";
            }
            else if (roll <= 5)
            {
                status += character.Name + " **missed** " + target.Name + ".";
            }
            else if (roll < 9)
            {
                target.TakeDamage(1);
                status += character.Name + " **hit** " + target.Name + " and did **1** damage. (rolled: " + roll + ")";
            }
            else
            {
                target.TakeDamage(2);
                status += character.Name + " **critically hit** " + target.Name + " and did **2** damage. (rolled: " + roll + ")";
            }

            if (character.Health <= 0)
            {
                character.Actionpoints = 0;
                status += "\n" + character.Name + " is out of the battle. " + Emotes.Dead;
            }
            else if (target.Health <= 0)
            {
                target.Actionpoints = 0;
                status += "\n" + target.Name + " is out of the battle. " + Emotes.Dead;
            }

            return status;

        }
    }
}
