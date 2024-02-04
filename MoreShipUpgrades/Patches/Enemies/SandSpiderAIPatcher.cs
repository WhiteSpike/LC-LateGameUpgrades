using HarmonyLib;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(SandSpiderAI))]
    internal class SandSpiderAIPatcher
    {
        const float PATROL_SPEED = 4.25f;
        const float WALL_SPEED = 4.5f;
        const float SPIDER_WALL_SPEED = 3.75f;
        const float GETTING_OFF_WALL_SPEED = 4.25f;

        const float CHASE_SPEED = 4.3f;
        const float INJURED_CHASE_SPEED = 4.56f;
        const float ALMOST_DEAD_CHASE_SPEED = 5f;

        const float HITTING_PLAYER_SPEED = 0.7f;
        const float SPIDER_HITTING_PLAYER_SPEED = 0.4f;

        [HarmonyPatch(nameof(SandSpiderAI.Update))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            PatchAgentSpeedWhenOnWall(ref index, ref codes);
            PatchAgentSpeedWhenPatrolling(ref index, ref codes);
            PatchAgentSpeedWhenOnWallStartChase(ref index, ref codes);
            PatchAgentSpeedWhenChasing(ref index, ref codes);
            PatchAgentSpeedWhenInjuredChasing(ref index, ref codes);
            PatchAgentSpeedWhenAlmostDeadChasing(ref index, ref codes);
            PatchAgentSpeedWhenHittingPlayer(ref index, ref codes);
            return codes;
        }
        static void PatchAgentSpeedWhenHittingPlayer(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: HITTING_PLAYER_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the agent speed for spider when hitting player");
            Tools.FindFloat(ref index, ref codes, findValue: SPIDER_HITTING_PLAYER_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the spider speed for spider when hitting player");
        }
        static void PatchAgentSpeedWhenAlmostDeadChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: ALMOST_DEAD_CHASE_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the agent speed for spider when chasing in 1 health");
            Tools.FindFloat(ref index, ref codes, findValue: ALMOST_DEAD_CHASE_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the spider speed for spider when chasing in 1 health");
        }
        static void PatchAgentSpeedWhenInjuredChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: INJURED_CHASE_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the agent speed for spider when chasing in 2 health");
            Tools.FindFloat(ref index, ref codes, findValue: INJURED_CHASE_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the spider speed for spider when chasing in 2 health");
        }
        static void PatchAgentSpeedWhenChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: CHASE_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the agent speed for spider when chasing");
            Tools.FindFloat(ref index, ref codes, findValue: CHASE_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the spider speed for spider when chasing");
        }
        static void PatchAgentSpeedWhenOnWallStartChase(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: GETTING_OFF_WALL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the agent speed for spider when on wall and start chasing");
            Tools.FindFloat(ref index, ref codes, findValue: GETTING_OFF_WALL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the spider speed for spider when on wall and start chasing");
        }
        static void PatchAgentSpeedWhenOnWall(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: PATROL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the agent speed for spider when on wall");
            Tools.FindFloat(ref index, ref codes, findValue: PATROL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the spider speed for spider when on wall");
        }
        static void PatchAgentSpeedWhenPatrolling(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: WALL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the agent speed for spider when patrolling");
            Tools.FindFloat(ref index, ref codes, findValue: SPIDER_WALL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the spider speed for spider when patrolling");
        }
    }
}
