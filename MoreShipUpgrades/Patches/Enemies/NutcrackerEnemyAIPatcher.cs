using HarmonyLib;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(NutcrackerEnemyAI))]
    internal class NutcrackerEnemyAIPatcher
    {
        const float PATROL_SPEED = 5.5f;
        const float CHASE_SPEED = 7f;
        [HarmonyPatch(nameof(NutcrackerEnemyAI.Update))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            index = PatchAgentSpeedWhenPatrolling(index, ref codes);
            index = PatchAgentSpeedWhenChasing(index, ref codes);
            return codes;
        }
        private static int PatchAgentSpeedWhenPatrolling(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.FindFloat(index, ref codes, findValue: PATROL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find agent speed when patrolling");
        }
        private static int PatchAgentSpeedWhenChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.FindFloat(index, ref codes, findValue: CHASE_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find agent speed when patrolling");
        }
    }
}
