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
            index = PatchAgentRunningSpeedWhenPatrolling(index, ref codes);
            index = PatchAgentWalkingSpeedWhenPatrolling(index, ref codes);
            index = PatchAgentRunningSpeedWhenChasing(index, ref codes);
            index = PatchAgentWalkingSpeedWhenPatrolling(index, ref codes);
            index = PatchAgentWalkingSpeedWhenHidingInShip(index, ref codes);
            return codes;
        }
        static int PatchAgentWalkingSpeedWhenHidingInShip(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.FindFloat(index, ref codes, findValue: TOWARDS_SHIP_SPEED, addCode: checkForBarbedWires, requireInstance: true, errorMessage: "Couldn't find agent running speed when patrolling");
        }
        static int PatchAgentRunningSpeedWhenChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.FindFloat(index, ref codes, findValue: CHASE_RUNNING_SPEED, addCode: checkForBarbedWires, requireInstance: true, errorMessage: "Couldn't find agent running speed when patrolling");
        }
        static int PatchAgentWalkingSpeedWhenPatrolling(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.FindFloat(index, ref codes, findValue: WALKING_SPEED, addCode: checkForBarbedWires, requireInstance: true, errorMessage: "Couldn't find agent running speed when patrolling");
        }

        static int PatchAgentRunningSpeedWhenPatrolling(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.FindFloat(index, ref codes, findValue: PATROL_RUNNING_SPEED, addCode: checkForBarbedWires, requireInstance: true, errorMessage: "Couldn't find agent running speed when patrolling");
        }
    }
}
