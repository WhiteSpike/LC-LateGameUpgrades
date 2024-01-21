using HarmonyLib;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(PufferAI))]
    internal class PufferAIPatcher
    {
        const float PATROL_SPEED = 4f;
        const float MAXIMUM_SPEED = 12f;
        [HarmonyPatch(nameof(PufferAI.Update))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            index = PatchAgentSpeedWhenPatrolling(index, ref codes);
            index = PatchAgentSpeedWhenPatrolling(index, ref codes);
            index = PatchAgentMaximumSpeedWhenRunning(index, ref codes);
            index = PatchAgentMaximumSpeedWhenRunning(index, ref codes);
            return codes;
        }
        private static int PatchAgentSpeedWhenPatrolling(int index, ref List<CodeInstruction> codes) 
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, PATROL_SPEED, checkForBarbedWire, true, "Couldn't find agent speed when patrolling");
        }
        private static int PatchAgentMaximumSpeedWhenRunning(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, MAXIMUM_SPEED, checkForBarbedWire, true, "Couldn't find agent maximum speed when running");
        }
    }
}
