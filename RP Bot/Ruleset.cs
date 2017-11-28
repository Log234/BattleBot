using System;
using System.Collections.Generic;
using System.Text;

namespace RP_Bot
{
    abstract class Ruleset
    {
        string name;
        bool waitOnJoin; // Whether characters that join mid-fight has to wait a round
        bool aoeAttacks; // Whether AoE attacks are allowed
        bool aoeRanged;  // Whether AoE ranged attacks are allowed
        bool aoeHeals;   // Whether AoE heals are allowed
        bool aoeWards;   // Whether AoE wards are allowed


    }
}
