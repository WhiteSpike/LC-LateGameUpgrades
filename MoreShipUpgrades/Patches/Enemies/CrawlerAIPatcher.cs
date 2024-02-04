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
        const float MAXIMUM_ACCELERATION = 40f;
        [HarmonyPatch(nameof(CrawlerAI.Update))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            PatchAgentSpeedWhenEnteringChase(ref index, ref codes);
            return codes;
        }

        [HarmonyPatch(nameof(CrawlerAI.CalculateAgentSpeed))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CalculateAgentSpeedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            PatchAgentSpeedWhenPatrolling(ref index, ref codes);
            PatchAgentMaximumSpeedWhenChasing(ref index, ref codes);
            PatchAgentMaximumAccelerationWhenChasing(ref index, ref codes);
            return codes;
        }
        static void PatchAgentMaximumAccelerationWhenChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: MAXIMUM_ACCELERATION, addCode: checkForBarbedWires, requireInstance: true, errorMessage: "Couldn't find the maximum agent acceleration when chasing");
        }
        static void PatchAgentMaximumSpeedWhenChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: MAXIMUM_CHASE_SPEED, addCode: checkForBarbedWires, requireInstance: true, errorMessage: "Couldn't find the maximum agent speed when chasing");
        }

        static void PatchAgentSpeedWhenPatrolling(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: PATROL_SPEED, addCode: checkForBarbedWires, requireInstance: true, errorMessage: "Couldn't find the agent speed when chasing");
        }

        static void PatchAgentSpeedWhenEnteringChase(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWires = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: ENTERING_CHASE_SPEED, addCode: checkForBarbedWires, requireInstance: true, errorMessage: "Couldn't find the agent speed when patrolling");
        }
    }
}
