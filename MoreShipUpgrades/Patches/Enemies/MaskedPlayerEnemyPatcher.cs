using HarmonyLib;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MaskedPlayerEnemyPatcher
    {
        const float PATROL_RUNNING_SPEED = 7f;
        const float WALKING_SPEED = 3.8f;
        const float CHASE_RUNNING_SPEED = 8f;
        const float TOWARDS_SHIP_SPEED = 5f;
        [HarmonyPatch(nameof(MaskedPlayerEnemy.Update))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            PatchAgentRunningSpeedWhenPatrolling(ref index, ref codes);
            PatchAgentWalkingSpeedWhenPatrolling(ref index, ref codes);
            PatchAgentRunningSpeedWhenChasing(ref index, ref codes);
            PatchAgentWalkingSpeedWhenPatrolling(ref index, ref codes);
            PatchAgentWalkingSpeedWhenHidingInShip(ref index, ref codes);
            return codes;
        }
        static void PatchAgentWalkingSpeedWhenHidingInShip(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: TOWARDS_SHIP_SPEED, addCode: checkForBarbedWires, requireInstance: true, errorMessage: "Couldn't find agent running speed when patrolling");
        }
        static void PatchAgentRunningSpeedWhenChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: CHASE_RUNNING_SPEED, addCode: checkForBarbedWires, requireInstance: true, errorMessage: "Couldn't find agent running speed when patrolling");
        }
        static void PatchAgentWalkingSpeedWhenPatrolling(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: WALKING_SPEED, addCode: checkForBarbedWires, requireInstance: true, errorMessage: "Couldn't find agent running speed when patrolling");
        }

        static void PatchAgentRunningSpeedWhenPatrolling(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: PATROL_RUNNING_SPEED, addCode: checkForBarbedWires, requireInstance: true, errorMessage: "Couldn't find agent running speed when patrolling");
        }
    }
}
