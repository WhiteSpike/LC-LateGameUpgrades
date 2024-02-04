using HarmonyLib;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(MouthDogAI))]
    internal class MouthDogAIPatcher
    {
        static LGULogger logger = new LGULogger(nameof(MouthDogAIPatcher));
        const float PATROL_SPEED = 3.5f;
        const float SUSPICIOUS_SPEED = 4.5f;
        const float MINIMUM_CHASING_SPEED = 13f;
        const float MAXIMUM_CHASING_SPEED = 18f;
        [HarmonyPatch(nameof(MouthDogAI.Update))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int index = 0;
            PatchAgentSpeedWhenPatrol(ref index, ref codes);
            PatchAgentSpeedWhenSuspicious(ref index, ref codes);
            PatchMinimumAgentSpeedWhenChasing(ref index, ref codes);
            PatchMaximumAgentSpeedWhenChasing(ref index, ref codes);
            return codes;
        }

        [HarmonyPatch(nameof(MouthDogAI.EnterLunge))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> EnterLungeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int index = 0;
            PatchMinimumAgentSpeedWhenChasing(ref index, ref codes);
            return codes;
        }

        static void PatchAgentSpeedWhenPatrol(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: PATROL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Could not find the agent speed when patrolling");
        }
        static void PatchAgentSpeedWhenSuspicious(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: SUSPICIOUS_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Could not find the agent speed when suspicious");
        }
        static void PatchMinimumAgentSpeedWhenChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: MINIMUM_CHASING_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Could not find the minimum agent speed when chasing");
        }
        static void PatchMaximumAgentSpeedWhenChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: MAXIMUM_CHASING_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Could not find the maximum agent speed when chasing");
        }
    }
}
