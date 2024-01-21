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
            index = PatchAngetMaximumSpeedWhenCarryingBody(index, ref codes);
            index = PatchAngetMaximumSpeedWhenPatrolling(index, ref codes);
            index = PatchAngetMaximumSpeedWhenPatrolling(index, ref codes);
            index = PatchAngetMaximumSpeedWhenChasing(index, ref codes);
            return codes;
        }

        [HarmonyPatch(nameof(FlowermanAI.EnterAngerModeClientRpc))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> EnterAngerModeClientRpcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            index = PatchAgentSpeedWhenAngry(index, ref codes);
            return codes;
        }
        static int PatchAngetMaximumSpeedWhenChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, MAXIMUM_ANGRY_SPEED, checkForBarbedWire, true, "Couldn't find agent maximum speed when chasing");
        }
        static int PatchAngetMaximumSpeedWhenPatrolling(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, PATROL_SPEED, checkForBarbedWire, true, "Couldn't find agent maximum speed when patrolling");
        }
        static int PatchAngetMaximumSpeedWhenCarryingBody(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, MAXIMUM_CARRYING_SPEED, checkForBarbedWire, true, "Couldn't find maximum agent speed when carrying body");
        }
        static int PatchAgentSpeedWhenAngry(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            return Tools.LookForFloat(index, ref codes, INITIAL_ANGRY_SPEED, checkForBarbedWire, true, "Couldn't find agent speed when entering angry mode");
        }
    }
}
