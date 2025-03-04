﻿using HarmonyLib;
using MoreShipUpgrades.Misc.Util;
using MoreShipUpgrades.UpgradeComponents.OneTimeUpgrades.Store;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.Ship;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MoreShipUpgrades.Patches.RoundComponents
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal static class TimeOfDayTranspilerPatcher
    {
        [HarmonyPatch(nameof(TimeOfDay.SetBuyingRateForDay))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> SetBuyingRateForDayTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo sigurdChance = typeof(Sigurd).GetMethod(nameof(Sigurd.GetBuyingRate));
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindFloat(ref index, ref codes, findValue: 1, addCode: sigurdChance, errorMessage: "Couldn't find the 1 value which is used as buying rate");
            Tools.FindFloat(ref index, ref codes, findValue: 0.3f, addCode: sigurdChance, errorMessage: "Couldn't find the 0.3f value which is used as buying rate");
            return codes.AsEnumerable();
        }

        [HarmonyPatch(nameof(TimeOfDay.SyncNewProfitQuotaClientRpc))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> SyncNewProfitQuotaClientRpcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo sigurdChance = typeof(Sigurd).GetMethod(nameof(Sigurd.GetBuyingRate));
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindFloat(ref index, ref codes, findValue: 0.3f, addCode: sigurdChance, errorMessage: "Couldn't find the 0.3 value which is used as buying rate");
            return codes.AsEnumerable();
        }

        [HarmonyPatch(nameof(TimeOfDay.MoveGlobalTime))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> MoveGlobalTimeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo getGlobalSpeedMultiplier = typeof(QuantumDisruptor).GetMethod(nameof(QuantumDisruptor.GetGlobalSpeedMultiplier));
            FieldInfo globalSpeedMultiplier = typeof(TimeOfDay).GetField(nameof(TimeOfDay.globalTimeSpeedMultiplier));

            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindField(ref index, ref codes, findField: globalSpeedMultiplier, addCode: getGlobalSpeedMultiplier, errorMessage: "Couldn't find occurence of global speed multiplier field");
            return codes;
        }
    }
}
