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
            PatchAgentMaximumSpeedWhenChasing(ref index, ref codes);
            return codes;
        }
        [HarmonyPatch(nameof(JesterAI.DoAIInterval))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DoAIIntervalTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            PatchAgentSpeedAfterStun(ref index, ref codes);
            return codes;
        }
        private static void PatchAgentMaximumSpeedWhenChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: MAXIMUM_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find agent maximum speed when chasing");
        }
        private static void PatchAgentSpeedAfterStun(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: INITIAL_SPEED_AFTER_STUN, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find agent maximum speed when chasing");
        }
    }
}
