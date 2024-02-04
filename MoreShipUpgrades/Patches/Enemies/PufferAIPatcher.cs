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
            PatchAgentSpeedWhenPatrolling(ref index, ref codes);
            PatchAgentSpeedWhenPatrolling(ref index, ref codes);
            PatchAgentMaximumSpeedWhenRunning(ref index, ref codes);
            PatchAgentMaximumSpeedWhenRunning(ref index, ref codes);
            return codes;
        }
        private static void PatchAgentSpeedWhenPatrolling(ref int index, ref List<CodeInstruction> codes) 
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: PATROL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find agent speed when patrolling");
        }
        private static void PatchAgentMaximumSpeedWhenRunning(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: MAXIMUM_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find agent maximum speed when running");
        }
    }
}
