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
            index = PatchAgentSpeedWhenPatrol(index, ref codes);
            index = PatchAgentSpeedWhenSuspicious(index, ref codes);
            index = PatchMinimumAgentSpeedWhenChasing(index, ref codes);
            index = PatchMaximumAgentSpeedWhenChasing(index, ref codes);
            return codes;
        }

        [HarmonyPatch("EnterLunge")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> EnterLungeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int index = 0;
            index = PatchMinimumAgentSpeedWhenChasing(index, ref codes);
            return codes;
        }

        static int PatchAgentSpeedWhenPatrol(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.FindFloat(index, ref codes, findValue: PATROL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Could not find the agent speed when patrolling");
        }
        static int PatchAgentSpeedWhenSuspicious(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.FindFloat(index, ref codes, findValue: SUSPICIOUS_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Could not find the agent speed when suspicious");
        }
        static int PatchMinimumAgentSpeedWhenChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.FindFloat(index, ref codes, findValue: MINIMUM_CHASING_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Could not find the minimum agent speed when chasing");
        }
        static int PatchMaximumAgentSpeedWhenChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.FindFloat(index, ref codes, findValue: MAXIMUM_CHASING_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Could not find the maximum agent speed when chasing");
        }
    }
}
