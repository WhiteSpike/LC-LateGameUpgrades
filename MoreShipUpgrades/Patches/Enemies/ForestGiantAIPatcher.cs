using HarmonyLib;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(ForestGiantAI))]
    internal class ForestGiantAIPatcher
    {
        static LGULogger logger = new LGULogger(nameof(ForestGiantAIPatcher));
        const float PATROL_SPEED = 5f;
        const float MAXIMUM_CHASE_SPEED = 7f;
        [HarmonyPatch(nameof(ForestGiantAI.Update))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            index = Tools.SkipFloat(index, ref codes, PATROL_SPEED);
            index = PatchAgentSpeedWhenPatrolling(index, ref codes);
            index = PatchAgentMaximumSpeedWhenChasing(index, ref codes);
            index = PatchAgentMaximumSpeedWhenChasing(index, ref codes);
            return codes;
        }
        static int PatchAgentMaximumSpeedWhenChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, MAXIMUM_CHASE_SPEED, checkForBarbedWire, true, "Couldn't find agent speed when chasing");
        }
        static int PatchAgentSpeedWhenPatrolling(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, PATROL_SPEED, checkForBarbedWire, true, "Couldn't find agent speed when patrolling");
        }
    }
}
