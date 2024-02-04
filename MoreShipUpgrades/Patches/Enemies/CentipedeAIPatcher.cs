using HarmonyLib;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(CentipedeAI))]
    internal class CentipedeAIPatcher
    {
        const float MAXIMUM_SPEED = 5.5f;
        [HarmonyPatch(nameof(CentipedeAI.IncreaseSpeedSlowly))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> IncreaseSpeedSlowlyTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            PatchAgentMaximumSpeedWhenChasing(ref index, ref codes);
            return codes;
        }
        private static void PatchAgentMaximumSpeedWhenChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: MAXIMUM_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find maximum agent speed when chasing");
        }
    }
}
