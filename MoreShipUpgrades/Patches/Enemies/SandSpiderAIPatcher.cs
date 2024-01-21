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
            index = PatchAgentSpeedWhenPatrolling(index, ref codes);
            index = PatchAgentSpeedWhenOnWall(index, ref codes);
            index = PatchAgentSpeedWhenOnWallStartChase(index, ref codes);
            index = PatchAgentSpeedWhenChasing(index, ref codes);
            index = PatchAgentSpeedWhenInjuredChasing(index, ref codes);
            index = PatchAgentSpeedWhenAlmostDeadChasing(index, ref codes);
            index = PatchAgentSpeedWhenHittingPlayer(index, ref codes);
            return codes;
        }
        static int PatchAgentSpeedWhenHittingPlayer(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            index = Tools.LookForFloat(index, ref codes, HITTING_PLAYER_SPEED, checkForBarbedWire, true, "Couldn't find the agent speed for spider when hitting player");
            return index = Tools.LookForFloat(index, ref codes, SPIDER_HITTING_PLAYER_SPEED, checkForBarbedWire, true, "Couldn't find the spider speed for spider when hitting player");
        }
        static int PatchAgentSpeedWhenAlmostDeadChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            index = Tools.LookForFloat(index, ref codes, ALMOST_DEAD_CHASE_SPEED, checkForBarbedWire, true, "Couldn't find the agent speed for spider when chasing in 1 health");
            return index = Tools.LookForFloat(index, ref codes, ALMOST_DEAD_CHASE_SPEED, checkForBarbedWire, true, "Couldn't find the spider speed for spider when chasing in 1 health");
        }
        static int PatchAgentSpeedWhenInjuredChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            index = Tools.LookForFloat(index, ref codes, INJURED_CHASE_SPEED, checkForBarbedWire, true, "Couldn't find the agent speed for spider when chasing in 2 health");
            return index = Tools.LookForFloat(index, ref codes, INJURED_CHASE_SPEED, checkForBarbedWire, true, "Couldn't find the spider speed for spider when chasing in 2 health");
        }
        static int PatchAgentSpeedWhenChasing(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            index = Tools.LookForFloat(index, ref codes, CHASE_SPEED, checkForBarbedWire, true, "Couldn't find the agent speed for spider when chasing");
            return index = Tools.LookForFloat(index, ref codes, CHASE_SPEED, checkForBarbedWire, true, "Couldn't find the spider speed for spider when chasing");
        }
        static int PatchAgentSpeedWhenOnWallStartChase(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            index = Tools.LookForFloat(index, ref codes, GETTING_OFF_WALL_SPEED, checkForBarbedWire, true, "Couldn't find the agent speed for spider when on wall and start chasing");
            return index = Tools.LookForFloat(index, ref codes, GETTING_OFF_WALL_SPEED, checkForBarbedWire, true, "Couldn't find the spider speed for spider when on wall and start chasing");
        }
        static int PatchAgentSpeedWhenOnWall(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            index = Tools.LookForFloat(index, ref codes, PATROL_SPEED, checkForBarbedWire, true, "Couldn't find the agent speed for spider when on wall");
            return index = Tools.LookForFloat(index, ref codes, PATROL_SPEED, checkForBarbedWire, true, "Couldn't find the spider speed for spider when on wall");
        }
        static int PatchAgentSpeedWhenPatrolling(int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            index = Tools.LookForFloat(index, ref codes, WALL_SPEED, checkForBarbedWire, true, "Couldn't find the agent speed for spider when patrolling");
            return index = Tools.LookForFloat(index, ref codes, SPIDER_WALL_SPEED, checkForBarbedWire, true, "Couldn't find the spider speed for spider when patrolling");
        }
    }
}
