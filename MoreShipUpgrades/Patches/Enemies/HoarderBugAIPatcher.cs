using HarmonyLib;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using MoreShipUpgrades.Misc;
using System.Diagnostics.CodeAnalysis;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(HoarderBugAI))]
    internal class HoarderBugAIPatcher
    {
        static LGULogger logger = new LGULogger(nameof(HoarderBugAIPatcher));
        const float PATROL_SPEED = 6f;
        const float CHASE_SPEED = 18f;
        [HarmonyPatch(nameof(HoarderBugAI.Update))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            PatchAgentSpeedWhenGettingItem(ref index, ref codes);
            PatchAgentSpeedWhenPatrolling(ref index, ref codes);
            PatchMinimumAgentSpeedWhenChasing(ref index, ref codes);
            return codes;
        }

        static void PatchAgentSpeedWhenGettingItem(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: PATROL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Could not find the agent speed when getting item");
        }
        static void PatchAgentSpeedWhenPatrolling(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: PATROL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Could not find the agent speed when patrolling");
        }
        static void PatchMinimumAgentSpeedWhenChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: CHASE_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Could not find the minimum agent speed when chasing");
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(HoarderBugAI.IsHoarderBugAngry))]
        private static void MakeHoarderBugSwarmAngry(ref bool __result)
        {
            if (UpgradeBus.instance.contractType != "exterminator") return;

            if (UpgradeBus.instance.contractLevel == RoundManager.Instance.currentLevel.PlanetName)
            {
                __result = true;
            }
        }
    }

}
