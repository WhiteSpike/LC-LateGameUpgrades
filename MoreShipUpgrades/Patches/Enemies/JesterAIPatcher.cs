using HarmonyLib;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(JesterAI))]
    internal class JesterAIPatcher
    {
        const float INITIAL_SPEED_AFTER_STUN = 5f;
        const float MAXIMUM_SPEED = 18f;
        [HarmonyPatch(nameof(JesterAI.Update))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            index = PatchAgentMaximumSpeedWhenChasing(index, ref codes);
            return codes;
        }
        [HarmonyPatch(nameof(JesterAI.DoAIInterval))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DoAIIntervalTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            index = PatchAgentSpeedAfterStun(index, ref codes);
            return codes;
        }
        private static int PatchAgentMaximumSpeedWhenChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, MAXIMUM_SPEED, checkForBarbedWire, true, "Couldn't find agent maximum speed when chasing");
        }
        private static int PatchAgentSpeedAfterStun(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, INITIAL_SPEED_AFTER_STUN, checkForBarbedWire, true, "Couldn't find agent maximum speed when chasing");
        }
    }
}
