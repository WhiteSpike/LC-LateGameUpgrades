﻿using HarmonyLib;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(ForestGiantAI))]
    internal class ForestGiantAIPatcher
    {
        static LGULogger logger = new LGULogger(nameof(ForestGiantAIPatcher));
        const float PATROL_SPEED = 5f;
        const float MAXIMUM_CHASE_SPEED = 7f;
        [HarmonyPatch(nameof(ForestGiantAI.Update))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            Tools.FindFloat(ref index, ref codes, findValue: PATROL_SPEED, skip: true);
            PatchAgentSpeedWhenPatrolling(ref index, ref codes);
            PatchAgentMaximumSpeedWhenChasing(ref index, ref codes);
            PatchAgentMaximumSpeedWhenChasing(ref index, ref codes);
            return codes;
        }
        static void PatchAgentMaximumSpeedWhenChasing(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: MAXIMUM_CHASE_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find agent speed when chasing");
        }
        static void PatchAgentSpeedWhenPatrolling(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            Tools.FindFloat(ref index, ref codes, findValue: PATROL_SPEED, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find agent speed when patrolling");
        }
    }
}