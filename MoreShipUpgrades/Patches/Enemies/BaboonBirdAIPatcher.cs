using HarmonyLib;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(BaboonBirdAI))]
    internal class BaboonBirdAIPatcher
    {
        const float PATROL_SPEED = 10f;
        const float CHASE_SPEED = 12f;
        const float COMFORTABLE_SPEED = 9f;
        [HarmonyPatch(nameof(BaboonBirdAI.DoAIInterval))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DoAIIntervalTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            index = PatchAgentSpeedWhenPatrolling(index, ref codes);
            index = PatchAgentSpeedWhenChasing(index, ref codes);
            index = PatchAgentSpeedWhenGettingComfortable(index, ref codes);
            index = PatchAgentSpeedWhenGettingComfortable(index, ref codes);
            return codes;
        }
        private static int PatchAgentSpeedWhenGettingComfortable(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, COMFORTABLE_SPEED, checkForBarbedWire, true, "Couldn't find agent speed when waking up");
        }
        private static int PatchAgentSpeedWhenChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, CHASE_SPEED, checkForBarbedWire, true, "Couldn't find agent speed when chasing");
        }
        private static int PatchAgentSpeedWhenPatrolling(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, PATROL_SPEED, checkForBarbedWire, true, "Couldn't find agent speed when patrolling");
        }
    }
}
