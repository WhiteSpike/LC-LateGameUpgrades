using HarmonyLib;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(FlowermanAI))]
    internal class FlowermanAIPatcher
    {
        const float INITIAL_ANGRY_SPEED = 9f;
        const float MAXIMUM_ANGRY_SPEED = 12f;
        const float MAXIMUM_CARRYING_SPEED = INITIAL_ANGRY_SPEED;
        const float PATROL_SPEED = 6f;

        [HarmonyPatch(nameof(FlowermanAI.Update))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            PatchAngetMaximumSpeedWhenCarryingBody(ref index, ref codes);
            PatchAngetMaximumSpeedWhenPatrolling(ref index, ref codes);
            PatchAngetMaximumSpeedWhenPatrolling(ref index, ref codes);
            PatchAngetMaximumSpeedWhenChasing(ref index, ref codes);
            return codes;
        }

        [HarmonyPatch(nameof(FlowermanAI.EnterAngerModeClientRpc))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> EnterAngerModeClientRpcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            PatchAgentSpeedWhenAngry(ref index, ref codes);
            return codes;
        }
        static void PatchAngetMaximumSpeedWhenChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: MAXIMUM_ANGRY_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find agent maximum speed when chasing");
        }
        static void PatchAngetMaximumSpeedWhenPatrolling(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: PATROL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find agent maximum speed when patrolling");
        }
        static void PatchAngetMaximumSpeedWhenCarryingBody(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: MAXIMUM_CARRYING_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find maximum agent speed when carrying body");
        }
        static void PatchAgentSpeedWhenAngry(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: INITIAL_ANGRY_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find agent speed when entering angry mode");
        }
    }
}
