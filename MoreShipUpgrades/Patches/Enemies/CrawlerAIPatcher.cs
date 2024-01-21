using HarmonyLib;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(CrawlerAI))]
    internal class CrawlerAIPatcher
    {
        const float ENTERING_CHASE_SPEED = 7f;
        const float PATROL_SPEED = 8f;
        const float MAXIMUM_CHASE_SPEED = 16f;
        const float MAXIMUM_ACCELERATION = 80f;
        [HarmonyPatch(nameof(CrawlerAI.Update))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            index = PatchAgentSpeedWhenEnteringChase(index, ref codes);
            return codes;
        }

        [HarmonyPatch("CalculateAgentSpeed")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CalculateAgentSpeedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            index = PatchAgentSpeedWhenPatrolling(index, ref codes);
            index = PatchAgentMaximumSpeedWhenChasing(index, ref codes);
            index = PatchAgentMaximumAccelerationWhenChasing(index, ref codes);
            return codes;
        }
        static int PatchAgentMaximumAccelerationWhenChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, MAXIMUM_ACCELERATION, checkForBarbedWires, true, "Couldn't find the maximum agent acceleration when chasing");
        }
        static int PatchAgentMaximumSpeedWhenChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, MAXIMUM_CHASE_SPEED, checkForBarbedWires, true, "Couldn't find the maximum agent speed when chasing");
        }

        static int PatchAgentSpeedWhenPatrolling(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, PATROL_SPEED, checkForBarbedWires, true, "Couldn't find the agent speed when chasing");
        }

        static int PatchAgentSpeedWhenEnteringChase(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, ENTERING_CHASE_SPEED, checkForBarbedWires, true, "Couldn't find the agent speed when patrolling");
        }
    }
}
