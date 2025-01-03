﻿using HarmonyLib;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.UpgradeComponents.Items.RadarBooster;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.Items.RadarBooster;
using UnityEngine;

namespace MoreShipUpgrades.Patches.Items
{
    [HarmonyPatch(typeof(RadarBoosterItem))]
    internal static class RadarBoosterPatcher
    {
        [HarmonyPatch(nameof(RadarBoosterItem.Start))]
        [HarmonyPostfix]
        static void StartPostifx(RadarBoosterItem __instance)
        {
            if (!UpgradeBus.Instance.PluginConfiguration.ChargingBoosterConfiguration.Enabled.Value) return;
            if (!BaseUpgrade.GetActiveUpgrade(ChargingBooster.UPGRADE_NAME)) return;
            __instance.gameObject.AddComponent<ChargingStationManager>();

        }
    }
}
